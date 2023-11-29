#if GOOGLE_SHEET_DOWNLOADER
using Cysharp.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CsvReader
{
    [Serializable]
    public class SheetConfig
    {
        [ReadOnly] public string csvName;
        public bool selected;
    }

    public class GoogleSheetGroupConfig : SerializedScriptableObject
    {
        public bool isDownloading;

        [PropertyOrder(0)] public string googleSheetId;

        [PropertyOrder(2)] public string subFolder;

        [PropertyOrder(100)] public List<SheetConfig> sheetsConfig;

        [HorizontalGroup("buttons")]
        [PropertyOrder(3)]
        [Button("Select All")]
        public void SelectAll()
        {
            foreach (var item in this.sheetsConfig)
            {
                item.selected = true;
            }
        }

        [HorizontalGroup("buttons")]
        [PropertyOrder(3)]
        [Button("UnSelect All")]
        public void UnSelectAll()
        {
            foreach (var item in this.sheetsConfig)
            {
                item.selected = false;
            }
        }

        [HorizontalGroup("buttons")]
        [PropertyOrder(3)]
        [Button("Load Selected")]
        public void Load()
        {
            if (isDownloading)
                return;

            isDownloading = true;
            LoadAsync(false).Forget();
        }
        
        [InfoBox("If is downloading is on, it will not run the method load")]
        [PropertyOrder(4)]
        [Button("Load All")]
        public async UniTask LoadAll()
        {
            if (isDownloading)
                return;

            isDownloading = true;
            await LoadAsync(true);
        }

        private async UniTask LoadAsync(bool selectedAll)
        {
            var credential = GoogleCredential.FromJson(CsvConfig.Instance.credentialFile.text)
                .CreateScoped(DriveService.Scope.DriveReadonly);

            using (var service =
                   new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = credential }))
            {
                var sheetReq = service.Spreadsheets.Get(this.googleSheetId);
                sheetReq.Fields = "properties,sheets(properties,data.rowData.values.formattedValue)";
                var spreadSheet = await sheetReq.ExecuteAsync();

                Debug.Log("Finished download spreadsheet");
                Dictionary<string, List<CsvPage>> _pages = new();

                foreach (var gSheet in spreadSheet.Sheets)
                {
                    if (!char.IsLetterOrDigit(gSheet.Properties.Title.FirstOrDefault()))
                        continue;

                    var (sheetName, subName) = CsvDownloaderUtils.ParseSheetName(gSheet.Properties.Title);

                    if (!_pages.TryGetValue(sheetName, out var sheetList))
                    {
                        sheetList = new List<CsvPage>();
                        _pages.Add(sheetName, sheetList);
                    }

                    sheetList.Add(new CsvPage(gSheet, subName));
                }

                var copySheetsConfig = new List<SheetConfig>(this.sheetsConfig);
                
                // update Sheets Config - Clear and check new sheet
                this.sheetsConfig.Clear();

                foreach (KeyValuePair<string,List<CsvPage>> page in _pages)
                {
                    var cacheConfig = copySheetsConfig.Find(x => x.csvName == page.Key);
                    this.sheetsConfig.Add(cacheConfig ?? new SheetConfig { csvName = page.Key, selected = true});
                }

                foreach (var item in _pages)
                {
                    if (!selectedAll && !this.sheetsConfig.Any(x => x.selected && x.csvName == item.Key))
                    {
                        continue;
                    }

                    string csvText = string.Empty;

                    var pages = item.Value;

                    bool isAddedFirstRow = false;

                    foreach (var page in pages)
                    {
                        var grid = page.Grid;

                        if (grid.RowData == null)
                            continue;

                        if (grid.RowData.Count == 0)
                            continue;

                        var indexCount = 0;
                        foreach (var row in grid.RowData)
                        {
                            if (row.Values == null)
                                continue;

                            if (row.Values.Count == 0)
                                continue;

                            if (!string.IsNullOrEmpty(row.Values[0].FormattedValue) &&
                                row.Values[0].FormattedValue.Contains(CsvDownloaderUtils.Comment))
                                continue;

                            if (row.Values.All(x => string.IsNullOrEmpty(x.FormattedValue)))
                                continue;

                            if (isAddedFirstRow)
                            {
                                if (indexCount == 0)
                                {
                                    indexCount++;
                                    continue;
                                }
                            }
                            else
                            {
                                if (indexCount == 0)
                                {
                                }
                            }

                            isAddedFirstRow = true;

                            for (int i = 0; i < row.Values.Count; i++)
                            {
                                var cell = row.Values[i];

                                var plusText = string.IsNullOrEmpty(cell.FormattedValue)
                                    ? string.Empty
                                    : cell.FormattedValue;

                                if (i == 0)
                                {
                                    csvText += plusText;
                                }
                                else
                                {
                                    csvText += ("," + plusText);
                                }

                                if (i == row.Values.Count - 1)
                                {
                                    csvText += "\r\n";
                                }
                            }

                            indexCount++;
                        }
                    }

                    var finalCsv = csvText;
                    var folderPath = CsvConfig.Instance.csvPath + "/" + subFolder;

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    var path = folderPath + "/" + item.Key + ".csv";

                    try
                    {
                        if (File.Exists(path))
                        {
                            // If the file exists, read its content for comparison
                            string existingData = await File.ReadAllTextAsync(path);

                            if (existingData != null && existingData.Equals(finalCsv))
                            {
                                // Google Sheet is up to date
                                continue;
                            }

                            Debug.Log($"Changed File: + <color=green>{path}</color>");
                        }
                        else
                        {
                            Debug.Log($"New File: + <color=green>{path}</color>");
                        }

                        await File.WriteAllTextAsync(path, finalCsv);

                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);
                        var text = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
                        EditorUtility.SetDirty(text);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Error " + e);
                    }
                }
            }

            isDownloading = false;
        }

        private class CsvPage
        {
            public readonly GridData Grid;

            public string SubName { get; }

            public CsvPage(Sheet gSheet, string subName)
            {
                this.Grid = gSheet.Data.First();
                SubName = subName;
            }
        }
    }
}
#endif