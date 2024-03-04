#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace CsvReader
{
    [GlobalConfig("Assets/Plugins/CsvReader")]
    [Title("Setting")]
    public class CsvConfig : GlobalConfig<CsvConfig>
    {
        [ReadOnly, ShowInInspector, PropertyOrder(1)]
        private string RootProject
        {
            get;
            set;
        }

        [Title("Get class type from Assembly")]
        [ValidateInput("ArrayCantEmpty", "Can't be empty")]
        [ValueDropdown("GetFilteredAssemblyList", AppendNextDrawer = true, IsUniqueList = true,
            DropdownTitle = "Select Assembly")]
        [OnValueChanged("SetScriptAssembly"), PropertyOrder(2)]
        public string[] assemblyNames;
        
        public IEnumerable GetFilteredAssemblyList()
        {
            var assemblies = GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                    yield return assembly.Location.Substring(RootProject.Length);
            }
        }

        public Assembly[] ScriptAssemblies
        {
            get;
            private set;
        }

        [PropertySpace(10), PropertyOrder(3)]
        [ReadOnly] [Title("Path to config tool")] [FolderPath(RequireExistingPath = true)]
        public string toolConfigPath = "Assets/Plugins/CsvReader";

        [PropertyOrder(4)]
        [ReadOnly] [FolderPath(RequireExistingPath = true)]
        public string readerConfigPath = "Assets/Plugins/CsvReader/ReaderConfig";

        [PropertySpace(20), PropertyOrder(5)]
        [FolderPath(RequireExistingPath = true)]
        [ValidateInput("StringCantEmpty", "Can't be empty")]
        [InlineButton("CreateDirectory", "Create")]
        public string csvPath = "Assets/Samples/Csv";

        [FolderPath(RequireExistingPath = true)] [ValidateInput("StringCantEmpty", "Can't be empty")]
        [InlineButton("CreateDirectory", "Create"), PropertyOrder(6)]
        public string scriptableObjectPath = "Assets/Samples/ScriptableObject";

        [PropertySpace(10)]
        [Button("Refresh All Csv Config"), PropertyOrder(7)]
        private void RefreshAllCsvConfig()
        {
            CsvDataController.Instance.SetReaderData();
            foreach (CsvData data in CsvDataController.Instance.readerData)
            {
                if (data == null)
                {
                    continue;
                }
                
                foreach (CsvData.ClassInfo classInfo in data.classInfomations)
                {
                    foreach (CsvData.CsvInfo csvInfo in classInfo.csvInformations)
                    {
                        csvInfo.OnCsvFileChange();
                    }
                }
            }
            
            Debug.Log("RefreshAllCsvConfig is Complete");
        }
        
        private bool StringCantEmpty(string path)
        {
            return path != null && !path.Equals(string.Empty);
        }

        private bool ArrayCantEmpty(string[] stringArray)
        {
            return stringArray is { Length: > 0 };
        }

        [Serializable, Toggle("enabled"), PropertyOrder(8)]
        public class FilterNameSpace
        {
            public bool enabled;

            [ValueDropdown("GetFilteredNameSpaceList", AppendNextDrawer = true, IsUniqueList = true,
                DropdownTitle = "Select NameSpace")]
            public string[] namespaceList;

            public IEnumerable GetFilteredNameSpaceList()
            {
                if (Instance.ScriptAssemblies != null)
                {
                    foreach (Assembly assembly in Instance.ScriptAssemblies)
                    {
                        var namespaces = assembly.GetTypes().Select(x => x.Namespace).Distinct();
                        foreach (var n in namespaces)
                        {
                            yield return n;
                        }
                    }
                }
            }
        }
        
        [PropertySpace(20), PropertyOrder(90)]
        [Title("Filter Namespace")]
        public FilterNameSpace enabledFilter = new();
        
        protected override void OnConfigInstanceFirstAccessed()
        {
            SetRootPath();
            
            SetScriptAssembly();
            
            CreateDirectory(this.readerConfigPath);
            
#if GOOGLE_SHEET_DOWNLOADER
            CreateDirectory(this.downloaderConfigPath);
#endif
            RefreshAllCsvConfig();
        }

        private void SetScriptAssembly()
        {
            if (this.assemblyNames is { Length: > 0 })
            {
                var assemblies = GetAssemblies();
                List<Assembly> validAssemblyList = new List<Assembly>();
                foreach (var assemblyName in this.assemblyNames)
                {
                    var assembly = assemblies.ToList().Find(x => x.Location.IndexOf(assemblyName, StringComparison.Ordinal) != -1);
                    if (assembly == null)
                    {
                        Debug.LogWarningFormat("Not found assembly: {0}", assemblyName);
                    }
                    else
                    {
                        validAssemblyList.Add(assembly);
                    }
                }

                this.ScriptAssemblies = validAssemblyList.ToArray();
            }
            else
            {
                this.ScriptAssemblies = null;
            }
        }

        private Assembly[] GetAssemblies()
        {
            AppDomain current = AppDomain.CurrentDomain;

            var assemblies = current.GetAssemblies();

            var root = Directory.GetCurrentDirectory();

            return assemblies
                .Where(x => !x.IsDynamic && x.Location.IndexOf(root, StringComparison.Ordinal) != -1).ToArray();
        }

        private void SetRootPath()
        {
            this.RootProject = Directory.GetCurrentDirectory().Replace('\\', '/');
        }

        private void CreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(new DirectoryInfo(path).FullName);
            AssetDatabase.Refresh();
        }
        
#if GOOGLE_SHEET_DOWNLOADER
        [Required,Title("Csv Downloader"), PropertyOrder(100)]
        [ReadOnly] [FolderPath(RequireExistingPath = true)]
        public string downloaderConfigPath = "Assets/Plugins/CsvReader/DownloaderConfig";
        
        [Required,Title("Credential File"), PropertyOrder(110)]
        public TextAsset credentialFile;

        [InfoBox("Wait until activity complete before do anything else!")]
        [PropertyOrder(110)]
        [Button("Download All Google Sheet Files")]
        public void LoadAll()
        {
            LoadAsync().Forget();
        }

        private async UniTaskVoid LoadAsync()
        {
            var count = 0;
            foreach (GoogleSheetGroupConfig data in CsvDataController.Instance.downloaderData)
            {
                await data.LoadAll();
                count++;
                Debug.Log($"Finished download: {count}/{CsvDataController.Instance.downloaderData.Length}");
            }
            
            
            Debug.Log("Finished download all sheets");
        }
#endif
    }
}
#endif