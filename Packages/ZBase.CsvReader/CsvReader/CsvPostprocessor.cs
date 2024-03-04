#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CsvReader
{
    public class CsvPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool isReaderConfigUpdated = false;
#if GOOGLE_SHEET_DOWNLOADER
            bool isDownloaderConfigUpdated = false;
#endif

            foreach (string assetPath in importedAssets)
            {
                if (Path.GetExtension(assetPath).Equals(".csv"))
                {
                    ProcessCsv(assetPath);
                }
                else
                {
                    if (Path.GetExtension(assetPath) != ".asset")
                    {
                        continue;
                    }

                    if (isReaderConfigUpdated == false &&
                        assetPath.IndexOf(CsvConfig.Instance.readerConfigPath, StringComparison.Ordinal) != -1)
                    {
                        isReaderConfigUpdated = true;
                        continue;
                    }

#if GOOGLE_SHEET_DOWNLOADER
                    if (isDownloaderConfigUpdated == false && assetPath.IndexOf(CsvConfig.Instance.downloaderConfigPath,
                            StringComparison.Ordinal) != -1)
                    {
                        isDownloaderConfigUpdated = true;
                        continue;
                    }
#endif

                    if (isReaderConfigUpdated
#if GOOGLE_SHEET_DOWNLOADER
                        && isDownloaderConfigUpdated
#endif
                       )
                    {
                        break;
                    }
                }
            }

            if (isReaderConfigUpdated)
            {
                CsvDataController.Instance.SetReaderData();
            }

#if GOOGLE_SHEET_DOWNLOADER
            if (isDownloaderConfigUpdated)
            {
                var guids = AssetDatabase.FindAssets("t:ScriptableObject",
                    new[] { CsvConfig.Instance.downloaderConfigPath });

                CsvDataController.Instance.downloaderData = guids.Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GoogleSheetGroupConfig>).Where(data => data).ToArray();
            }
#endif
        }

        static void ProcessCsv(string assetPath)
        {
            if (CsvConfig.Instance.csvPath.Equals(string.Empty))
            {
                throw new ArgumentException("Csv path can't be null");
            }

            if (assetPath.IndexOf(CsvConfig.Instance.csvPath, StringComparison.Ordinal) == -1)
            {
                return;
            }

            var classInformation = CsvDataController.Instance.GetClassInfo(assetPath) ??
                                   throw new ArgumentNullException(
                                       $"Can't find csv config: Asset Path[{assetPath}]");

            foreach (var csvInfo in classInformation.csvInformations)
            {
                var collectionType = CsvReaderUtils.GetType(csvInfo.className) ??
                                     throw new ArgumentNullException(
                                         $"Class name is null: Class Name[{csvInfo.className}], Asset Path[{assetPath}]");

                if (CsvConfig.Instance.scriptableObjectPath.Equals(string.Empty))
                {
                    throw new ArgumentException("ScriptableObject path can't be null");
                }

                if (csvInfo.csvType == CsvData.CsvInfo.CsvType.File)
                {
                    string nameAsset = $"{csvInfo.className}.asset";

                    string assetFile = $"{CsvConfig.Instance.scriptableObjectPath}/{nameAsset}";

                    var gm = GetScriptableObject(assetFile, collectionType);

                    if (csvInfo.csvFile != null)
                    {
                        SetFileToScriptableObject(gm, csvInfo, csvInfo.csvFile.text);
                    }
                }
                else
                {
                    // Get all files in the same folders.
                    var directoryInfo = new DirectoryInfo(csvInfo.csvFolderPath);
                    FileInfo[] files = directoryInfo.GetFiles("*.csv");

                    if (csvInfo.separateScriptableObject)
                    {
                        foreach (var file in files)
                        {
                            var distinctPart = file.Name.Replace(csvInfo.fileStartWith, "");
                            if (!assetPath.EndsWith(distinctPart))
                            {
                                continue;
                            }

                            var relativeFilePath = Path.Combine(csvInfo.csvFolderPath, file.Name);

                            var convertedFilePath = relativeFilePath.Replace("\\", "/");
                            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(convertedFilePath);

                            if (textAsset)
                            {
                                distinctPart = distinctPart.Replace(".csv", "");
                                string nameAsset = $"{csvInfo.className}{distinctPart}.asset";
                                string assetFile = $"{CsvConfig.Instance.scriptableObjectPath}/{nameAsset}";

                                var gm = GetScriptableObject(assetFile, collectionType);

                                SetFileToScriptableObject(gm, csvInfo, textAsset.text);
                            }
                        }
                    }
                    else
                    {
                        // Get ScriptableObject Info
                        string nameAsset = $"{csvInfo.className}.asset";
                        string assetFile = $"{CsvConfig.Instance.scriptableObjectPath}/{nameAsset}";

                        var gm = GetScriptableObject(assetFile, collectionType);

                        var field = gm.GetType().GetField(csvInfo.fieldSetValue,
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var dataType = CsvReaderUtils.GetElementTypeFromFieldInfo(field);
                        // ===================

                        Type genericListType = typeof(List<>).MakeGenericType(dataType);
                        var resultList = (IList)Activator.CreateInstance(genericListType);
                        if (field != null)
                        {
                            foreach (var file in files)
                            {
                                var relativeFilePath = Path.Combine(csvInfo.csvFolderPath, file.Name);
                                var convertedFilePath = relativeFilePath.Replace("\\", "/");
                                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(convertedFilePath);

                                if (textAsset)
                                {
                                    var result = Reader.Deserialize(dataType, textAsset.text, csvInfo.fieldSetValue) as Array;
                                    foreach (var item in result)
                                        resultList.Add(item);
                                }
                            }

                            Array resultArray = Array.CreateInstance(dataType, resultList.Count);
                            resultList.CopyTo(resultArray, 0);
                            field.SetValue(gm, resultArray);

                            if (csvInfo.IsUsingConvertMethod())
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
            }

            Debug.Log("Reimport Asset: " + assetPath);
        }

        static Object GetScriptableObject(string assetFile, Type collectionType)
        {
            var gm = AssetDatabase.LoadAssetAtPath(assetFile, collectionType);
            if (gm == null)
            {
                gm = ScriptableObject.CreateInstance(collectionType);
                AssetDatabase.CreateAsset(gm, assetFile);
            }

            return gm;
        }

        static void SetFileToScriptableObject(Object gm, CsvData.CsvInfo csvInfo, string text)
        {
            var field = gm.GetType().GetField(csvInfo.fieldSetValue,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var dataType = CsvReaderUtils.GetElementTypeFromFieldInfo(field);

            var isArray = field.FieldType.IsArray;

            var result = Reader.Deserialize(dataType, text, csvInfo.fieldSetValue, isArray);

            if (field != null)
            {
                field.SetValue(gm, result);

                if (csvInfo.IsUsingConvertMethod())
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
}
#endif