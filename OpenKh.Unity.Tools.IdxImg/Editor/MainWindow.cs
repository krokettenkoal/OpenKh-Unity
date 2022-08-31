using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using AssetImporter = OpenKh.Unity.Tools.IdxImg.IO.AssetImporter;

namespace OpenKh.Unity.Tools.IdxImg
{
    public class MainWindow : IdxManagerWindow
    {

        #region Menu entry

        [MenuItem("OpenKh/Asset Importer", false, 0)]
        public static void ShowAssetImporter()
        {
            var wnd = GetWindow<MainWindow>();
            wnd.titleContent = new GUIContent("OpenKh Asset Importer");
        }

        #endregion

        #region Initialization

        public void CreateGUI()
        {
            if (m_TreeViewAsset == null || m_EntryViewAsset == null)
                return;

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            Init(root);
            BindViews();
            AddListeners();
        }

        protected override void Init(VisualElement root)
        {
            base.Init(root);
            HideEntryView();
        }
        private void BindViews()
        {
            m_Tree.columns["name"].makeCell = () => new Label();
            m_Tree.columns["import"].makeCell = () => new Toggle();

            m_Tree.columns["name"].bindCell = (element, index) =>
            {
                if (element is Label l)
                    l.text = m_Tree.GetItemDataForIndex<EntryViewModel>(index).Name;
            };

            m_Tree.columns["import"].bindCell = (element, index) =>
            {
                if (element is Toggle t)
                {
                    var entry = m_Tree.GetItemDataForIndex<EntryViewModel>(index);
                    t.name = entry.Name;
                    t.userData = entry;
                    t.SetValueWithoutNotify(entry.IsChecked);
                    t.showMixedValue = entry is NodeViewModel {IsChecked: false} node && node.Children.Any(a => a.IsChecked);
                    t.RegisterValueChangedCallback(ToggleImport);
                }
            };

            m_Tree.columns["import"].unbindCell = (element, index) =>
            {
                if (element is Toggle t)
                {
                    t.UnregisterValueChangedCallback(ToggleImport);
                }
            };
        }
        protected override void AddListeners()
        {
            base.AddListeners();

            m_Tree.onSelectionChange += UpdateEntryView;
            m_OpenFile.clicked += OpenIdxImgFiles;
            m_ImportAssets.clicked += ImportAssets;
        }

        #endregion

        #region Event handlers

        private void ToggleImport(ChangeEvent<bool> ev)
        {
            if (ev.target is not Toggle {userData: EntryViewModel evm})
                return;

            evm.IsChecked = ev.newValue;

            //Debug.Log($"{entry.Name}<{entry.GetType()}>: {entry.IsChecked}");
            
            m_Tree.RefreshItems();
        }
        protected void ImportAssets()
        {
            if (Root == null || Root.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Import Assets",
                    "Open a valid IDX/IMG file before importing assets.",
                    "OK"
                );

                return;
            }

            var @checked = Root[0]
                .Where(evm => evm is FileViewModel { IsChecked: true })
                .Select(evm => evm as FileViewModel)
                .ToList();

            if (@checked.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Import Assets",
                    "No assets marked for import. Tick at least one checkbox to import assets.",
                    "OK"
                );

                return;
            }

            if (HasMdlxMsetPair(@checked) && !EditorUtility.DisplayDialog("Asset import",
                    "Importing models with animations can take a long time. Do you want to continue?", "Continue",
                    "Cancel"))
                return;

            //  Check for MDLX/MSET file pairs
            //  Warn user for long import time
            AssetImporter.ImportAssets(@checked);
        }

        #endregion

    }
}

