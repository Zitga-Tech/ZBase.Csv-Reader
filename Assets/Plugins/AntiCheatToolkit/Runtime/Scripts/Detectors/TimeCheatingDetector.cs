#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#pragma warning disable 618

#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
#define ACTK_DEBUG_ENABLED
#endif

#define ACTK_DETECTOR_ENABLED
#if ACTK_PREVENT_INTERNET_PERMISSION
#undef ACTK_DETECTOR_ENABLED
#endif

#if !UNITY_WEBGL
#define ACTK_ASYNC
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
#define ACTK_WEBGL_BUILD
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
#define ACTK_ACTUAL_ANDROID_DEVICE
#endif

namespace CodeStage.AntiCheat.Detectors
{
	using Common;

	using System;
	using System.Collections;
	using ObscuredTypes;
#if ACTK_ASYNC
	using System.Threading.Tasks;
#endif

#if ACTK_DETECTOR_ENABLED
	using UnityEngine.Networking;
	using Utils;
#endif

	using UnityEngine;
	using UnityEngine.Serialization;

	/// <summary>
	/// Allows to detect time cheating using time from any properly configured server (almost all servers around the web).
	/// </summary>
	/// Requires Internet connection and appropriate 'android.permission.INTERNET' permission when used on Android platform.<br/>
	/// Automatically switches to the current domain on WebGL (if #RequestUrl leads to any external resource) to avoid CORS limitation.<br/>
	/// Make sure your server returns correct Date response header value in case of problems.
	///
	/// Doesn't detects cheating if there is no Internet connection, should work even with weak connection though.<br/>
	/// Just add it to any GameObject as usual or through the "GameObject > Create Other > Code Stage > Anti-Cheat Toolkit"
	/// menu to get started.<br/>
	/// You can use detector completely from inspector without writing any code except the actual reaction on cheating.
	///
	/// <b>Avoid using detectors from code at the Awake phase</b>.
	[AddComponentMenu(MenuPath + ComponentName)]
	[DisallowMultipleComponent]
	[HelpURL(ACTk.DocsRootUrl + "class_code_stage_1_1_anti_cheat_1_1_detectors_1_1_wall_hack_detector.html")]
	public class TimeCheatingDetector : ACTkDetectorBase<TimeCheatingDetector>
	{
		/// <summary>
		/// Delegate with result of online time receive attempt.
		/// </summary>
		/// <param name="result">OnlineTimeResult instance with details about operation result.</param>
		public delegate void OnlineTimeCallback(OnlineTimeResult result);

		/// <summary>
		/// Delegate with cheat check result.
		/// </summary>
		/// <param name="result">Result of the cheat check.</param>
		/// <param name="error">Kind of occured error, if any.</param>
		public delegate void TimeCheatingDetectorEventHandler(CheckResult result, ErrorKind error);

		/// <summary>
		/// Result of the online time receive attempt.
		/// </summary>
		public struct OnlineTimeResult
		{
			// TODO: make fields private in later versions
			
			[Obsolete("Please use Success property instead")]
			public bool success;

			[Obsolete("Please use Error property instead")]
			public string error;

			[Obsolete("Please use ErrorResponseCode property instead")]
			public long errorResponseCode;

			[Obsolete("Please use OnlineSecondsUtc property instead")]
			public double onlineSecondsUtc;
			
			[Obsolete("Please use OnlineDateTimeUtc property instead")]
			public DateTime onlineDateTimeUtc;

			/// <summary>
			/// Indicates success of the operation.
			/// </summary>
			public bool Success => success;

			/// <summary>
			/// Error text (check if success == false).
			/// </summary>
			public string Error => error;

			/// <summary>
			/// HTTP Response Code for the error.
			/// </summary>
			public long ErrorResponseCode => errorResponseCode;

			/// <summary>
			/// UTC seconds value retrieved from the online server.
			/// </summary>
			public double OnlineSecondsUtc => onlineSecondsUtc;

			/// <summary>
			/// UTC DateTime retrieved from the online server.
			/// </summary>
			public DateTime OnlineDateTimeUtc => onlineDateTimeUtc;

			internal OnlineTimeResult SetTime(double secondsUtc, DateTime dateTimeUtc)
			{
				success = true;
				error = null;
				errorResponseCode = -1;
				onlineSecondsUtc = secondsUtc;
				onlineDateTimeUtc = dateTimeUtc;

				return this;
			}

			internal OnlineTimeResult SetError(string errorText, long responseCode = -1)
			{
				success = false;
				error = errorText;
				errorResponseCode = responseCode;
				onlineSecondsUtc = -1;
				
				return this;
			}

			public override string ToString()
			{
				if (success)
					return "onlineSecondsUtc: " + onlineSecondsUtc;

				return "Error response code: " + errorResponseCode + "\nError: " + error;
			}
		}

		/// <summary>
		/// Describes possible detector check results.
		/// </summary>
		public enum CheckResult
		{
			/// <summary>
			/// Cheat detection was not started yet or is in progress.
			/// </summary>
			Unknown = 0,

			/// <summary>
			/// Cheat check successfully passed and nor cheating nor wrong time was detected.
			/// </summary>
			CheckPassed = 5,

			/// <summary>
			/// Direct time cheating was not detected but difference between local and online UTC clocks exceeds the #wrongTimeThreshold.
			/// </summary>
			WrongTimeDetected = 10,

			/// <summary>
			/// Subsequent measurements difference of local and online time difference exceeded the #realCheatThreshold.
			/// </summary>
			CheatDetected = 15,

			/// <summary>
			/// There was error while making cheat check, please check #LastError for details.
			/// </summary>
			Error = 100
		}

		/// <summary>
		/// Describes possible detector errors.
		/// </summary>
		public enum ErrorKind
		{
			/// <summary>
			/// Indicates there were no any error registered.
			/// </summary>
			NoError = 0,

			/// <summary>
			/// Url set for the online time receiving is not a correct URI.
			/// </summary>
			IncorrectUri = 3,

			/// <summary>
			/// Error while receiving online time (check logs for error details).
			/// </summary>
			OnlineTimeError = 5,

			/// <summary>
			/// Detector was not started yet. It should be started before performing any cheat checks.
			/// </summary>
			NotStarted = 10,

			/// <summary>
			/// Detector already checks for the cheat.
			/// Please make sure #IsCheckingForCheat == false before trying to force another cheat check.
			/// </summary>
			AlreadyCheckingForCheat = 15,

			/// <summary>
			/// Something strange happened, please check logs for details.
			/// </summary>
			Unknown = 100
		}

		/// <summary>
		/// Method of the request to the server. Please consider Head by default and fall back to Get in case of problems.
		/// </summary>
		/// Some servers do not like HEAD requests and sometimes treat it as a malicious bot activity and may temporary block the caller.<br/>
		/// For such servers use GET method as a more compatible yet slower and loading all page data.
		public enum RequestMethod
		{
			/// <summary>
			/// Preferable method as it requests only headers thus it runs faster and has minimal possible traffic.
			/// </summary>
			Head,

			/// <summary>
			/// More compatible method which loads whole content at the given URL.
			/// </summary>
			Get
		}

		public const string ComponentName = "Time Cheating Detector";
		private const string LogPrefix = ACTk.LogPrefix + ComponentName + ": ";
		private const int DefaultTimeoutSeconds = 10;

		protected override string GetComponentName()
		{
			return ComponentName;
		}
#if ACTK_DETECTOR_ENABLED
		private static readonly WaitForEndOfFrame CachedEndOfFrame = new WaitForEndOfFrame();
		private static bool gettingOnlineTime;

#if ACTK_ACTUAL_ANDROID_DEVICE
		private static int sdkLevel;
#endif

		#region public fields and properties

		/// <summary>
		/// Gets called after each cheat check and provides results of the check.
		/// </summary>
		public event TimeCheatingDetectorEventHandler CheatChecked;

		[Header("Request settings")]

		[Tooltip("Absolute URL which will return correct datetime in response headers (you may use popular web servers like google.com, microsoft.com etc.).")]
		[SerializeField]
		private string requestUrl = "https://google.com";

		/// <summary>
		/// Absolute URL which will return correct Date in response header to the HEAD request (nearly any popular web server out there including google.com, microsoft.com etc.).
		/// </summary>
		public string RequestUrl
		{
			get { return requestUrl; }
			set
			{
				if (requestUrl == value || !Application.isPlaying) return;

				requestUrl = value;
				cachedUri = UrlToUri(requestUrl);
			}
		}

		/// <summary>
		/// Method to use for url request. Use Head method if possible and fall back to get if server does not reply or block head requests.
		/// </summary>
		[Tooltip("Method to use for url request. Use Head method if possible and fall back to get if server does not " +
				 "reply or block head requests.")]
		public RequestMethod requestMethod = RequestMethod.Head;

		/// <summary>
		/// Online time request timeout in seconds. Request will be automatically aborted if server will not response in specified time.
		/// </summary>
		[Tooltip("Online time request timeout in seconds.")]
		public int timeoutSeconds = 10;

		/// <summary>
		/// Time (in minutes) between detector checks. Set to 0 to disable automatic time checks and use
		/// ForceCheck(), ForceCheckEnumerator() or ForceCheckTask() to manually run a check.
		/// </summary>
		[Header("Settings in minutes")]
		[Tooltip("Time (in minutes) between detector checks.")]
		[Range(0f, 60)]
		public float interval = 5;

		/// <summary>
		/// Maximum allowed difference between subsequent measurements, in minutes.
		/// </summary>
		[Tooltip("Maximum allowed difference between subsequent measurements, in minutes.")]
		[FormerlySerializedAs("threshold")]
		[Range(10, 180)]
		public int realCheatThreshold = 65;

		/// <summary>
		/// Maximum allowed difference between local and online time, in minutes.
		/// </summary>
		/// If online and local time difference exceed this threshold, detector will raise an event with
		[Tooltip("Maximum allowed difference between local and online time, in minutes.")]
		[Range(1, 180)]
		public int wrongTimeThreshold = 65;

		/// <summary>
		/// Ignore case when time changes to be in sync with online correct time.
		/// </summary>
		/// Wrong time threshold is taken into account.
		[Tooltip("Ignore case when time changes to be in sync with online correct time. " +
				 "Wrong time threshold is taken into account.")]
		public bool ignoreSetCorrectTime = true;

		/// <summary>
		/// Last detector error kind. Will be ErrorKind.NoError if there were no errors.
		/// </summary>
		public ErrorKind LastError { get; private set; }

		/// <summary>
		/// Last check result. Check #LastError if this has value of CheckResult.Error.
		/// </summary>
		public CheckResult LastResult { get; private set; }

		/// <summary>
		/// Allows to check if cheating check is currently in process.
		/// </summary>
		public bool IsCheckingForCheat { get; private set; }

		#endregion

		#region private and protected fields

		private readonly string onlineOfflineDifferencePrefsKey = Base64Utils.ToBase64(ObscuredString.Encrypt("onlineOfflineSecondsDifference", "TeslaOnMars".ToCharArray()));

		private Uri cachedUri;
		private TimeCheatingDetectorEventHandler cheatChecked;
		private float timeElapsed;
		private bool updateAfterPause;
		private double lastOnlineSecondsUtc;

		#endregion

		#region Unity messages

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void OnApplicationPause(bool pauseStatus)
		{
			if (!IsStarted) return;

			if (pauseStatus)
			{
				PauseDetector();
			}
			else
			{
				ResumeDetector();
			}
		}

#if ACTK_EXCLUDE_OBFUSCATION
		[System.Reflection.Obfuscation(Exclude = true)]
#endif
		private void Update()
		{
			if (!IsStarted || !IsRunning) return;

			if (interval > 0)
			{
				if (updateAfterPause)
				{
					updateAfterPause = false;
					return;
				}

				timeElapsed += Time.unscaledDeltaTime;
				if (timeElapsed >= interval * 60)
				{
					timeElapsed = 0;
					StartCoroutine(CheckForCheat());
				}
			}
		}

		#endregion

		#region public static methods
		/// <summary>
		/// Creates new instance of the detector at scene if it doesn't exists. Make sure to call NOT from Awake phase.
		/// </summary>
		/// <returns>New or existing instance of the detector.</returns>
		public static TimeCheatingDetector AddToSceneOrGetExisting()
		{
			return GetOrCreateInstance;
		}

		/// <summary>
		/// Starts detection with specified callback.
		/// </summary>
		/// If you have detector in scene make sure it has empty Detection Event.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="cheatCheckedEventHandler">Method to call after each cheat check. Pass null if you wish to use event, set in detector inspector.</param>
		public static TimeCheatingDetector StartDetection(TimeCheatingDetectorEventHandler cheatCheckedEventHandler = null)
		{
			if (cheatCheckedEventHandler == null)
			{
				if (Instance != null)
				{
					return Instance.StartDetectionInternal(Instance.interval);
				}

				return StartDetection(GetOrCreateInstance.interval, null);
			}

			return StartDetection(GetOrCreateInstance.interval, cheatCheckedEventHandler);
		}

		/// <summary>
		/// Starts detection with specified callback using passed interval.<br/>
		/// </summary>
		/// If you pass cheatCheckedCallback than make sure you have no filled Detection Event on detector instance in scene to avoid duplicate event calls.<br/>
		/// Creates a new detector instance if it doesn't exists in scene.
		/// <param name="intervalMinutes">Time in minutes between checks. Overrides #interval property.</param>
		/// <param name="cheatCheckedEventHandler">Method to call after each cheat check. Pass null if you wish to use event, set in detector inspector.</param>
		public static TimeCheatingDetector StartDetection(float intervalMinutes, TimeCheatingDetectorEventHandler cheatCheckedEventHandler = null)
		{
			return GetOrCreateInstance.StartDetectionInternal(intervalMinutes, cheatCheckedEventHandler);
		}

		/// <summary>-
		/// Stops detector. Detector's component remains in the scene. Use Dispose() to completely remove detector.
		/// </summary>
		public static void StopDetection()
		{
			if (Instance != null) Instance.StopDetectionInternal();
		}

		/// <summary>
		/// Stops and completely disposes detector component.
		/// </summary>
		/// On dispose Detector follows 2 rules:
		/// - if Game Object's name is "Anti-Cheat Toolkit Detectors": it will be automatically
		/// destroyed if no other Detectors left attached regardless of any other components or children;<br/>
		/// - if Game Object's name is NOT "Anti-Cheat Toolkit Detectors": it will be automatically destroyed only
		/// if it has neither other components nor children attached;
		public static void Dispose()
		{
			if (Instance != null) Instance.DisposeInternal();
		}

		/// <summary>
		/// Receives UTC seconds from url. Runs asynchronously in coroutine.
		/// </summary>
		/// Automatically switches to the current domain when running in WebGL to avoid CORS limitation.
		/// <param name="url">Absolute url to receive time from.
		/// Make sure this server has proper Date values in the response headers
		/// (almost all popular web sites are suitable).</param>
		/// <param name="callback">Delegate to call and pass OnlineTimeResult to.</param>
		/// <param name="method">Method to use for url request. Use Head method if possible and fall back to get if server does not reply or block head requests.</param>
		public static IEnumerator GetOnlineTimeCoroutine(string url, OnlineTimeCallback callback, RequestMethod method = RequestMethod.Head)
		{
			var uri = UrlToUri(url);
			yield return GetOnlineTimeCoroutine(uri, callback, method);
		}

		/// <summary>
		/// Receives UTC seconds from url. Runs asynchronously in coroutine.
		/// </summary>
		/// Automatically switches to the current domain when running in WebGL to avoid CORS limitation.
		/// <param name="uri">Absolute url to receive time from.
		/// Make sure this server has proper Date values in the response headers
		/// (almost all popular web sites are suitable).</param>
		/// <param name="callback">Delegate to call and pass OnlineTimeResult to.</param>
		/// <param name="method">Method to use for url request. Use Head method if possible and fall back to get if server does not reply or block head requests.</param>
		public static IEnumerator GetOnlineTimeCoroutine(Uri uri, OnlineTimeCallback callback, RequestMethod method = RequestMethod.Head)
		{
#if ACTK_WEBGL_BUILD
			EnsureCurrentDomainUsed(ref uri);
#endif

			if (gettingOnlineTime)
				yield return CachedEndOfFrame;

			gettingOnlineTime = true;

			var result = new OnlineTimeResult();

			using (var wr = GetWebRequest(uri, method))
			{
				yield return wr.SendWebRequest();
				FillRequestResult(wr, ref result);
			}

			callback?.Invoke(result);

			gettingOnlineTime = false;
		}

#if ACTK_ASYNC
		/// <summary>
		/// Receives UTC seconds from url. Runs asynchronously.
		/// </summary>
		/// Automatically switches to the current domain when running in WebGL to avoid CORS limitation.
		/// <param name="url">Absolute url to receive time from.
		/// Make sure this server has proper Date values in the response headers
		/// (almost all popular web sites are suitable).</param>
		/// <param name="method">Method to use for url request. Use Head method if possible and fall back to get if server does not reply or block head requests.</param>
		/// <returns>OnlineTimeResult with UTC seconds or error.</returns>
		public static Task<OnlineTimeResult> GetOnlineTimeTask(string url, RequestMethod method = RequestMethod.Head)
		{
			return GetOnlineTimeTask(url, System.Threading.CancellationToken.None, method);
		}
		
		/// <summary>
		/// Receives UTC seconds from url. Runs asynchronously.
		/// </summary>
		/// Automatically switches to the current domain when running in WebGL to avoid CORS limitation.
		/// <param name="url">Absolute url to receive time from.
		/// Make sure this server has proper Date values in the response headers
		/// (almost all popular web sites are suitable).</param>
		/// <param name="cancellationToken">CancellationToken to cancel the operation.</param>
		/// <param name="method">Method to use for url request. Use Head method if possible and fall back to get if server does not reply or block head requests.</param>
		/// <returns>OnlineTimeResult with UTC seconds or error.</returns>
		public static Task<OnlineTimeResult> GetOnlineTimeTask(string url, System.Threading.CancellationToken cancellationToken, RequestMethod method = RequestMethod.Head)
		{
			var uri = UrlToUri(url);
			return GetOnlineTimeTask(uri, cancellationToken, method);
		}

		/// <summary>
		/// Receives UTC seconds from url. Runs asynchronously.
		/// </summary>
		/// Automatically switches to the current domain when running in WebGL to avoid CORS limitation.
		/// <param name="uri">Absolute url to receive time from.
		/// Make sure this server has proper Date values in the response headers
		/// (almost all popular web sites are suitable).</param>
		/// <param name="method">Method to use for url request. Use Head method if possible and fall back to get if server does not reply or block head requests.</param>
		/// <returns>OnlineTimeResult with UTC seconds or error.</returns>
		public static Task<OnlineTimeResult> GetOnlineTimeTask(Uri uri, RequestMethod method = RequestMethod.Head)
		{
			return GetOnlineTimeTask(uri, System.Threading.CancellationToken.None, method);
		}
		
		/// <summary>
		/// Receives UTC seconds from url. Runs asynchronously.
		/// </summary>
		/// Automatically switches to the current domain when running in WebGL to avoid CORS limitation.
		/// <param name="uri">Absolute url to receive time from.
		/// Make sure this server has proper Date values in the response headers
		/// (almost all popular web sites are suitable).</param>
		/// <param name="cancellationToken">CancellationToken to cancel the operation.</param>
		/// <param name="method">Method to use for url request. Use Head method if possible and fall back to get if server does not reply or block head requests.</param>
		/// <returns>OnlineTimeResult with UTC seconds or error.</returns>
		public static async Task<OnlineTimeResult> GetOnlineTimeTask(Uri uri, System.Threading.CancellationToken cancellationToken, RequestMethod method = RequestMethod.Head)
		{
#if ACTK_WEBGL_BUILD
			EnsureCurrentDomainUsed(ref uri);
#endif
			while (gettingOnlineTime)
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					await Task.Delay(100, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					return new OnlineTimeResult().SetError("Operation cancelled while waiting for previous attempt.");
				}
			}

			gettingOnlineTime = true;

			var result = new OnlineTimeResult();

			try
			{
				using (var wr = GetWebRequest(uri, method))
				{
					var asyncOperation = wr.SendWebRequest();

					while (!asyncOperation.isDone)
					{
						cancellationToken.ThrowIfCancellationRequested();
						await Task.Delay(100, cancellationToken);
					}

					FillRequestResult(wr, ref result);
				}
			}
			catch (OperationCanceledException)
			{
				result = new OnlineTimeResult().SetError("Operation cancelled.");
			}
			catch (Exception e)
			{
				Debug.LogError($"{LogPrefix} Couldn't retrieve online time");
				result = new OnlineTimeResult().SetError(e.Message);
			}
			finally
			{
				gettingOnlineTime = false;
			}

			return result;
		}
#endif
		#endregion

#if ACTK_WEBGL_BUILD
		private static string currentDomain;
		private static Uri currentUri;

		private static void EnsureCurrentDomainUsed(ref string url)
		{
			FillCurrentDomainAndUriIfNecessary();

			if (!url.Contains(currentDomain))
			{
				url = currentDomain;
			}
		}

		private static void EnsureCurrentDomainUsed(ref Uri uri)
		{
			FillCurrentDomainAndUriIfNecessary();

			if (currentUri.GetLeftPart(UriPartial.Authority) != uri.GetLeftPart(UriPartial.Authority))
			{
				uri = currentUri;
			}
		}

		private static void FillCurrentDomainAndUriIfNecessary()
		{
			if (!string.IsNullOrEmpty(currentDomain))
				return;

			var ownUrl = Application.absoluteURL;
			if (string.IsNullOrEmpty(ownUrl))
			{
				Debug.LogWarning($"{LogPrefix} Couldn't get valid string from Application.absoluteURL");
				return;
			}

			currentUri = new Uri(ownUrl);
			currentDomain = currentUri.GetLeftPart(UriPartial.Authority);
		}
#endif

		private static UnityWebRequest GetWebRequest(Uri uri, RequestMethod method)
		{
			var request = new UnityWebRequest(uri, method == RequestMethod.Head ? UnityWebRequest.kHttpVerbHEAD : UnityWebRequest.kHttpVerbGET)
			{
				useHttpContinue = false,
				timeout = Instance ? Instance.timeoutSeconds : DefaultTimeoutSeconds,
				certificateHandler = null
			};
			
#if ACTK_ACTUAL_ANDROID_DEVICE
			if (method == RequestMethod.Head)
			{
				try
				{
					if (sdkLevel == 0)
						sdkLevel = GetAndroidSDKLevel();

					if (sdkLevel <= 17)
						request.SetRequestHeader("Accept-Encoding", "");

				}
				catch (Exception e)
				{
					ACTk.PrintExceptionForSupport("Couldn't get SDK version or set Accept-Encoding header.", LogPrefix, e);
				}
			}
#endif
			return request;
		}

		private static void FillRequestResult(UnityWebRequest request, ref OnlineTimeResult result)
		{
			if (!string.IsNullOrEmpty(request.error))
			{
				result.SetError(request.error, request.responseCode);
			}
			else
			{
				var dateHeader = request.GetResponseHeader("Date");
				if (!string.IsNullOrEmpty(dateHeader))
				{
					var success = TryGetDate(dateHeader, out var serverTime);
					if (success)
					{
						var onlineTimeUtc = serverTime.ToUniversalTime();
						var onlineSecondsUtc = onlineTimeUtc.Ticks / (double)TimeSpan.TicksPerSecond;
						result.SetTime(onlineSecondsUtc, onlineTimeUtc);
					}
					else
					{
						result.SetError("Couldn't parse 'Date' response header value\n " + dateHeader, request.responseCode);
					}
				}
				else
				{
					result.SetError("Couldn't find 'Date' response header value!", request.responseCode);
				}
			}

			if (!result.success)
				Debug.Log(LogPrefix + "Online Time Retrieve error:\n" + result);
		}

		private static Uri UrlToUri(string url)
		{
#if ACTK_WEBGL_BUILD
			EnsureCurrentDomainUsed(ref url);
#endif
			var success = Uri.TryCreate(url, UriKind.Absolute, out var result);
			if (!success)
				Debug.LogError(LogPrefix + "Could not create URI from URL: " + url);

			return result;
		}

		// naive but blazingly fast standard HTTP GMT Date parsing 
		// ex. 'Tue, 18 Sep 2018 16:28:26 GMT' (ddd, dd MMM yyyy HH:mm:ss 'GMT')
		// (C) Dmitriy Yukhanov | codestage.net
		private static bool TryGetDate(string source, out DateTime date)
		{
			try
			{
				var chars = source.ToCharArray(5, 20);

				var d1 = chars[0] & 0x0f;
				var d2 = chars[1] & 0x0f;
				var day = d1 * 10 + d2;

				var mo1 = (int)chars[4];
				var mo2 = (int)chars[5];
				var sum = mo1 + mo2;
				int month;

				switch (sum)
				{
					case 207:
						month = 1;
						break;
					case 199:
						month = 2;
						break;
					case 211:
						month = 3;
						break;
					case 226:
						month = 4;
						break;
					case 218:
						month = 5;
						break;
					case 227:
						month = 6;
						break;
					case 225:
						month = 7;
						break;
					case 220:
						month = 8;
						break;
					case 213:
						month = 9;
						break;
					case 215:
						month = 10;
						break;
					case 229:
						month = 11;
						break;
					case 200:
						month = 12;
						break;
					default:
						month = 1;
						break;
				}

				var y1 = chars[7] & 0x0f;
				var y2 = chars[8] & 0x0f;
				var y3 = chars[9] & 0x0f;
				var y4 = chars[10] & 0x0f;
				var year = y1 * 1000 + y2 * 100 + y3 * 10 + y4;

				var h1 = chars[12] & 0x0f;
				var h2 = chars[13] & 0x0f;
				var hour = h1 * 10 + h2;

				var m1 = chars[15] & 0x0f;
				var m2 = chars[16] & 0x0f;
				var minute = m1 * 10 + m2;

				var s1 = chars[18] & 0x0f;
				var s2 = chars[19] & 0x0f;
				var second = s1 * 10 + s2;

				date = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

				return true;
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Error while parsing date!", LogPrefix, e);
				date = default;
				return false;
			}
		}

		#region public instance methods
		/// <summary>
		/// Allows to manually execute cheating check. Restarts #interval.
		/// </summary>
		/// Listen for detector events to know about check result.
		/// <returns>True if check was started.</returns>
		public bool ForceCheck()
		{
			if (!IsStarted || !IsRunning)
			{
				Debug.LogWarning(LogPrefix + "Detector should be started to use ForceCheck().");
				LastError = ErrorKind.NotStarted;
				LastResult = CheckResult.Error;
				return false;
			}

			if (IsCheckingForCheat)
			{
				Debug.LogWarning(LogPrefix + "Can't force cheating check since another check is already in progress.");
				LastError = ErrorKind.AlreadyCheckingForCheat;
				LastResult = CheckResult.Error;
				return false;
			}

			timeElapsed = 0;
			StartCoroutine(CheckForCheat());

			return true;
		}

		/// <summary>
		/// Allows to manually execute cheating check and wait for the completion within coroutine. Restarts #interval.
		/// </summary>
		/// Use inside of the coroutine and check #LastResult property after yielding this method.
		/// Detector events will be invoked too.
		/// Example:
		/// \code
		/// StartCoroutine(MakeForcedCheck());
		/// private IEnumerator MakeForcedCheck()
		/// {
		///	    yield return TimeCheatingDetector.Instance.ForceCheckEnumerator();
		///	    // check TimeCheatingDetector.Instance.LastResult
		///	    // ...
		/// }
		/// \endcode
		public IEnumerator ForceCheckEnumerator()
		{
			if (!IsStarted || !IsRunning)
			{
				Debug.LogWarning(LogPrefix + "Detector should be started to use ForceCheckEnumerator().");
				LastError = ErrorKind.NotStarted;
				LastResult = CheckResult.Error;
				yield break;
			}

			if (IsCheckingForCheat)
			{
				Debug.LogWarning(LogPrefix + "Can't force cheating check since another check is already in progress.");
				LastError = ErrorKind.AlreadyCheckingForCheat;
				LastResult = CheckResult.Error;
				yield break;
			}

			timeElapsed = 0;
			yield return CheckForCheat();

			while (IsCheckingForCheat)
			{
				yield return CachedEndOfFrame;
			}
		}

#if ACTK_ASYNC
		/// <summary>
		/// Allows to manually execute cheating check and wait for the completion within async method. Restarts #interval.
		/// </summary>
		/// Await this method in async method to wait for the check completion and to get the check result.
		/// Detector events will be invoked too.
		/// Disabled for WebGL platform.
		/// \code
		/// MakeForcedCheckAsync();
		/// private async void MakeForcedCheckAsync()
		/// {
		///	    var result = await TimeCheatingDetector.Instance.ForceCheckTask();
		///	    // check result
		///	    // ...
		/// }
		/// \endcode
		public async Task<CheckResult> ForceCheckTask()
		{
			if (!IsStarted || !IsRunning)
			{
				Debug.LogWarning(LogPrefix + $"Detector should be started in order to use the {nameof(ForceCheckTask)}.");
				LastError = ErrorKind.NotStarted;
				LastResult = CheckResult.Error;
				return LastResult;
			}

			if (IsCheckingForCheat)
			{
				Debug.LogWarning(LogPrefix + "Can't force cheating check since another check is already in progress.");
				LastError = ErrorKind.AlreadyCheckingForCheat;
				LastResult = CheckResult.Error;
				return LastResult;
			}

			timeElapsed = 0;
			StartCoroutine(CheckForCheat());

			await Task.Delay(50);

			if (IsCheckingForCheat)
			{
				while (IsCheckingForCheat)
				{
					await Task.Delay(50);
				}
			}

			return LastResult;
		}
#endif

		#endregion

		private TimeCheatingDetector StartDetectionInternal(float checkInterval, TimeCheatingDetectorEventHandler cheatCheckedEventHandler = null)
		{
			if (IsRunning)
			{
				Debug.LogWarning(LogPrefix + "already running!", this);
				return this;
			}

			if (!enabled)
			{
				Debug.LogWarning($"{LogPrefix}disabled but {nameof(StartDetection)} still called from somewhere (see stack trace for this message)!", this);
				return this;
			}
			
			timeElapsed = 0;
			cheatChecked = cheatCheckedEventHandler;
			interval = checkInterval;

			IsStarted = true;
			IsRunning = true;

			return this;
		}

		protected override bool Init(TimeCheatingDetector instance, string detectorName)
		{
			if (cachedUri == null)
			{
				cachedUri = UrlToUri(requestUrl);
			}

			return base.Init(instance, detectorName);
		}

		protected override void StartDetectionAutomatically()
		{
			StartDetectionInternal(interval);
		}

		protected override bool DetectorHasListeners()
		{
			return base.DetectorHasListeners() || CheatChecked != null || cheatChecked != null;
		}

		protected override void PauseDetector()
		{
			base.PauseDetector();
			updateAfterPause = true;
		}

		protected override void StopDetectionInternal()
		{
			base.StopDetectionInternal();

			cheatChecked = null;
			CheatChecked = null;
		}

		private IEnumerator CheckForCheat()
		{
			if (!IsRunning || IsCheckingForCheat) 
				yield break;

			IsCheckingForCheat = true;

			LastError = ErrorKind.NoError;
			LastResult = CheckResult.Unknown;

			if (cachedUri == null)
			{
				LastError = ErrorKind.IncorrectUri;
				LastResult = CheckResult.Error;
			}
			else
			{
				yield return GetOnlineTimeCoroutine(cachedUri, OnOnlineTimeReceived, requestMethod);
			}

			if (!IsStarted || !IsRunning)
			{
				LastError = ErrorKind.Unknown;
			}

			if (lastOnlineSecondsUtc <= 0 && LastError == ErrorKind.NoError)
			{
				LastError = ErrorKind.Unknown;
			}

			if (LastError != ErrorKind.NoError && LastError != ErrorKind.AlreadyCheckingForCheat)
			{
				LastResult = CheckResult.Error;
				ReportCheckResult();
				IsCheckingForCheat = false;
				yield break;
			}

			LastError = ErrorKind.NoError;

			var offlineSecondsUtc = DateTime.UtcNow.Ticks / (double)TimeSpan.TicksPerSecond; // local utc secs
			var offlineOnlineDifference = (int)Math.Abs(lastOnlineSecondsUtc - offlineSecondsUtc);

			LastResult = CheckResult.CheckPassed;

			if (offlineOnlineDifference > wrongTimeThreshold * 60)
			{
				LastResult = CheckResult.WrongTimeDetected;
			}

			var lastOfflineOnlineDifference = PlayerPrefs.GetInt(onlineOfflineDifferencePrefsKey, 0);
			if (lastOfflineOnlineDifference != 0)
			{
				lastOfflineOnlineDifference ^= int.MaxValue;
				var differenceOfDifferences = Math.Abs(offlineOnlineDifference - lastOfflineOnlineDifference);

				if (realCheatThreshold < 10)
					Debug.LogWarning(LogPrefix + "Please consider increasing realCheatThreshold to reduce false positives chance!");

				if (differenceOfDifferences > realCheatThreshold * 60)
				{
					if (LastResult == CheckResult.WrongTimeDetected || !ignoreSetCorrectTime)
					{
#if ACTK_DETECTION_BACKLOGS
						Debug.LogWarning(LogPrefix + "Detection backlog:\n" +
										 "wrongTimeThreshold: " + wrongTimeThreshold + "\n" +
										 "realCheatThreshold: " + realCheatThreshold + "\n" +
										 "offlineSecondsUtc: " + offlineSecondsUtc + "\n" +
										 "lastOnlineSecondsUtc: " + lastOnlineSecondsUtc + "\n" +
										 "offlineOnlineDifference: " + offlineOnlineDifference + "\n" +
										 "lastOfflineOnlineDifference: " + lastOfflineOnlineDifference + "\n" +
										 "differenceOfDifferences: " + differenceOfDifferences);
#endif
						LastResult = CheckResult.CheatDetected;
					}
				}
			}

			PlayerPrefs.SetInt(onlineOfflineDifferencePrefsKey, offlineOnlineDifference ^ int.MaxValue);
			IsCheckingForCheat = false;
			ReportCheckResult();
		}

		private void ReportCheckResult()
		{
			cheatChecked?.Invoke(LastResult, LastError);
			CheatChecked?.Invoke(LastResult, LastError);

			switch (LastResult)
			{
				case CheckResult.Unknown:
					break;
				case CheckResult.CheckPassed:
					break;
				case CheckResult.WrongTimeDetected:
					break;
				case CheckResult.CheatDetected:
					OnCheatingDetected();
					break;
				case CheckResult.Error:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void OnOnlineTimeReceived(OnlineTimeResult result)
		{
			if (result.success)
			{
				lastOnlineSecondsUtc = result.onlineSecondsUtc;
			}
			else
			{
				lastOnlineSecondsUtc = -1;
				LastError = ErrorKind.OnlineTimeError;
			}
		}

#if ACTK_ACTUAL_ANDROID_DEVICE
		private static int GetAndroidSDKLevel()
		{
			var osString = SystemInfo.operatingSystem;
			if (string.IsNullOrEmpty(osString))
			{
				return -1;
			}

			var dashIndex = osString.IndexOf("-", StringComparison.Ordinal);
			if (dashIndex <= 0)
			{
				return -1;
			}

			if (osString.Length < dashIndex + 3)
			{
				return -1;
			}

			var apiPart = osString.Substring(dashIndex + 1, 2);
			if (string.IsNullOrEmpty(apiPart))
			{
				return -1;
			}

			if (!int.TryParse(apiPart, out var result))
			{
				return -1;
			}

			return result;
		}
#endif

		#region obsolete members

		[Obsolete("Please use CheatChecked event instead", true)]
#pragma warning disable 67
		public event Action<ErrorKind> Error;

		[Obsolete("Please use CheatChecked event instead", true)]
		public event Action CheckPassed;
#pragma warning restore 67

		[Obsolete("Use wrongTimeThreshold instead.", true)]
		[NonSerialized]
		public int threshold = 65;

		[Obsolete("Use requestUrl instead", true)]
		[NonSerialized]
		public string timeServer = "google.com";

		[Obsolete("Please use GetOnlineTimeCoroutine or GetOnlineTimeTask instead", true)]
		public static double GetOnlineTime(string server)
		{
			return -1;
		}

		[Obsolete("Please use Instance.Error event instead.", true)]
		public static void SetErrorCallback(Action<ErrorKind> errorCallback)
		{

		}

		[Obsolete("Please use StartDetection(int, ...) instead.", true)]
		public static void StartDetection(Action detectionCallback, int interval)
		{

		}

		[Obsolete("Please use StartDetection(int, ...) instead.", true)]
		public static void StartDetection(Action detectionCallback, Action<ErrorKind> errorCallback, int interval)
		{

		}

		[Obsolete("Please use other overloads of this method instead", true)]
		public static void StartDetection(float interval, Action detectionCallback, Action<ErrorKind> errorCallback, Action checkPassedCallback)
		{

		}

		#endregion
#else
		private const string ErrorMessage = LogPrefix + " is disabled with ACTK_PREVENT_INTERNET_PERMISSION conditional or is not supported on current platform!";
		protected override void StartDetectionAutomatically()
		{
			Debug.LogError(ErrorMessage);
		}
#endif
	}
}
