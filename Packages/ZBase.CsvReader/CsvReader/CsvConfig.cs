#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace CsvReader
{
    [GlobalConfig("Assets/Plugins/CsvReader")]
    [Title("Setting")]
    public class CsvConfig : GlobalConfig<CsvConfig>
    {
        [ReadOnly, ShowInInspector, PropertyOrder(-1)]
        private string RootProject
        {
            get;
            set;
        }

        [Title("Get class type from Assembly")]
        [ValidateInput("ArrayCantEmpty", "Can't be empty")]
        [ValueDropdown("GetFilteredAssemblyList", AppendNextDrawer = true, IsUniqueList = true,
            DropdownTitle = "Select Assembly")]
        [OnValueChanged("SetScriptAssembly")]
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

        [PropertySpace(10)]
        [ReadOnly] [Title("Path to config tool")] [FolderPath(RequireExistingPath = true)]
        public string toolConfigPath = "Assets/Plugins/CsvReader";

        [ReadOnly] [FolderPath(RequireExistingPath = true)]
        public string csvConfigPath = "Assets/Plugins/CsvReader/Data";

        [PropertySpace(20)]
        [FolderPath(RequireExistingPath = true)]
        [ValidateInput("StringCantEmpty", "Can't be empty")]
        [InlineButton("CreateDirectory", "Create")]
        public string csvPath = "Assets/Samples/Csv";

        [FolderPath(RequireExistingPath = true)] [ValidateInput("StringCantEmpty", "Can't be empty")]
        [InlineButton("CreateDirectory", "Create")]
        public string scriptableObjectPath = "Assets/Samples/ScriptableObject";

        private bool StringCantEmpty(string path)
        {
            return path != null && !path.Equals(string.Empty);
        }

        private bool ArrayCantEmpty(string[] stringArray)
        {
            return stringArray is { Length: > 0 };
        }

        [Serializable, Toggle("enabled")]
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

        [PropertySpace(20)] [Title("Filter Namespace")]
        public FilterNameSpace enabledFilter = new();

        protected override void OnConfigInstanceFirstAccessed()
        {
            SetRootPath();
            
            SetScriptAssembly();
            
            CreateDirectory(this.csvConfigPath);
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
    }
}
#endif