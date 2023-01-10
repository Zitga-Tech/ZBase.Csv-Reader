#if UNITY_EDITOR
#nullable enable
using System;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace CsvReader
{
    [GlobalConfig("Assets/Plugins/CsvReader")]
    public class CsvDataController : GlobalConfig<CsvDataController>
    {
#pragma warning disable CS8618
        [ReadOnly] public CsvData[] data;
#pragma warning restore CS8618
        
        public CsvData.CsvInfo? GetInfo(string csvPath)
        {
            return this.data.Select(csvData => csvData.GetInfo(csvPath)).FirstOrDefault(csvInfo => csvInfo != null);
        }

        public CsvData.ClassInfo? GetClassInfo(string csvPath)
        {
            return this.data.Select(csvData => csvData.GetClassInfo(csvPath)).FirstOrDefault(csvInfo => csvInfo != null);
        }

        [Title("Create new csv data")]
#pragma warning disable CS8618
        [InlineButton("CreateNewCsvData", "Create New")]
        [SuffixLabel("Data Name", true)]
        [HideLabel]
        public string csvDataName;
#pragma warning restore CS8618
        
        public void CreateNewCsvData()
        {
            if (csvDataName == null || csvDataName.Equals(string.Empty))
            {
                
                throw new ArgumentException("Config Name is null");
            }
            
            var filePath = $"{CsvConfig.Instance.csvConfigPath}/{csvDataName}.asset";
            var type = typeof(CsvData);
            var gm = AssetDatabase.LoadAssetAtPath(filePath, type);
            if (gm == null)
            {
                gm = CreateInstance(typeof(CsvData)) ?? throw new ArgumentNullException($"CreateInstance(typeof(CsvData))");
                AssetDatabase.CreateAsset(gm, filePath);
            }
            else
            {
                throw new ArgumentException("Config Name is existed: " + csvDataName);
            }
        }
    }

    public class CsvDataPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool isUpdated = false;

            foreach (string assetPath in importedAssets)
            {
                if (Path.GetExtension(assetPath) != ".asset")
                {
                    continue;
                }

                Debug.Log(assetPath);

                isUpdated = true;
                break;
            }

            if (!isUpdated)
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Plugins/CsvReader/Data" });

            CsvDataController.Instance.data = guids.Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<CsvData>).Where(data => data).ToArray();
        }
    }
}
#endif