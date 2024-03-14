# Anti-Cheat Toolkit Changelog
Changelog format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

#### Types of changes
- **Added** for new features.
- **Changed** for changes in existing functionality.
- **Deprecated** for soon-to-be removed features.
- **Removed** for now removed features.
- **Fixed** for any bug fixes.
- **Security** in case of vulnerabilities

_Please, always remove previous plugin version before updating!_

## [2021.1.1] - 2022-05-04

### Added
- Add TimeCheatingDetector.GetOnlineTimeTask() overloads with CancellationToken argument

## [2021.1.0] - 2022-04-11

### Added
- Add ObscuredFilePrefs Auto Save on mobile platforms (enabled by default)
  - Automatically saves unsaved changes on app loose focus / pause
- Add API to disable ObscuredFilePrefs Auto Save (disables Auto Save on both mobile and non-mobile platforms)
- Introduce IObscuredFileSettings to improve API usage experience

### Changed
- Add locks to the ObscuredFilePrefs sync operations to improve stability when accessing it from different threads
- Move ObscuredFilePrefs Save-On-Quit code to the Auto Save feature entity so it's disableable now

### Fixed
- Prevent ObscuredFilePrefs Save-On-Quit while not initialized
- Fix ObscuredFilePrefs behavior with disabled Reload Domain
- Fix compilation error at Unity 2018 Android
- Fix compilation warnings for WebGL platform

## [2021.0.10] - 2022-03-09

### Fixed
- Fix ObscuredString name in Inspector might render incorrect in arrays (thx Sungmin An)

## [2021.0.9] - 2022-03-06

### Changed
- CodeHashGenerator's Summary Hash is no longer printed for AAB builds
- Skip Android Patch Packages hashing by CodeHashGenerator

### Fixed
- Fix obsolete API usage leading to compilation error in Unity 2022.1

## [2021.0.8] - 2022-02-08

### Changed
- Minor Prefs Editor UI improvements

### Fixed
- Fix Prefs Editor window didn't update properly under specific conditions (thx Todd Gillissie)

## [2021.0.7] - 2021-11-18

### Fixed
- Fix iOS Conditional compilation constants settings could not apply in some Unity versions (thx Hesham)
- Fix empty ObscuredString fields automatic migration (thx thiagolr)

## [2021.0.6] - 2021-11-18

### Changed
- Warn when trying to use ObscuredFile with StreamingAssets on Android and WebGL (thx Harama)

### Fixed
- Fix automatic ObscuredString migration didn't happen properly in some cases (thx thiagolr)
- Fix exception in ObscuredFilePrefs on iOS could happen under rare conditions
- Fix ObscuredString example log 

## [2021.0.5] - 2021-10-26

### Changed
- Improve ObscuredPrefs and ObscuredFilePrefs compatibility with Obscured types

### Fixed
- Fix TimeUtils could be disposed unexpectedly (thx Hesham)
- Fix TimeUtils might not reinitialize properly in rare case

## [2021.0.4] - 2021-10-02

### Fixed
- Fix BehaviorDesigner integration package compilation errors (thx Levent)

## [2021.0.3] - 2021-09-27

### Changed
- Improve TimeCheatingDetector performance a bit

### Fixed
- Fix missing script at the example scene
- Fix CodeHashGeneratorListener example compilation errors

## [2021.0.2] - 2021-09-17

### Fixed
- Fix empty string was read as null from ObscuredPrefs and ObscuredFilePrefs (thx C0dingschmuser)

## [2021.0.1] - 2021-09-10

### Changed
- Improve exceptions logging a bit

### Fixed
- Fix compilation exception for iOS platform (thx Vladnee)

## [2021.0.0] - 2021-09-06

### Added
- Add new ObscuredFile and ObscuredFilePrefs tools to the ACTk 🧰
  - Encrypted and plain modes 
  - All modes have data consistency validation
  - All modes have lock to device feature
  - ObscuredFilePrefs has simple and easy to use PlayerPrefs-like APIs
  - Async compatible
  - Supports UWP starting from Unity 2019.1
  - BehaviorDesigner and PlayMaker Actions
- Add generic APIs to ObscuredPrefs
- Add new types support to ObscuredPrefs:
  - rest of simple C# types (SByte, Byte, Int16, UInt16, Char)
  - System.DateTime
  - Color (it's possible to save HDR colors now)
  - Matrix4x4, RangeInt, Ray, Ray2D, RectInt, Vector2Int, Vector3Int, Vector4
- Add ObscuredQuaternion property drawer (now it's editable from inspector)
- Add automatic link.xml generation option to complement fix for WallHack Detector false positives due to stripping
- Add additional information to the important error logs
- Make ThreadSafeRandom utility public
- Add Copy Player Prefs path context menu item to the Prefs Editor tab
- Add ObscuredPrefs Vector2Int and Vector3Int support to BehaviorDesigner integration
- Add new support contact, let's chat at [Discord](https://discord.gg/Ppsb89naWf)!

### Changed
- Swap Changelog to md version to better match Unity packages format ([Keep a Changelog](https://keepachangelog.com/en/1.0.0/))
- Rename following ObscuredPrefs API in order to better suite coding style:
  - OnAlterationDetected -> NotGenuineDataDetected
  - OnPossibleForeignSavesDetected -> DataFromAnotherDeviceDetected
  - lockToDevice -> DeviceLockLevel
- Move ObscuredPrefs.DeviceLockLevel enum out from the ObscuredPrefs type
- Introduce DeviceLockTamperingSensitivity instead of readForeignSaves and emergencyMode settings for additional clarity
- Decimal values processing at ObscuredPrefs are much faster now with much lesser GC-allocations footprint
- Improve exceptions handling across whole codebase
- Improve incorrect type usage handling at ObscuredPrefs (thx David E)
- Improve Settings UI a bit
- Improve detectors startup a bit
- Improve Prefs Editor error handling
- Minor code refactoring and cleanup
- Update some API docs

### Deprecated
- Deprecate non-generic ObscuredPrefs APIs (to be removed in future versions)

### Removed
- Remove .NET 3.5 scripting runtime version support

### Fixed
- Fix possible data corruption at all Obscured types in super rare scenarios (only one rare case for ObscuredBool was found)
- Fix possible false positives from WallHackDetector on Unity 2019.3 or newer when IL2CPP "Strip Engine Code" setting is used (thx Hesham)
- Fix compilation warning on UWP platform
- Fix redundant injection detector support were added into IL2CPP builds in some conditions
- Fix exceptions in Unity 2021.2 and newer while browsing ACTk settings
- Fix code hash pre-generation was run redundantly when building with Create Visual Studio Solution option enabled
- Fix Behavior Tree at BehaviorDesigner's integration ObscuredPrefsExample scene 
- Fix other minor stuff here and there

2.3.4
- backport from 3.0.0-beta.2: fix null reference exception in Unity 2021.2 and newer while browsing ACTk settings (IL2CPP section)
- want to participate in ACTk v3 Beta and get a chance to win v3 voucher or any other Code Stage plugin voucher of your choice?
  reach out at https://codestage.net/contacts or https://discord.gg/Ppsb89naWf

2.3.3
- backport from 3.0.0-alpha: fix possible exception in Unity 2021.2 and newer while browsing ACTk settings

2.3.2
- prepare for legacy .NET 3.5 support deprecation in upcoming 3.0 major release
- fix inverted ACTK_US_EXPORT_COMPATIBLE logic
- improve minor stuff

2.3.1
- fix possible code corruption due to the obfuscation (thx firediver98)

2.3.0
- update minimum Unity version to 2018.4
- remove legacy code
- add DateTime value to the TimeCheatingDetector's OnlineTimeResult

2.2.4
- fix compilation warnings for non-IL2CPP UWP platform
- fix build errors on non-IL2CPP UWP platform (thx Aymen)

2.2.3
- fix possible SharpZipLib.dll conflicts (thx Nathaniel)

2.2.2
- fix possible null reference exception when migrating from ACTkv1 (thx RonoVanBono)
- fix HasKey() did not checked ACTkv1 keys (thx Albert)

2.2.1
- fix compilation error in Unity 2018.1 - 2018.4 with .NET 3.5 scripting (thx Vladimir)

2.2.0
- add new APIs to CodeHashGenerator to return all files hashes for accurate checks
  * allows to check split builds hashes (e.g. from aab)
- improve CodeHashGenerator performance
- improve CodeHashGenerator Android native part errors processing
- clean and refactor some code
- fix possible error in CodeHashGenerator example code

2.1.3
- fix Code Hashing example compilation error
- add ProGuard notice to the Readme (thx Vladimir)

2.1.2
- fix compilation warnings and errors with TimeCheatingDetector disabled (thx Gary)

2.1.1
- fix ObscuredString.GetDecrypted returning value (thx Jose)

2.1.0
- add ignoreSetCorrectTime option to TimeCheatingDetector
- add IsStarted property to all detectors
- fix possible wrong TimeCheatingDetector check result (thx esteban16108)

2.0.6
- add ACTK_US_EXPORT_COMPATIBLE symbol (see Readme for details)
- fix possible data loss after removing all obscured prefs (thx n0lex)

2.0.5
- fix incorrect CodeHashGenerator result Success calculation (thx murat303)
- ignore more standard prefs at the Prefs Editor (thx n0lex)

2.0.4
- remove few extra debug logs from Android native plugin
- add code obfuscation and overall code protection notes to the readme

2.0.3
- add few API proxies to the ObscuredString for better compatibility
- fix ObscuredString.GetHashCode returned incorrect values (thx quanbp)

2.0.2
- fix incorrect ObscuredString.Length property value (thx Davy)

2.0.1
- move settings from User to Project space

2.0.0
- add new feature: CodeHashGenerator (Unity 2018.1 and newer)
  * generate hash for your code in Editor to save in resources or on your server
  * generate hash for your code in Runtime for further validation
  * hash generation performs on separate thread
  * hash Mono, IL2CPP, Android App Bundle builds
  * support for Standalone Windows and Android, more to come
- add new feature: SpeedHackProofTime class
  * use speed-hack resistant timers to let you ignore speedhack
  * proxy to Time.* until speed hack is detected to avoid extra resources waste
  * mimic non-fixed Time.* APIs (e.g. time, deltaTime, unscaledTime, etc.)
- Obscured types update:
  * make obscured types handle crypto keys fully automatically
  * automatically randomize crypto key for each new instance
  * close few key-related vulnerabilities
  * refactor and update API and docs for consistency 
  * add GenerateKey() API
  * improve ObscuredString performance
  * reduce ObscuredString GC allocations by 24%
  * fix overridden obscured types indication in Unity 2018.3+
- Obscured Prefs update:
  * make ObscuredPrefs handle crypto keys fully automatically
  * change encryption bits with automatic migration from ACTk v1
  * make SetRawValue and GetRawValue to take encrypted key into account
  * remove support for pre-1.4.0 prefs migration, use ACTk v1 to migrate
  * remove unobscured mode, please use PrefsEditor to view obscured prefs
  * refactor and cleanup
- Injection Detector update:
  * add dynamic whitelist generation (no more default whitelist)
  * dramatically improve injection check performance (Unity 2018.3 and newer)
  * include only used assemblies in whitelist (Unity 2018.3 and newer)
  * enable Injection Detector feature from the detector's inspector
  * fix lots false positives with old default whitelist
  * fix detector didn't disable for IL2CPP builds in some cases
  * add detection event delegate for addition argument clarity
- Speed Hack Detector update:
  * add Threshold property to allow tiny clocks speed difference on different HW
  * slightly improve and refactor internals
- Wall Hack Detector update:
  * improve warnings and errors UI in inspector
  * fix false positive for volumetric fog (volumetric atmospheric scattering in HDRP)
  * fix rare false positive for Linear color space in 2017.4.1 and newer
  * fix rare false positive for Post-processing stack
  * fix rare false positives due to Unity rendering glitches
- Settings update:
  * move Settings window to the Unity Editor Preferences section
  * add new buttons-icons for quick access to the important related resources
  * add IL2CPP section with "Switch to IL2CPP" button for supported platforms
  * add "add support to build" toggle in Injection Detector section
  * add validation for Injection Detector to have support toggle enabled
  * add validation for Wallhack Detector wireframe shader to be always included
  * add custom whitelist entries count to the button which opens whitelist editor
  * make all ACTk settings to be per-project
  * update and refactor UI a bit
  * prevent compilation conditional changes while editor is compiling anything
  * fix ACTK_IS_HERE state check missed
- add StringToBytes() and BytesToString() helper methods to ObscuredString
- add IsCheatDetected property to all detectors
- add Assembly Definitions
- make all detectors return their instances from StartDetection methods
- make PrefsEditor aware of TimeCheatingDetector service pref
- make PrefsEditor aware of more standard Unity prefs to hide them
- fix and improve lots of other stuff across codebase
- fix various compilation warnings in newest Unity versions
- remove deprecated components from example scenes preventing warnings in 2019.3
- fix possible data corruption while incrementing ObscuredShort
- fix compilation errors regression in PlayMaker integration package
- update project to Unity 2017.4.1
- refactor all detectors code to have nicer inheritance
- refactor menu-related code a bit for better readability and clarity
- refactor other minor parts of code base
- remove legacy code

1.6.7 [Maintenance Update before big 2.0 release!]
- update to Unity 5.6.1
- improve random key generation performance in some cases
- add few missing assemblies to the default InjectionDetector whitelist
- prevent detectors duplicates on same Game Object
- workaround for Android 4.2.* HEAD request bug
- fix RandomRangeInt can only be called from the main thread in some cases
- fix possible null ref exception in ObscuredPrefs.SetString
- other minor changes

1.6.6
- update to Unity 5.6
- fix Unity 2018.3 project settings open code didn't compiled
- fix ObscuredQuaternion was shown in inspector
- fix ObscuredVector3 and ObscuredQuaternion migration (thx friuns3)
- fix migration of prefab instances in scene after prefabs migration

1.6.5
- ObscuredPrefs actions refactored to events to properly allow multiple listeners
- example code refactored and updated
- add support for dynamic assemblies to InjectionDetector (thx Michal)
- fix InjectionDetector whitelist had no some .NET Core assemblies (thx Michal)
- fix InjectionDetector crash when user whitelist has incorrect chars (thx Michal)

1.6.4
- celebrating 5 years of free updates! \o/
- file names and structure refactored - please remove previous version completely!
- TimeCheatingDetector got more attention again (breaks existing APIs):
  * core rewritten and now works with WebGL and UWP .NET (thank you Vincent!)
  * logic to exclude false positives when user just has wrong time
  * now detects both wrong time and actual time cheating with separate thresholds
  * few old events deprecated in favor of new more generic CheatChecked event
  * fixed possible Sockets errors
  * fixed incorrect check interval calculation after resuming unity player
- add static method FromEncrypted() to all obscured variable types
- added new detectors prefab with another layout (each detector on separate object)
- improved UI of settings window
- added few new conditional symbols options for better debug and compatibility
- updated InjectionDetector whitelist to match Unity 2018.3.0b2
- detectors inspector UI updated to make it easier to read settings
- fixed obscured values initialization in inspector was inconsistent in some cases
- fixed warnings when KeepAlive detectors were started from nested objects
- readme got list of contents with hyperlinks for quicker navigation
- some legacy redundant code removed
- other minor fixes and improvements

1.6.3
- SpeedHackDetector on Android improvements:
  * now detects root level speed hack on Android 8.0 and newer (no root required)
  * more accurate algorythm removing chance of false positives
  * resources usage significantly improved
- fixed possible ObscuredCheatingDetector false positive on SetEncryptred() (thx Matt)
- fixed TimeCheatingDetector compilation for the UWP IL2CPP platform

1.6.2
- fixed 1.6.1 critical compilation error for Android platform (sorry!)
- fixed missing ObscuredVector2, ObscuredVector3 and ObscuredQuaternion migration
- fixed WallHackDetector false positive when Linear color space is used (thx Fiete)
- fixed wrong plugin path at Unity 2018 packages (thx Fiete)
- fixed compilation error when enabling WALLHACK_DEBUG flag (thx Fiete)
- fixed test scene null reference errors in some cases when cheats are detected
- fixed exception when removing entry at the user-defined Whitelist window (thx Matt)
- other minor fixes
- minor refactorings

1.6.1
- SpeedHackDetector now able to detect root-level speed hack on Android (beta)
- organized usings in all code to prevent ambiguity with third-party classes
- TimeCheatingDetector:
  * ForceCheckEnumerator method execution slightly changed to improve stability
  * added force check examples to the ActTesterGui
- removed some legacy code

1.6.0
- TimeCheatingDetector got some more love:
  * enabled for Windows Universal Platform with IL2CPP
  * stability improvements (thx Nilooo & off3nd)
  * Error and CheckPassed events exposed in public API now
  * new ErrorKind enum for error events
  * added new callback for successful check passes
  * interval 0 is now supported for the manual ForceCheck() execution only
  * added ForceCheckEnumerator() for yielding from coroutine
  * added awaitable ForceCheckTask() for async methods (.NET 4.6+, no WebGL)
  * added LastResult and LastError properties
- all Detectors: 
  * added new AddToSceneOrGetExisting() API (execute from Start phase or later)
  * exposed CheatDetected event for subscription
  * now you may have multiple listeners for the detection event
- added IComparable implementations to all native obscured types
  * now native obscured types are sortable and LINQ-friendly
- exposed double epsilon at the ObscuredCheatingDetector
- added Paste functionality to the prefs editor
- get rid of internals usage at editor scripts (allows asmdef usage)
- improved ObscuredDouble/Float security, migration required (thx off3nd)
- added ObscuredDouble/Float migration from 1.5.2.0 - 1.5.8.0 to 1.6.0+ :
  * auto migration at runtime with ACTK_OBSCURED_AUTO_MIGRATION flag
  * manual migration at all prefabs and opened scenes via menu commands
  * runtime API to migrate values got with GetEncrypted()
  * removed legacy pre-1.5.2.0 support for Obscured types migration
- now detector object is automatically selected in scene after creation
- BehaviorDesigner integration package updated:
  * added TimeCheatingDetected conditional
  * fixed compatibility issues
- PlayMaker integration package updated:
  * added TimeCheatingDetected action
  * fixed compatibility issues
- updated InjectionDetector whitelist to match Unity 2018.2.0b3
- switched to X.X.X version tracking format
- fixed compiler errors on UWP platform (thx Wajdi)
- fixed possible incorrect readout of string pref at PrefsEditor
- fixed incorrect behavior when copying unobscured string pref
- fixed incorrect behavior of InjectionDetector in editor in debug mode
- fixed missing Reflection.Obfuscation attributes at TimeCheatingDetector
- fixed compilation flags set/get code to avoid obsolete platforms
- fixed duplicate flags setting for some platforms (cleanup needed)
- fixes and additions in API docs
- minor refactor
- other minor fixes

1.5.8.0
- TimeCheatingDetector got some love:
  * added new ForceCheck method for manual execution
  * added new IsCheckingForCheat property
  * first check on Start now removed
  * fixed callbacks could run not on the main thread
  * improved overall stability of the TimeCheatingDetector (thx Nilo)
  * reduced min range for the TimeCheatingDetector interval to 0.1 min
- updated default whitelist of InjectionDetector up to Unity 2018 beta

1.5.7.0
- added obscured versions of Vector2Int and Vector3Int from Unity 2017.2+
- base Unity version increased to 5.1.0f3
- InjectionDetector whitelist updated up to Unity 2017.3b6
- added ObscuredDecimalDrawer (thx mcmorry)
- added IsRunning read-only property to all detectors
- added ObscuredByte.EncryptDecrypt(byte[] value) API variants
- added proper help urls for all detectors
- added error callback to the TimeCheatingDetector
- reduced GC allocations count at WallHackDetector for Unity 5.3+
- removed urls-related code from the examples for better compatibility
- fixed non-blocking error while switching the condition compilation symbols
- minor refactorings
- minor improvements

1.5.6.1
- TimeCheatingDetector now uses async methods to reduce main thread locks
- disabled TimeCheatingDetector for Windows Universal Platform
- fixed initialization of Obscured types with SetEncrypted call (thx Keith)
- added version output at the Settings window
- added tooltips to the compilation symbols at the Settings window
- updated InjectionDetector whitelist to match Unity 2017.2.0b4
- minor improvements
- minor fixes

1.5.6.0
- brand new TimeCheatingDetector (needs Internet connection)
- another kind of SpeedHack detection added (thx lol)
- third-party integration packages paths are match ACTk path now
- added switches for all important conditional compilation symbols 
  to the Settings window
- improved Unity 2017 compatibility

1.5.5.0
- menu items migrated to the "Tools > Code Stage > Anti-Cheat Toolkit"
- added new utility script to migrate ObscuredDouble and ObscuredFloat 
  instances on prefabs when updating from 1.5.1.0 and previous to the 
  1.5.2.0 +; you can run it via menu Tools > Code Stage > ACTk > Migrate...
- improved compatibility with Unity 5.5+
- fixed ObscuredCheatingDetector false positives (huge thx Fiete)
- fixed vulnerability of the obscured types
- fixed ObscuredShort inspector output in Unity 5.0+
- fixed false cheating detections for obscured types which are able to 
  show up in the inspector in some cases (thx mrm83)

1.5.4.0
- added (x,y) constructor and same Encrypt API to the ObscuredVector2
- added (x,y,z) constructor and same Encrypt API to the ObscuredVector3
- added (x,y,z,w) constructor and same Encrypt API to the ObscuredQuaternion
- added new GetDecrypted API to all obscured memory types
- added ObscuredShort inspector output for Unity 5.0+
- fixed incorrect InjectionDetector execution with argument in callback

1.5.3.0
- plugin moved to Plugins folder, please remove previous ACTk version!
- added ObscuredString.Length property for better compatibility
- added ULong support to the ObscuredPrefs
- added Decimal support to the ObscuredPrefs
- added ObscuredUInt inspector output for Unity 5.0+
- added ObscuredULong inspector output for Unity 5.0+

1.5.2.3
- improved Unity 5.6 compatibility
- added workaround for a Unity bug related to ObscuredDouble on prefabs

1.5.2.2
- updated InjectionDetector whitelist up to the Unity 5.5
- fixed possible data corruption leading to false positives in Obscured types
- updated third-party integration packages to the new APIs
- improved compatibility with Unity 5.5
- reduced chance of the wrong encrypted pref detection at the prefs editor

1.5.2.1
- added back classic parameterless callback support to the InjectionDetector

1.5.2.0
- InjectionDetector callbacks now should accept string with detection cause
- improved Window > CodeStage > Anti-Cheat Toolkit menu appearance
- added new Prefs Editor window mode: as an utility window (via new menu item)
- renamed DEBUG conditional symbols to the ACTK_DEBUG to avoid collisions
- added few more standard prefs to the Prefs Editor ignores
- ObscuredFloat GC allocations on initialization removed (thx pachash)
- ObscuredDecimal GC allocations on initialization removed (thx pachash)
- added proper reaction in case obscured data can't be decrypted in Prefs Editor
- fixed "E" button width when adding new pref
- fixed wrong prefs readout in rare cases in Prefs Editor (thx mcmorry)
- fixed "Cutting off characters" error by checking string length (thx mcmorry)
- fixed errors if working from the Plugins folder in Unity 5.3+ (thx demigiant)
- fixed pref deletion wasn't saved on Mac

1.5.1.0
- added ObscuredPrefs Editor!
    * edit both PlayerPrefs and Obscured prefs in Unity Editor
    * encrypt regular PlayerPrefs with configurable encryption key
    * decrypt ObscuredPrefs to the regular PlayerPrefs
    * search in prefs names
    * sort prefs by name, type, or encrypted state
    * smart prefs reading progress bar
    * shows 50 prefs per page allowing to work with huge collections
    * overwrite notice to prevent data loss
    * coloration of obscured prefs for easier navigation
    * copy prefs to clipboard
    * copy raw obscured prefs to clipboard
    * works on Win, Mac, Linux
- ObscuredFloat/Double now may be assigned to ObscuredInt
- changed the randomize crypto key logic for all obscured types
- added support for the Unity 5.4 SceneManager.sceneLoaded delegate
- more editor actions are undoable now
- minor ObscuredPrefs API changes

1.5.0.6
- SetRawValue and GetRawValue APIs added to the ObscuredPrefs
- fixed possible issues for the Unity 4.7

1.5.0.5
- added warning when new detector self-destroying because of another instance
- added new item to the readme's troubleshooting section
- added explicit operator for the ObscuredFloat > ObscuredDecimal conversion
- fixed InjectionDetecor may not start in Editor in debug mode properly in some cases

1.5.0.4
- WallHackDetector:
    * slightly decreased amount of the generated garbage

1.5.0.3
- WallHackDetector:
    * fixed false positive when used in scenes with fog (thx Andrey Pakhomov!)
    * minor resources usage optimizations

1.5.0.2
- WallHackDetector: 
    * added extra editor check for the proper Physics Layers settings
    * additional debug output
- minor refactorings

1.5.0.1
- ObscuredLong can now be exposed to the inspector
- fixed wrong inspector height of the ObscuredVectors in not wide mode

1.5.0.0
- Project updated to the Unity 4.6
- TestScene changed to better represent ACTk features
- Changes for all detectors:
    * new Detection Event exposed to the inspector
    * new Auto Start option (see readme for details)
    * improvements in placement algorithm
    * fixed incorrect behavior in rare cases after disabling and re-enabling
    * fixed incorrect behavior on scenes switch in some cases
    * inspector appearance improvements and fixes
- WallHackDetector changes:
    * new "Raycast" option to detect shooting through the walls
    * new "Wireframe" option to detect looking through the walls
    * new option maxFalsePositives allows to set allowed detections in a row
    * now you can separately control Rigidbody and Character Controller modules
    * now you can enable and disable all detection modules at any time
    * fixes and improvements
    * ACTK_WALLHACK_DEBUG conditional introduced, see readme for details
- InjectionDetector changes:
    * default whitelist updated (up to Unity 5.3 Beta 2)
    * workaround for the IL2CPP bug (Issue ID 711312)
    * debug conditionals renamed and their defines removed, use Player Settings
    * iOS support removed since this target doesn't allow to inject Assemblies
- Obscured vars changes: 
    * RandomizeCryptoKey() API introduced to hide from 'unknown value' search
    * vars with same value are treated as equal now even if crypto keys differ
    * fixed bold texts in the inspectors of the prefabs with obscured vars
- ObscuredPrefs changes:
    * API change: DeviceID -> DeviceId
    * fixed incorrect Color storage in the unobscuredMode
    * fixed DeleteKey removed PlayerPrefs value with enabled preservePlayerPrefs 
- ObscuredVector2 now can be exposed to the inspector
- ObscuredVector3 now can be exposed to the inspector
- ObscuredDouble now can be exposed to the inspector in Unity 5 and higher
- ObscuredInt and ObscuredUInt are now able to explicitly cast to each other
- ObscuredVector2 now able to implicitly cast to the Vector3
- ACTK_EXCLUDE_OBFUSCATION conditional introduced, see readme for details
- PREVENT_READ_PHONE_STATE is ACTK_PREVENT_READ_PHONE_STATE now
- BehaviorDesigner integration package changes:
    * RandomizeCryptoKey API for all SharedObscured types
    * WallHackDetector added to the example scene
    * fixed absent crypto key in the ObscuredPrefs example scene
    * fixes in comment docs
    * other minor fixes
- PlayMaker integration package changes:
    * added keepAlive and autoDispose options to detector actions
    * fixes in comment docs
- all detectors are now more correctly placed in the Component menu
- you'll not see example scripts in the Component menu anymore
- fixed possible cs0241 error in xxHash.cs file
- fixed API compatibility issues
- removed obsolete editor code
- all editor code has proper Namespaces now
- all examples code has proper Namespaces now
- numerous optimizations and refactorings	
- additions and fixes in readme
- additions and fixes in docs

1.4.1.1
- WallHackDetector: additional auto-correction for rigidbody module
- WallHackDetector: service sandbox now survive scene load if detector has Keep Alive enabled
- WallHackDetector: resources usage improvements
- WallHackDetector: initialization performance improvements
- all ACTk windows are moved to Window > Code Stage > Anti-Cheat Toolkit menu item
- fixes in readme

1.4.1
- added new WallHackDetector! Detects 2 native hacks: Rigidbody and CharacterController patches
- added new InjectionDetected and WallHackDetected Actions for the PlayMaker
- added new WallHackDetected task to the BehaviorDesigner
- InjectionDetector now detects case when there is 0 assemblies in current domain
- fixes in filenames: ObscuredUint.cs to ObscuredUInt.cs, ObscuredUshort.cs to ObscuredUShort.cs
- minor fixes and refactorings
- fixes in readme

1.4.0.2
- fixed ObscuredString editing in multiple objects (thanks VitaMin00)

1.4.0.1
- fixed ObscuredPrefs.SetColor for WebPlayer

1.4.0
- massive ObscuredPrefs update:
  * uint support added
  * Rect support added
  * overall performance and memory usage improvements
  * values are locked to keys now to prevent value swapping
  * all service data in final values is hidden now
  * hashing algorithm replaced with more secure keeping the same performance
  * SetColor/GetColor optimizations
  * Float accuracy increased
  * Double accuracy increased
  * ByteArray support fixed (no more extra length after load)
  * PREVENT_READ_PHONE_STATE define introduced to let you remove Android permission if you don't use lockToDevice
  * legacy ObscuredPlayerPrefs support removed (use previous ACTk version to read them)
  * ForceDeviceID() method replaced with DeviceID property (now you can retrieve current device ID as well)
  * format improvements allowing to easily change it with backwards compatibility
  * refactorings
- ObscuredString.EncryptDecrypt() optimization (affects all Get/Set methods in ObscuredPrefs)
- disabling and enabling started detectors now should pause and resume them
- Anti-Cheat Toolkit Detectors object will self-destroy now ignoring attached scripts and nested objects
- fixed disabled detectors behavior on StartDetection()
- fixed issues with casting obscured vars from objects in the Behavior Designer package
- fixes in readme
- fixes in docs
- other minor fixes

1.3.5
- Anti-Cheat Toolkit now fully supports Opsive Behavior Designer, see readme for details (thanks Justin Mosiman!)
- added new ObscuredPrefsSetNewCryptoKey Action for PlayMaker
- added DetectorsExample scene into the PlayMaker package with SpeedHackDetector usage example
- added ObscuredPrefsExample scene into the PlayMaker package with simple ObscuredPrefs usage example
- fixed obscured vars initialization via SetEncrypted()
- serialization support added to: ObscuredQuaternion, ObscuredVector3 & ObscuredVector2
- redundant alteration callback logic removed from ObscuredPrefs
- removed deprecated code from ObscuredQuaternion, ObscuredVector3 & ObscuredVector2
- plugin abbreviation changed from ACT to ACTk
- minor fixes

1.3.4
- added ObscuredPrefs actions for PlayMaker (thanks kreischweide)
- added SpeedHackDetected action for PlayMaker (thanks kreischweide)
- added Vector2, Quaternion, Color support to the ObscuredPrefs
- detectors keepAlive behavior fixed and improved
- new detectors prefab
- increased detectors placement flexibility: you may put all detectors at one GameObject now
- all detectors will be added to the single Game Object when instantiating from code
- detectors Instance property reworked (can be null now)
- fixed ObscuredCheatingDetector values were not tunable in inspector
- fixed iPhone.vendorIdentifier error on Unity 5 iOS target platform (thanks Mike Messer)
- obscured types now implement IFormattable for better compatibility
- added more tool-tips for detectors
- added troubleshooting section to the readme
- few fixes and additions in readme & API docs
- minor refactoring in ObscuredPrefs

1.3.3
- Flash Player support deprecated, lot of ugly code removed
- Unity 5 fully supported
- plugin project updated to the Unity 4.5 (still can work in earlier Unity versions with few restrictions)
- obscured fields are now bold in inspector if values changed in prefab instance (mimics regular behavior)
- minor refactorings
- some fixes and additions in the readme

1.3.2.1
- optimized inspector exposition for supported obscured types
- fixed array elements names while exposing ObscuredString[] in inspector
- fixed ObscuredString truncation while being shown at inspector in some cases (thanks Stefan Laubenberger)

1.3.2
- ObscuredVector2, ObscuredVector3 and ObscuredQuaternion accuracy increased (consistent on all platforms now)
- added epsilons for ObscuredFloat, Obscured vectors and ObscuredQuaternion in ObscuredCheatDetector
- fixed cheating detection false positives for Obscured vectors and ObscuredQuaternion (thanks Capital j Media)
- fixed ObscuredInt returned 0 in case value matched crypto key regression
- minor optimizations and refactorings

1.3.1
- ObscuredBool inspector support added (requires Unity 4.5)
- new public API for all basic Obscured types: ApplyNewCryptoKey(), see docs for more details
- Significant changes in SpeedHackDetector:
  * Cool Down system introduced. Read more in pdf and "coolDown" field API docs (thanks Cliff Cawley)
  * detection checks period is not affected by speed hack anymore (now it uses system date timer)
  * fixed speed hack wasn't detected in some cases
  * fixed false positives in some cases
  * fixed extra calculations on first detection check in the SpeedHackDetector
  * fixed incorrect datetime change detection
  * fixed continuous detects after first detect even if speed hack was removed (thanks Cliff Cawley)
  * speed hack detection log message now appears in debug builds only
  * application pause handled correctly now
- all basic Obscured types are serializable now (only binary serialization supported)
- ObscuredBool cheating detection is now supported in the Flash Player.
- fixed ObscuredInt default value issues
- fixed null reference error while using exposed ObscuredFloat fields to the inspector without default value
- fixed incorrectly decrypted ObscuredString values in inspector when used without default values
- fixed cheating detection false positives for Obscured variables while being exposed to the editor
- fixed possible cheating detection false positives for ObscuredVector2, ObscuredVector3 and ObscuredQuaternion
- fixed GetEncrypted() after SetNewCryptoKey() in ObscuredString didn't counted new crypto key
- removed redundant [InitializeOnLoad] attribute from ActPostprocessor
- minor refactorings in all Obscured types
- fixes and additions in the docs

1.3.0
- ObscuredString fields are now editable from inspector
- ObscuredInt and ObscuredFloat fields are now editable from inspector (Unity 4.5 or higher required)
  * in case int->ObscuredInt \ float->ObscuredFloat replacement, old values set in inspector will be reset to defaults!
  * experimental inspector support for the obscured structs instances, report any bugs please
- ObscuredCheatingDetector implemented
  * central detector for all Obscured types (except ObscuredPrefs) cheating detection
  * replacement for legacy onCheatingDetected Action callback
- Injection Detector user-defined Whitelist Editor implemented
  * allows gather assembly data both from files and manually (for assemblies made in realtime)
- ObscuredPrefs.ForceDeviceID(string) method introduced
  * useful if you have server-side authorization, especially on iOS (we have no reliable way to get device ID there)
- Injection Detector data file is restored now if it was accidentally removed
- public API StopMonitoring was renamed to the StopDetection for all detectors
- most of Vector3 operators are now supported by ObscuredVector3
- fixed possible false positives in Speed Hack Detector caused by system time / date change
- fixed exception in ActPostprocessor while trying to write into the read-only file (thanks Sebastiano Mandalà)
- fixed possible false positives for ObscuredVector3, ObscuredVector2, ObscuredQuaternion cheating detection.
- added ability to force Injection Detector data collection for unsupported platforms for debugging
- Injection Detector will be re-enabled after switching from unsupported target if it was enabled previously
- significant refactorings in the detectors code
- service data cleanup code simplified
- improved removing of obsolete editor prefs used by ACT
- refactorings in the Editor scripts and file structure (remove prev version before updating to avoid collisions!)
- fixed possible name conflicts: MenuItem, Action
- minor fixes and optimizations
- minor fixes in documentation

1.2.9
- cheating detection added to the ObscuredQuaternion, ObscuredVector3, ObscuredVector2, ObscuredString
- fixed rare critical bug in ++ and -- operators of some obscured types leading to false detections (huge thanks redux!)
- fixed ObscuredPrefs.HasKey() and ObscuredPrefs.DeleteKey() were not count regular PlayerPrefs keys (thanks Sashko)
- fixed possible access issue while reading from AllowedAssemblies.bytes file in editor (thanks Sabastiano)
- fixed possible fndid file corruption leading to the false positives in Injection Detector (thanks Sabastiano)
- access to the x, y, z fields was added to the ObscuredVector3 (access by index was implemented as well)
- access to the x and y fields was added to the ObscuredVector2 (access by index was implemented as well)
- DetectorsUsageExample script was moved from Anti-Cheat Toolkit Detectors prefab to the separate GameObject
- minor fixes in the API docs

1.2.8
- New obscured types with cheating detection implemented
  * ObscuredSByte
  * ObscuredChar
  * ObscuredDecimal (Flash Player is not supported, use ObscuredFloat instead)
  * ObscuredULong
- cheating detection added to these existing obscured types
  * ObscuredUInt
  * ObscuredLong
  * ObscuredDouble
  * ObscuredBool (except Flash Player: bug in exported pool manager code doesn't allow to use nullables)
- minor public API changes for few obscured types
- fixed SpeedHackDetector error while playing test scene in editor with Flash Player target
- fixed possible false positives of SpeedHackDetector in Flash Player
- fixed Flash Player implementation of the ObscuredUShort.GetHashCode()
- another minor improvements in flash implementation of SpeedHackDetector
- other minor fixes and optimizations

1.2.7
- InjectionDetector introduced
  * allows to detect if someone injects any managed dll into your app
- ObscuredShort and ObscuredUShort with cheating detection implemented
- IntegrityChecker removed
- significant changes and fixes in API documentation (should be prettier and more detailed now)
- DeviceLockLevel enum moved into the ObscuredPrefs class, thus now it should be reached as ObscuredPrefs.DeviceLockLevel
- SpeedHackDetector's StartMonitoring methods were renamed to StartDetection ones
- minor improvements in flash implementation of SpeedHackDetector
- ObscuredUint was renamed to ObscuredUInt
- minor files structure changes, please, make clean update (completely remove previous version before updating)
- minor fixes and optimizations

1.2.6u2
- fixed possible false positives of cheating detection in Obscured types
- minor API docs fixes

1.2.6u1
- extra traces in ActionScript code were commented out

1.2.6
- flash build support for SpeedHackDetector implemented
- ObscuredVector2 and ObscuredQuaternion implemented
- cheating detection added to the ObscuredByte
- SpeedHackDetector now awaits Action delegate instead of MonoBehaviour and method name. Please, read API docs for details
- ObscuredPrefs.readForeignSaves field introduced
  * allows to read saves locked to other device and onPossibleForeignSavesDetected action still will be fired
- deprecated PlayerPrefsObscured was removed completely, use ObscuredPrefs instead
- ObscuredFloat slightly optimized
- ObscuredDouble slightly optimized
- all default arguments were replaced by methods overloading for additional compatibility with UnityScript (again)
- minor fixes in API docs

1.2.5u1
- SpeedHackDetector.Instance is now public and may be used to set interval, autoDispose, etc. from code.

1.2.5
- SpeedHackDetector introduced! Allows you to react on Cheat Engine's speed hack
  * some other speed hack tools may be detected too
  * use GameObject->Create Other->Code Stage->Speed Hack Detector menu item to add detector in scene
  * use any public SpeedHackDetector API to automatically add it to scene
- added cheating detection to the ObscuredInt and ObscuredFloat (use Obscured*.onCheatingDetected)
- migration from PlayerPrefs to ObscuredPrefs became super easy: just replace PlayerPrefs with ObscuredPrefs
  and read \ write any data as usual, no more manual PlayerPrefs to ObscuredPrefs conversion!
  * all data saved with regular PlayerPrefs will be automatically encrypted with ObscuredPrefs on read and
    original PlayerPrefs data will be deleted
  * Set ObscuredPrefs.preservePlayerPrefs to true to prevent original PlayerPrefs data deletion
- migration from legacy PlayerPrefsObscured to ObscuredPrefs became smoother: data, written with PlayerPrefsObscured
  now converts to the new format while readed with ObscuredPrefs (previously - old legacy keys were left untouched 
  after migration).
- ObscuredPrefs.unobscuredMode implemented
  * allows to write all data unobscured, for testing purposes
  * thus it works in Editor only
  * breaks PlayerPrefs to ObscuredPrefs migration (reminder: in Editor)
- ObscuredVector3 and ObscuredBool implemented!
- data saved in TestScene now cleans up on application quit
- ObscuredPrefs overfill in Web Player no longer breaks entire game process (and logs error in such case)
- added selftests and performance tests code to the example scene (disabled by default) for debugging and 
  making wise choices ;)
- ObscuredString default crypto key was changed. Set new key to 4444 before using SetEncrypted() to set data got with
  GetEncrypted in previous versions.
- fixed compilation error of WP8 build
- fixed initialization of some Obscured types
- fixed ObscuredString equality operator implementation
- minor fixes
- minor docs fixes

1.2.0
- project updated to Unity 4.2.2
- new ObscuredPrefs introduced instead of old PlayerPrefsObscured
  * PlayerPrefsObscured is no more supported, please, make sure to save all your new data using ObscuredPrefs
  * all data saved with ObscuredPrefs will not be accessible using deprecated PlayerPrefsObscured!
  * all data saved with deprecated PlayerPrefsObscured is accessible with ObscuredPrefs automatically as fallback
    (will be removed in future)
  * new flexible lock data to device feature introduced, read more in api docs and readme
  * now saved data may be recovered in emergency cases (like device ID change after updating from iOS 6 to iOS 7)
  * attempt to prevent device id change after iOS6 to iOS7 update (works in some cases only)
  * key is now encrypted with common encryptionKey (previously default key for ObscuredString was used)
  * accessing data saved with regular PlayerPrefs now generates warning with additional information
  * saves alteration can be detected now, use ObscuredPrefs.onAlterationDetected (see ObscuredPrefsTest)
  * foreign saves usage detection added, use ObscuredPrefs.onPossibleForeignSavesDetected (see ObscuredPrefsTest)
  * added example of storing encryption key outside of the IL bytecode (increasing its security level)
    (see ObscuredPrefsTest)
  * added support of these types: long, bool, byte[], Vector3
- ObscuredByte and ObscuredLong added
- equality operations (==, !=, .Equals()) are now supported in all simple Obscured types
- all default arguments were replaced by methods overloading for additional compatibility with UnityScript
- other minor fixes
- few fixes in docs

1.1.0u1
- fixed ObscuredDouble not worked correctly
- fixed link to ObscuredDouble missed in object Tester in the test scene

1.1.0
- no more dlls, plugin now comes with full source code included!
- Yey, Flash Player exporter support is back! :D IntegrityChecker, ObscuredDouble and 
  PlayerPrefsObscured.lockToDevice are not supported there though.
- added increment and decrement operators support to the numeric obscured types (thanks Yuri Saveliev)
- added ObscuredDouble! Not supported in FlashPlayer (use ObscuredFloat instead)! (thanks Andriy Pidvirnyy)
- added correct analogues of toString() methods to some of the Obscured types
- added PlayerPrefsObscured.ForceLockToDeviceInit() method to call device ID obtanation (noticably slow at first
  call) process at desirable time (at splash screen time for example)
- added initial support for Windows Store (Metro) Apps (needs testing)
- added initial support for Windows Phone 8 (needs testing, thanks friuns3)

- PlayerPrefsObscured.lockToDevice field is now property, so please use PlayerPrefsObscured.LockToDevice instead.
  Sorry for inconvenience.
- removed unsafe code in ObscuredFloat
- fixed possible placement in memory not obscured float while using ObscuredFloat, oops :P
- fixed possible data loss when using values equal to the crypto keys
- attempt to fix "get_deviceUniqueIdentifier can only be called from the main thread" error (thanks ecc83)
- fixed issue with different line endings on different platforms in IntegrityChecker causing it to think
  assemblies are not valid
- fixed incorrect line breaks in the xml docs
- assemblies signing now can't be enabled on known unsupported platforms
- assemblies signing will be disabled after switching to the known unsupported platform
- assemblies signing will be disabled if Stripping Level in Player Settings is not set to "Disabled"
- changed PlayerPrefsObscured default encryption key. Use SetNewCryptoKey(e806f69e7aea3ee30fe27a6abfae967f) to
  read any data saved in previous ACT version with default key.
- docs were fixed a bit
- removed obsolete methods in PlayerPrefsObscured
- minor fixes


1.0.1.2
- fixed .meta files handling
- moved Anti-Cheat Toolkit/Options menu item to the Window/Anti-Cheat Toolkit/Options
- moved plugin into CodeStage directory (to compact placing of any future plugins I'll release)
- reduced ObscuredInt default key length
- attempt to fix async assemblies reloading issue


1.0.1.1
- Added assemblies signing process duration estimation
- Fixed issues with Anti-Cheat Toolkit .dll import in Unity 4.x


1.0.1.0
- Added Assemblies signing functionality * EXPERIMENTAL *
- Fixed web player detection, now ObscuredFloat can be used in static conditions (thanks Andriy Pidvirnyy)


1.0.0.9
- Fixed data loss in PlayerPrefsObscured (use Get*Deprecated to read data saved with ACT 1.0.0.8)


1.0.0.8
- PlayerPrefsObscured now able to lock saved data to the current device. See PlayerPrefsObscured.lockToDevice
  field description in API docs.
- Improved PlayerPrefsObscured stability and obscuration strength (use Get*Deprecated() methods to load data,
  saved with ACT 1.0.0.6 or earlier)
- PlayerPrefsObscured now has own encryption key. Use PlayerPrefsObscured.SetNewCryptoKey() to change it from
  default value.
- Created home page for ACT: https://codestage.net/uas/actk/


1.0.0.7
- Fixed error in ObscuredString (now it works in WebPlayer without errors).
- Fixed error in ObscuredFloat (now it works in WebPlayer without errors).
- ObscuredFloat is memory wiser now.
- ObscuredFloat.SetNewCryptoKey() accepts int now (was long).
- Added changelog :)