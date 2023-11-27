using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using CsvReader;
using UnityEditor;

namespace CsvDownloader
{

    [GlobalConfig("Assets/Plugins/CsvReader")]
    public class CsvDownloaderController : GlobalConfig<CsvDownloaderController>
    {
#pragma warning disable CS8618
        [ReadOnly] public CsvDownloaderGroupItemConfig[] data;
#pragma warning restore CS8618
        
        [Title("Create new csv download group")]
        [InlineButton(nameof(CreateNewCsvDownloadGroup), "Create New")]
        [SuffixLabel("Data Name", true)]
        [HideLabel]
        public string csvDownloadGroupName;

        public void CreateNewCsvDownloadGroup()
        {
            if (csvDownloadGroupName == null || csvDownloadGroupName.Equals(string.Empty))
            {

                throw new ArgumentException("Csv Group Name is null");
            }

            var filePath = $"{CsvConfig.Instance.downloaderConfigPath}/{csvDownloadGroupName}.asset";
            var type = typeof(CsvDownloaderGroupItemConfig);
            var gm = AssetDatabase.LoadAssetAtPath(filePath, type);
            if (gm == null)
            {
                gm = CreateInstance(typeof(CsvDownloaderGroupItemConfig)) ?? throw new ArgumentNullException($"CreateInstance(typeof(CsvDownloaderGroupItemConfig))");
                AssetDatabase.CreateAsset(gm, filePath);
            }
            else
            {
                throw new ArgumentException("Config Name is existed: " + csvDownloadGroupName);
            }
        }
    }
}