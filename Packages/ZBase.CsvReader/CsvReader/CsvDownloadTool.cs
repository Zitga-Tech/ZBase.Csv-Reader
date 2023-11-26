using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace CsvDownloader
{

    [GlobalConfig("Assets/_Game/Settings/CsvDownloader")]
    public class CsvDownloadTool : GlobalConfig<CsvDownloadTool>
    {
        [Title("Path to folder csv")]
        [FolderPath(RequireExistingPath = true)]
        public string csvFolderPath;

        [Title("Path to config")]
        [FolderPath(RequireExistingPath = true)]
        public string pathToConfig;

        public TextAsset credentialFile;

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

            var filePath = $"{pathToConfig}/{csvDownloadGroupName}.asset";
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

    public class CsvDownloaderToolsEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/CsvDowloader/Tools")]
        private static void OpenWindow()
        {
            var window = GetWindow<CsvDownloaderToolsEditorWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true) {
                { "Csv Downloader", CsvDownloadTool.Instance, EditorIcons.Bell },
                { "Groups", null , EditorIcons.SettingsCog },
            };

            tree.AddAllAssetsAtPath("Groups", CsvDownloadTool.Instance.pathToConfig, typeof(ScriptableObject), true,
                true).SortMenuItemsByName();

            var customMenuStyle = new OdinMenuStyle
            {
                BorderPadding = 0f,
                AlignTriangleLeft = true,
                TriangleSize = 16f,
                TrianglePadding = 0f,
                Offset = 20f,
                Height = 23,
                IconPadding = 0f,
                BorderAlpha = 0.323f
            };

            tree.DefaultMenuStyle = customMenuStyle;

            tree.Config.DrawSearchToolbar = true;
            tree.Config.DrawScrollView = true;

            return tree;
        }
    }
}