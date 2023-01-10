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
            return this.classInfomations.FirstOrDefault(classInfo => classInfo.csvInformations.Any(csvInfo => csvPath.Contains(csvInfo.csvPath)));
        }

        [Serializable]
        public class CsvInfo
        {
            [HideInInspector] public string className = string.Empty;

            [AssetsOnly, Required] [OnValueChanged("OnCsvFileChange")]
            public TextAsset? csvFile;

            [ReadOnly] public string csvPath = string.Empty;

#pragma warning disable CS8618
            [ValueDropdown("GetAllFields", AppendNextDrawer = true, IsUniqueList = true,
                DropdownTitle = "Select Field")]
            [ValidateInput("FieldCantEmpty", "Can't be empty")]
            [GUIColor(1f, 1f, 0.4f)]
            public string fieldSetValue;
#pragma warning restore CS8618

            [HorizontalGroup("Convert")] public bool isConvert;

#pragma warning disable CS8618
            [HorizontalGroup("Convert")]
            [EnableIf("isConvert")]
            [HideLabel]
            [SuffixLabel("method", Overlay = true)]
            [ValidateInput("ConvertCantEmpty", "Can't be empty")]
            [ValueDropdown("GetAllMethods", AppendNextDrawer = true, IsUniqueList = true,
                DropdownTitle = "Select Method")]
            public string convertMethod = string.Empty;
#pragma warning restore CS8618

            public void OnCsvFileChange()
            {
                this.csvPath = AssetDatabase.GetAssetPath(this.csvFile);
            }

            public IEnumerable GetAllFields()
            {
                var fieldNames = CsvUtils.GetAllArrayField(this.className);

                foreach (var t in fieldNames)
                {
                    yield return t;
                }
            }

            public IEnumerable GetAllMethods()
            {
                var methodNames = CsvUtils.GetAllMethod(this.className);
                foreach (var t in methodNames)
                {
                    yield return t;
                }
            }

            private bool ConvertCantEmpty(string? text)
            {
                return !this.isConvert || (text != null && !text.Equals(string.Empty));
            }

            private bool FieldCantEmpty(string? text)
            {
                return text != null && !text.Equals(string.Empty);
            }
        }
    }
}

#endif