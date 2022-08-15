using System.Collections.Generic;
using System.Linq;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenKh.Unity.Tools.IdxImg
{
    public class MainWindow : IdxManagerWindow
    {
        //  Toolbar
        private ToolbarButton m_OpenFile;
        private ToolbarButton m_ImportAssets;
        
        [MenuItem("OpenKh/Asset Importer")]
        public static void ShowAssetImporter()
        {
            var wnd = GetWindow<MainWindow>();
            wnd.titleContent = new GUIContent("OpenKh Asset Importer");
        }

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

            m_OpenFile = m_RootElement.Q<ToolbarButton>("OpenFiles");
            m_ImportAssets = m_RootElement.Q<ToolbarButton>("ImportAssets");
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
        private void AddListeners()
        {
            m_Tree.onSelectionChange += UpdateEntryView;
            m_OpenFile.clicked += OpenIdxImgFiles;
            m_ImportAssets.clicked += ImportAssets;
        }

        private void ToggleImport(ChangeEvent<bool> ev)
        {
            if (ev.target is not Toggle {userData: EntryViewModel evm})
                return;

            evm.IsChecked = ev.newValue;

            //Debug.Log($"{entry.Name}<{entry.GetType()}>: {entry.IsChecked}");
            
            m_Tree.RefreshItems();
        }

        private void UpdateEntryView(IEnumerable<object> selection)
        {
            var active = selection.OfType<FileViewModel>().FirstOrDefault();

            if (active is null)
            {
                HideEntryView();
                return;
            }

            SetEntryView(active);
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

            var @checked = Root[0].Where(evm => evm.IsChecked).ToList();

            if (@checked.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Import Assets",
                    "No assets marked for import. Tick at least one checkbox to import assets.",
                    "OK"
                );

                return;
            }

            Debug.Log($"Importing {@checked.Count} assets..");

            foreach (var node in @checked)
            {
                ImportAsset(node);
            }

            Debug.Log("Import complete.");
        }
        protected void ImportAsset(EntryViewModel evm)
        {
            if (evm is not FileViewModel { IsChecked: true } fvm)
            {
                //Debug.Log($"Skipped '{evm.Name}': item is not a file or not marked for import.");
                return;
            }

            Debug.Log($"Importing asset '{fvm.Name}'..");

            if (_img.TryFileOpen(fvm.Entry.GetFullName(), out var stream))
            {
                Debug.Log("Open file successful. Stream can be saved.");
            }
            else
            {
                Debug.LogWarning("Open file failed. No stream received.");
            }
        }
    }
}

