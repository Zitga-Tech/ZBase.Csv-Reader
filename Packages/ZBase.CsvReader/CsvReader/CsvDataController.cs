#if UNITY_EDITOR
#nullable enable
using System;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace CsvReader
{
    [GlobalConfig("Assets/Plugins/CsvReader")]
    public class CsvDataController : GlobalConfig<CsvDataController>
    {
        #region Reader Controller

        [Title("Create new csv data")]
#pragma warning disable CS8618
        [InlineButton("CreateNewCsvData", "Create New")]
        [SuffixLabel("Data Name", true)]
        [HideLabel]
        [Required]
        public string csvDataName;
#pragma warning restore CS8618

        public void CreateNewCsvData()
        {
            if (csvDataName == null || csvDataName.Equals(string.Empty))
            {
                throw new ArgumentException("Config Name is null");
            }

            var filePath = $"{CsvConfig.Instance.readerConfigPath}/{csvDataName}.asset";
            var type = typeof(CsvData);
            var gm = AssetDatabase.LoadAssetAtPath(filePath, type);
            if (gm == null)
            {
                gm = CreateInstance(typeof(CsvData)) ??
                     throw new ArgumentNullException($"CreateInstance(typeof(CsvData))");
                AssetDatabase.CreateAsset(gm, filePath);
            }
            else
            {
                throw new ArgumentException("Config Name is existed: " + csvDataName);
            }
        }

#pragma warning disable CS8618
        [ReadOnly] public CsvData[] readerData;
#pragma warning restore CS8618

        public CsvData.CsvInfo? GetInfo(string csvPath)
        {
            return this.readerData.Select(csvData => csvData.GetInfo(csvPath))
                .FirstOrDefault(csvInfo => csvInfo != null);
        }

        public CsvData.ClassInfo? GetClassInfo(string csvPath)
        {
            return this.readerData.Select(csvData => csvData.GetClassInfo(csvPath))
                .FirstOrDefault(csvInfo => csvInfo != null);
        }

        #endregion

        #region Downloader Controller
#if GOOGLE_SHEET_DOWNLOADER
        [Title("Create new csv download group")]
        [InlineButton(nameof(CreateNewCsvDownloadGroup), "Create New")]
        [SuffixLabel("Data Name", true)]
        [HideLabel]
        [Required]
        public string csvDownloadGroupName = string.Empty;

        public void CreateNewCsvDownloadGroup()
        {
            if (csvDownloadGroupName == null || csvDownloadGroupName.Equals(string.Empty))
            {
                throw new ArgumentException("Csv Group Name is null");
            }

            var filePath = $"{CsvConfig.Instance.downloaderConfigPath}/{csvDownloadGroupName}.asset";
            var type = typeof(GoogleSheetGroupConfig);
            var gm = AssetDatabase.LoadAssetAtPath(filePath, type);
            if (gm == null)
            {
                gm = CreateInstance(typeof(GoogleSheetGroupConfig)) ??
                     throw new ArgumentNullException($"CreateInstance(typeof(GoogleSheetGroupConfig))");
                AssetDatabase.CreateAsset(gm, filePath);
            }
            else
            {
                throw new ArgumentException("Config Name is existed: " + csvDownloadGroupName);
            }
        }

#pragma warning disable CS8618
       [ReadOnly] public GoogleSheetGroupConfig[] downloaderData;
#pragma warning restore CS8618
#endif
        #endregion
    }
}
#endif