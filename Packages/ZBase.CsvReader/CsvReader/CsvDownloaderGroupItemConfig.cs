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
using CsvReader;
using UnityEditor;
using UnityEngine;

namespace CsvDownloader
{
    [Serializable]
    public class CsvDownloaderItemConfig
    {
        [ReadOnly]
        public string csvName;
        public bool selected;
    }

    public class CsvDownloaderGroupItemConfig : SerializedScriptableObject
    {
        public bool isDownloading;

        [PropertyOrder(0)]
        public string googleSpreadSheetId;

        [PropertyOrder(2)]
        public string subFolder;

        [PropertyOrder(100)]
        public List<CsvDownloaderItemConfig> items;

        [HorizontalGroup("buttons")]
        [PropertyOrder(3)]
        [Button("Select All")]
        public void SelectAll()
        {
            foreach (var item in items)
            {
                item.selected = true;
            }
        }

        [HorizontalGroup("buttons")]
        [PropertyOrder(3)]
        [Button("UnSelect All")]
        public void UnSelectAll()
        {
            foreach (var item in items)
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
        public void LoadAll()
        {
            if (isDownloading)
                return;

            isDownloading = true;
            LoadAsync(true).Forget();
        }


        private async UniTaskVoid LoadAsync(bool selectedAll)
        {
            var credential = GoogleCredential.
                FromJson(CsvConfig.Instance.credentialFile.text).
                CreateScoped(new[] { DriveService.Scope.DriveReadonly });

            using (var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            }))
            {

                var sheetReq = service.Spreadsheets.Get(googleSpreadSheetId);
                sheetReq.Fields = "properties,sheets(properties,data.rowData.values.formattedValue)";
                var spreadSheet = await sheetReq.ExecuteAsync();

                Debug.LogWarning("Finished download spreadsheet");
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

                if (selectedAll)
                {
                    items.Clear();
                }

                foreach (var item in _pages)
                {
                    if(!selectedAll && !items.Any(x => x.selected && x.csvName == item.Key))
                    {
                        continue;
                    }

                    string csvText = string.Empty;

                    var pages = item.Value;

                    bool isAddedFirstRow = false;

                    foreach (var page in pages)
                    {
                        var grid = page.grid;

                        if (grid.RowData == null)
                            continue;

                        if (grid.RowData.Count == 0)
                            continue;

                        var indexCount = 0;
                        foreach (var row in grid.RowData)
                        {
                            if (row.Values == null)
                                continue;

                            if(row.Values.Count == 0)
                                continue;

                            if (!string.IsNullOrEmpty(row.Values[0].FormattedValue) && row.Values[0].FormattedValue.Contains(CsvDownloaderUtils.Comment))
                                continue;

                            if(!row.Values.Any(x => !string.IsNullOrEmpty(x.FormattedValue)))
                                continue;

                            if (isAddedFirstRow)
                            {
                                if(indexCount == 0)
                                {
                                    indexCount++;
                                    continue;
                                }
                            }
                            else
                            {
                                if(indexCount == 0)
                                {

                                }
                            }

                            isAddedFirstRow = true;

                            for (int i = 0; i < row.Values.Count; i++)
                            {
                                var cell = row.Values[i];

                                var plusText = string.IsNullOrEmpty(cell.FormattedValue) ? string.Empty : cell.FormattedValue;

                                if(i == 0)
                                {
                                    csvText += plusText;
                                }
                                else
                                {
                                    csvText += ("," + plusText);
                                }

                                if(i == row.Values.Count - 1)
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
                        File.WriteAllText(path, finalCsv);
                        var text = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));

                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);
                        EditorUtility.SetDirty(text);

                        if (selectedAll)
                        {
                            Debug.LogWarning("Add file :" + path);
                            items.Add(new CsvDownloaderItemConfig { csvName = item.Key, selected = true });
                        }

                        Debug.LogWarning("Finished write file :" + finalCsv);
                    }
                    catch 
                    {
                        Debug.LogWarning("Error " + path);    
                    }
                }
            }

            isDownloading = false;
        }

        private class CsvPage
        {
            public readonly GridData grid;

            public string SubName { get; }

            public CsvPage(Sheet gSheet, string subName)
            {
                grid = gSheet.Data.First();
                SubName = subName;
            }
        }
    }
}
