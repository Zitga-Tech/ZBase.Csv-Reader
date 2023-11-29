#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CsvReader
{
    public class CsvMenuWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/Csv-Reader")]
        private static void OpenWindow()
        {
            var window = GetWindow<CsvMenuWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true) {
                { "Home", CsvConfig.Instance, EditorIcons.House },
                { "Controller", CsvDataController.Instance, EditorIcons.Info },
                { "Reader Config", null, EditorIcons.SettingsCog }, 
#if GOOGLE_SHEET_DOWNLOADER
                { "Downloader Config", null, EditorIcons.SettingsCog },
#endif
            };

            tree.AddAllAssetsAtPath("Reader Config", CsvConfig.Instance.readerConfigPath, typeof(ScriptableObject), true,
                true).SortMenuItemsByName();
#if GOOGLE_SHEET_DOWNLOADER
            tree.AddAllAssetsAtPath("Downloader Config", CsvConfig.Instance.downloaderConfigPath, typeof(ScriptableObject), true,
                true).SortMenuItemsByName();
#endif
            
            var customMenuStyle = new OdinMenuStyle {
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
#endif