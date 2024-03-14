#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitriy Yukhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using System.Collections.Generic;
	using Common;
	using Storage;
	using UnityEditor;
	using UnityEngine;

	internal partial class ACTkPrefsEditor
	{
		// -------------------------------------------------------------------
		// UI
		// -------------------------------------------------------------------
		
		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Prefs Editor as Tab...", false, 500)]
		internal static void ShowWindow()
		{
			var myself = GetWindow<ACTkPrefsEditor>(false, "Prefs Editor", true);
			myself.minSize = new Vector2(500, 300);
			myself.RefreshData();
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Prefs Editor as Utility...", false, 501)]
		internal static void ShowWindowUtility()
		{
			var myself = GetWindow<ACTkPrefsEditor>(true, "Prefs Editor", true);
			myself.minSize = new Vector2(500, 300);
			myself.RefreshData();
		}
		
		public void AddItemsToMenu(GenericMenu menu)
		{
			menu.AddItem(new GUIContent("Copy Player Prefs path"), false, OnCopyPrefsPath);
			menu.AddSeparator("");
		}

		private void DrawGUI()
		{
			if (allRecords == null) allRecords = new List<PrefsRecord>();
			if (filteredRecords == null) filteredRecords = new List<PrefsRecord>();

			DrawToolbar();

			if (addingNewRecord)
			{
				DrawAddNewRecordPanel();
			}

			using (GUITools.Vertical(GUITools.PanelWithBackground))
			{
				GUILayout.Space(5);

				DrawRecordsPages();

				GUILayout.Space(5);

				DrawFooterButtons();
			}
		}

		private void DrawToolbar()
		{
			using (GUITools.Horizontal(GUITools.Toolbar))
			{
				if (GUILayout.Button(new GUIContent("+", "Create new prefs record."), EditorStyles.toolbarButton, GUILayout.Width(20)))
				{
					addingNewRecord = true;
				}

				if (GUILayout.Button(new GUIContent("Refresh", "Re-read and re-parse all prefs."), EditorStyles.toolbarButton, GUILayout.Width(55)))
				{
					RefreshData();
					GUIUtility.keyboardControl = 0;
					scrollPosition = Vector2.zero;
					recordsCurrentPage = 0;
				}

				EditorGUI.BeginChangeCheck();
				sortingType = (SortingType)EditorGUILayout.EnumPopup(sortingType, EditorStyles.toolbarDropDown, GUILayout.Width(110));
				if (EditorGUI.EndChangeCheck())
				{
					ApplySorting();
				}

				GUILayout.Space(10);

				EditorGUI.BeginChangeCheck();
				searchPattern = GUITools.SearchToolbar(searchPattern);
				if (EditorGUI.EndChangeCheck())
				{
					ApplyFiltering();
				}
			}
		}
		
		private void DrawAddNewRecordPanel()
		{
			using (GUITools.Horizontal(GUITools.PanelWithBackground))
			{
				string[] types = {"String", "Int", "Float"};
				newRecordType = EditorGUILayout.Popup(newRecordType, types, GUILayout.Width(55));

				newRecordEncrypted = GUILayout.Toggle(newRecordEncrypted,
					new GUIContent("E", "Create new pref as encrypted ObscuredPref?"), GUITools.CompactButton,
					GUILayout.Width(25));

				var guiColor = GUI.color;
				if (newRecordEncrypted)
				{
					GUI.color = obscuredColor;
				}

				GUILayout.Label("Key:", GUILayout.ExpandWidth(false));
				newRecordKey = EditorGUILayout.TextField(newRecordKey);
				GUILayout.Label("Value:", GUILayout.ExpandWidth(false));

				if (newRecordType == 0)
				{
					newRecordStringValue = EditorGUILayout.TextField(newRecordStringValue);
				}
				else if (newRecordType == 1)
				{
					newRecordIntValue = EditorGUILayout.IntField(newRecordIntValue);
				}
				else
				{
					newRecordFloatValue = EditorGUILayout.FloatField(newRecordFloatValue);
				}

				GUI.color = guiColor;

				if (GUILayout.Button("OK", GUITools.CompactButton, GUILayout.Width(30)))
				{
					AddNewRecord();
				}

				if (GUILayout.Button("Cancel", GUITools.CompactButton, GUILayout.Width(60)))
				{
					CloseNewRecordPanel();
				}
			}
		}

		private void CloseNewRecordPanel()
		{
			addingNewRecord = false;
			newRecordKey = string.Empty;
			newRecordStringValue = string.Empty;
			newRecordIntValue = 0;
			newRecordFloatValue = 0;
			GUIUtility.keyboardControl = 0;
		}

		private void DrawFooterButtons()
		{
			GUI.enabled = filteredRecords.Count > 0;
			using (GUITools.Horizontal())
			{
				if (GUILayout.Button("Encrypt ALL", GUITools.CompactButton))
				{
					PerformEncryptAll();
				}

				if (GUILayout.Button("Decrypt ALL", GUITools.CompactButton))
				{
					PerformDecryptAll();
				}

				if (GUILayout.Button("Save ALL", GUITools.CompactButton))
				{
					PerformSaveAll();
				}

				if (GUILayout.Button("Delete ALL", GUITools.CompactButton))
				{
					PerformDeleteAll();
				}
			}
			GUI.enabled = true;
		}

		private void DrawRecordsPages()
		{
			recordsTotalPages = Math.Max(1,(int)Math.Ceiling((double)filteredRecords.Count / RecordsPerPage));

			if (recordsCurrentPage < 0) recordsCurrentPage = 0;
			if (recordsCurrentPage + 1 > recordsTotalPages) recordsCurrentPage = recordsTotalPages - 1;

			var fromRecord = recordsCurrentPage * RecordsPerPage;
			var toRecord = fromRecord + Math.Min(RecordsPerPage, filteredRecords.Count - fromRecord);

			if (recordsTotalPages > 1)
			{
				GUILayout.Label("Prefs " + fromRecord + " - " + toRecord + " from " + filteredRecords.Count);
			}

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			for (var i = fromRecord; i < toRecord; i++)
			{
				DrawRecord(i, out var recordRemoved);
				if (recordRemoved)
				{
					break;
				}
			}
			GUILayout.EndScrollView();

			if (recordsTotalPages <= 1) return;

			GUILayout.Space(5);
			using (GUITools.Horizontal())
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = recordsCurrentPage > 0;
				if (GUILayout.Button("<<", GUILayout.Width(50)))
				{
					RemoveNotification();
					recordsCurrentPage = 0;
					scrollPosition = Vector2.zero;
				}
				if (GUILayout.Button("<", GUILayout.Width(50)))
				{
					RemoveNotification();
					recordsCurrentPage--;
					scrollPosition = Vector2.zero;
				}
				GUI.enabled = true;
				GUILayout.Label(recordsCurrentPage + 1 + " of " + recordsTotalPages, GUITools.CenteredLabel, GUILayout.Width(100));
				GUI.enabled = recordsCurrentPage < recordsTotalPages - 1;
				if (GUILayout.Button(">", GUILayout.Width(50)))
				{
					RemoveNotification();
					recordsCurrentPage++;
					scrollPosition = Vector2.zero;
				}
				if (GUILayout.Button(">>", GUILayout.Width(50)))
				{
					RemoveNotification();
					recordsCurrentPage = recordsTotalPages - 1;
					scrollPosition = Vector2.zero;
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
			}
		}

		private void DrawRecord(int recordIndex, out bool recordRemoved)
		{
			recordRemoved = false;
			var record = filteredRecords[recordIndex];
			var guiColor = GUI.color;

			using (GUITools.Horizontal())
			{
				if (GUILayout.Button(new GUIContent("X", "Delete this pref."), GUITools.CompactButton, GUILayout.Width(20)))
				{
					DeleteRecord();
					recordRemoved = true;
					return;
				}

				GUI.enabled = (record.dirtyValue || record.dirtyKey) && record.prefType != PrefsRecord.PrefsType.Unknown;
				if (GUILayout.Button(new GUIContent("S", "Save changes in this pref."), GUITools.CompactButton, GUILayout.Width(20)))
				{
					record.Save();
					GUIUtility.keyboardControl = 0;
				}

				DrawEncryptDecryptButtons();

				if (GUILayout.Button(new GUIContent("...", "Other operations"), GUITools.CompactButton, GUILayout.Width(25)))
					ShowOtherMenu(record);
				
				if (record.Obscured)
					GUI.color = obscuredColor;

				GUI.enabled = record.prefType != PrefsRecord.PrefsType.Unknown;

				DrawKey();
				DrawValue();
				
				GUI.color = guiColor;
				GUI.enabled = true;

				EditorGUILayout.LabelField(record.DisplayType, GUILayout.Width(70));
			}
			
			void DeleteRecord()
			{
				record.Delete();
				allRecords.Remove(record);
				filteredRecords.Remove(record);
			}
			
			void DrawEncryptDecryptButtons()
			{
				GUI.enabled = record.prefType != PrefsRecord.PrefsType.Unknown;
				if (record.Obscured)
				{
					GUI.enabled &= record.obscuredType == StorageDataType.String ||
								   record.obscuredType == StorageDataType.Int32 ||
								   record.obscuredType == StorageDataType.Single;
					if (GUILayout.Button(new GUIContent("D", "Decrypt this pref using ObscuredPrefs"), GUITools.CompactButton, GUILayout.Width(25)))
					{
						record.Decrypt();
						GUIUtility.keyboardControl = 0;
					}
				}
				else
				{
					if (GUILayout.Button(new GUIContent("E", "Encrypt this pref using ObscuredPrefs"), GUITools.CompactButton, GUILayout.Width(25)))
					{
						record.Encrypt();
						GUIUtility.keyboardControl = 0;
					}
				}
				GUI.enabled = true;
			}
			
			void DrawKey()
			{
				if (record.Obscured && !(record.obscuredType == StorageDataType.String ||
										 record.obscuredType == StorageDataType.Int32 ||
										 record.obscuredType == StorageDataType.Single))
				{
					GUI.enabled = false;
					EditorGUILayout.TextField(record.Key, GUILayout.MaxWidth(200), GUILayout.MinWidth(50));
					GUI.enabled = record.prefType != PrefsRecord.PrefsType.Unknown;
				}
				else
				{
					record.Key = EditorGUILayout.TextField(record.Key, GUILayout.MaxWidth(200), GUILayout.MinWidth(50));
				}
			}

			void DrawValue()
			{
				if (record.prefType == PrefsRecord.PrefsType.String && !record.Obscured || record.Obscured && record.obscuredType == StorageDataType.String)
				{
					// to avoid TextMeshGenerator error because of too much characters
					if (record.StringValue.Length > 16382)
					{
						GUI.enabled = false;
						EditorGUILayout.TextField(StringTooLong, GUILayout.MinWidth(150));
						GUI.enabled = record.prefType != PrefsRecord.PrefsType.Unknown;
					}
					else
					{
						record.StringValue = EditorGUILayout.TextField(record.StringValue, GUILayout.MinWidth(150));
					}
				}
				else if (record.prefType == PrefsRecord.PrefsType.Int || (record.Obscured && record.obscuredType == StorageDataType.Int32))
				{
					record.IntValue = EditorGUILayout.IntField(record.IntValue, GUILayout.MinWidth(150));
				}
				else if (record.prefType == PrefsRecord.PrefsType.Float || (record.Obscured && record.obscuredType == StorageDataType.Single))
				{
					record.FloatValue = EditorGUILayout.FloatField(record.FloatValue, GUILayout.MinWidth(150));
				}
				else if (record.Obscured)
				{
					GUI.enabled = false;
					EditorGUILayout.TextField(UnsupportedValueDescription, GUILayout.MinWidth(150));
					GUI.enabled = record.prefType != PrefsRecord.PrefsType.Unknown;
				}
				else
				{
					GUI.enabled = false;
					EditorGUILayout.TextField(UnknownValueDescription, GUILayout.MinWidth(150));
					GUI.enabled = record.prefType != PrefsRecord.PrefsType.Unknown;
				}
			}
		}

		private static void ShowOtherMenu(PrefsRecord record)
		{
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Copy to clipboard"), false, () =>
			{
				EditorGUIUtility.systemCopyBuffer = record.ToString();
			});

			if (record.Obscured)
			{
				menu.AddItem(new GUIContent("Copy obscured raw data to clipboard"), false, () =>
				{
					EditorGUIUtility.systemCopyBuffer = record.ToString(true);
				});
			}

			var valueToPaste = EditorGUIUtility.systemCopyBuffer;
			switch (record.prefType)
			{
				case PrefsRecord.PrefsType.Unknown:
					break;
				case PrefsRecord.PrefsType.String:
					if (!record.Obscured || record.IsEditableObscuredValue())
					{
						menu.AddItem(new GUIContent("Paste string value from clipboard"), false, () =>
						{
							record.StringValue = valueToPaste;
						});
					}
					break;
				case PrefsRecord.PrefsType.Int:
					menu.AddItem(new GUIContent("Paste int value from clipboard"), false, () =>
					{
						if (int.TryParse(valueToPaste, out var pastedInt))
						{
							record.IntValue = pastedInt;
						}
						else
						{
							Debug.LogWarning(ACTk.LogPrefix + "Can't paste this value to Int pref:\n" + valueToPaste);
						}
					});
					break;
				case PrefsRecord.PrefsType.Float:
					menu.AddItem(new GUIContent("Paste float value from clipboard"), false, () =>
					{
						if (float.TryParse(valueToPaste, out var pastedFloat))
						{
							record.FloatValue = pastedFloat;
						}
						else
						{
							Debug.LogWarning(ACTk.LogPrefix + "Can't paste this value to Float pref:\n" + valueToPaste);
						}
					});
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			menu.ShowAsContext();
		}
	}
}