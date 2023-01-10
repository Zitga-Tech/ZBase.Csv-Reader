#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CsvReader
{
    public class CsvPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (Path.GetExtension(assetPath) != ".csv")
                {
                    continue;
                }

                if (CsvConfig.Instance.csvPath.Equals(string.Empty))
                {
                    throw new ArgumentException("Csv path can't be null");
                }

                if (assetPath.IndexOf(CsvConfig.Instance.csvPath, StringComparison.Ordinal) == -1)
                {
                    continue;
                }

                var classInformation = CsvDataController.Instance.GetClassInfo(assetPath) ??
                                       throw new ArgumentNullException(
                                           $"Can't find csv config: Asset Path[{assetPath}]");

                foreach (var csvInfo in classInformation.csvInformations)
                {
                    var collectionType = CsvUtils.GetType(csvInfo.className) ??
                                         throw new ArgumentNullException(
                                             $"Class name is null: Class Name[{csvInfo.className}], Asset Path[{assetPath}]");

                    string nameAsset = $"{csvInfo.className}.asset";

                    if (CsvConfig.Instance.scriptableObjectPath.Equals(string.Empty))
                    {
                        throw new ArgumentException("ScriptableObject path can't be null");
                    }

                    string assetFile = $"{CsvConfig.Instance.scriptableObjectPath}/{nameAsset}";
                    var gm = AssetDatabase.LoadAssetAtPath(assetFile, collectionType);
                    if (gm == null)
                    {
                        gm = ScriptableObject.CreateInstance(collectionType);
                        AssetDatabase.CreateAsset(gm, assetFile);
                    }

                    if (csvInfo.csvFile != null)
                    {
                        var field = gm.GetType().GetField(csvInfo.fieldSetValue,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        var dataType = CsvUtils.GetElementTypeFromFieldInfo(field);

                        var result = Reader.Deserialize(dataType, csvInfo.csvFile.text);

                        if (field != null)
                        {
                            field.SetValue(gm, result);

                            if (csvInfo.isConvert)
                            {
                                var method = gm.GetType().GetMethod(csvInfo.convertMethod);
                                method?.Invoke(gm, null);
                                if (field.IsPrivate) field.SetValue(gm, null);
                            }
                        }

                        EditorUtility.SetDirty(gm);
                        AssetDatabase.SaveAssets();
                    }
                }

                Debug.Log("Reimport Asset: " + assetPath);
            }
        }
    }
}
#endif