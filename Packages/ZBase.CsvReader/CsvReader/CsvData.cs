#if UNITY_EDITOR
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace CsvReader
{
    public class CsvData : ScriptableObject
    {
#pragma warning disable CS8618
        [Title("Csv Data")] public ClassInfo[] classInfomations;
#pragma warning restore CS8618

        [Serializable]
        public class ClassInfo
        {
            [ValueDropdown("GetAllClasses", AppendNextDrawer = true, IsUniqueList = true,
                DropdownTitle = "Select Field")]
            [OnValueChanged("OnClassNameChange")]
            [ValidateInput("ClassNameCantEmpty", "Can't be empty")]
            [GUIColor(0.3f, 0.8f, 0.8f)]
            public string className = string.Empty;

#pragma warning disable CS8618
            [OnCollectionChanged("AfterChanged")] public CsvInfo[] csvInformations;
#pragma warning restore CS8618

            public void OnClassNameChange()
            {
                foreach (var csvInformation in this.csvInformations)
                {
                    csvInformation.className = this.className;
                }
            }

            private bool ClassNameCantEmpty(string? text)
            {
                return text != null && !text.Equals(string.Empty);
            }

            public IEnumerable GetAllClasses()
            {
                if (CsvConfig.Instance.ScriptAssemblies != null)
                {
                    foreach (Assembly assembly in CsvConfig.Instance.ScriptAssemblies)
                    {
                        IEnumerable<string> nameList = assembly.GetTypes().Select(x => x.FullName);
                        if (CsvConfig.Instance.enabledFilter.enabled &&
                            CsvConfig.Instance.enabledFilter.namespaceList.Length > 0)
                        {
                            nameList = nameList.Where(name => {
                                foreach (var nameSpace in CsvConfig.Instance.enabledFilter.namespaceList)
                                {
                                    if (name != null && name.IndexOf(nameSpace, StringComparison.Ordinal) == 0)
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            });
                        }

                        foreach (var name in nameList)
                        {
                            yield return name;
                        }
                    }
                }
            }

            public void AfterChanged(CollectionChangeInfo info, object value)
            {
                if (info.ChangeType != CollectionChangeType.Add)
                {
                    return;
                }

                var items = ((CsvInfo[])value);
                foreach (CsvInfo csvInfo in items)
                {
                    csvInfo.className = this.className;
                }
            }
        }

        public CsvInfo? GetInfo(string csvPath)
        {
            return this.classInfomations.SelectMany(classInfo => classInfo.csvInformations)
                .FirstOrDefault(csvInfo => csvPath.Contains(csvInfo.csvPath));
        }

        public ClassInfo? GetClassInfo(string csvPath)
        {
            return this.classInfomations.FirstOrDefault(classInfo => classInfo.csvInformations.Any(csvInfo =>
                (csvInfo.csvType == CsvInfo.CsvType.File && csvPath.Contains(csvInfo.csvPath)) ||
                (csvInfo.csvType == CsvInfo.CsvType.Folder && csvPath.Contains(csvInfo.csvFolderPath))));
        }

        [Serializable]
        public class CsvInfo
        {
            public enum CsvType
            {
                File,
                Folder
            }

            public CsvType csvType = CsvType.File;

            [HideInInspector] public string className = string.Empty;

            [AssetsOnly, Required] [OnValueChanged("OnCsvFileChange")] [ShowIf(nameof(csvType), CsvType.File)]
            public TextAsset? csvFile;

            [ShowIf(nameof(csvType), CsvType.File)] [ReadOnly]
            public string csvPath = string.Empty;

            [ShowIf(nameof(csvType), CsvType.Folder)] [FolderPath(RequireExistingPath = true)]
            public string csvFolderPath = string.Empty;

            [ShowIf(nameof(csvType), CsvType.Folder)]
            public bool separateScriptableObject = false;

            [ShowIf("@this.separateScriptableObject && this.csvType == CsvType.Folder")]
            [Tooltip(
                "Let empty if not separate ScriptableObject, it's used to create distinct scriptableObject's names depends on remain part without 'start with' part")]
            public string fileStartWith = "";

#pragma warning disable CS8618
            [ValueDropdown("GetAllFields", AppendNextDrawer = true, IsUniqueList = true,
                DropdownTitle = "Select Field")]
            [ValidateInput("FieldCantEmpty", "Can't be empty")]
            [GUIColor(1f, 1f, 0.4f)]
            public string fieldSetValue;
#pragma warning restore CS8618

#pragma warning disable CS8618
            [SuffixLabel("default is empty", Overlay = true)]
            [ValueDropdown("GetAllMethods", AppendNextDrawer = true, IsUniqueList = true,
                DropdownTitle = "Select Method")]
            public string convertMethod = string.Empty;
#pragma warning restore CS8618

            public void OnCsvFileChange()
            {
                var newPath = AssetDatabase.GetAssetPath(this.csvFile);
                if(string.IsNullOrEmpty(this.csvPath) == false && newPath.Equals(this.csvPath) == false)
                {
                    Debug.Log($"File is changed: <color=green>{newPath}</color>");
                }
                this.csvPath = newPath;
            }

            public IEnumerable GetAllFields()
            {
                var fieldNames = CsvReaderUtils.GetAllArrayField(this.className);

                foreach (var t in fieldNames)
                {
                    yield return t;
                }
            }

            public bool IsUsingConvertMethod()
            {
                return this.convertMethod.Equals(string.Empty) == false;
            }

            public IEnumerable GetAllMethods()
            {
                var methodNames = CsvReaderUtils.GetAllMethod(this.className);
                foreach (var t in methodNames)
                {
                    yield return t;
                }
            }

            private bool FieldCantEmpty(string? text)
            {
                return text != null && !text.Equals(string.Empty);
            }
        }
    }
}

#endif