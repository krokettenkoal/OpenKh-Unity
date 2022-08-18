using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using FileViewModel = OpenKh.Unity.Tools.IdxImg.ViewModels.FileViewModel;

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

            var @checked = Root[0]
                .Where(evm => evm is FileViewModel {IsChecked: true})
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

            //Debug.Log($"Importing {@checked.Count} assets..");

            int i = 0, successful = 0;

            for (; i < @checked.Count; i++)
            {
                var fvm = @checked[i];
                var cancel = EditorUtility.DisplayCancelableProgressBar($"Importing assets ({i+1} / {@checked.Count})..", fvm.Entry.GetFullName(), (float)i / @checked.Count);

                if (cancel)
                    break;

                if (ImportAsset(fvm))
                    successful++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log($"Import {(i >= @checked.Count ? "done" : "cancelled")}. Successful: {successful} / Skipped: {i - successful}");
        }
        protected bool ImportAsset(FileViewModel fvm)
        {
            //Debug.Log($"Importing asset '{fvm.Name}'..");

            if (!_img.TryFileOpen(fvm.Entry.GetFullName(), out var stream))
            {
                //Debug.LogWarning($"Could not import '{fvm.Entry.GetFullName()}'");
                return false;
            }

            var filePath = Path.Combine(PackageInfo.TempDir, $"{Path.GetFileNameWithoutExtension(_idxFilePath)}/{fvm.Entry.GetFullName()}");
            var destDir = Path.GetDirectoryName(filePath);

            if (destDir == null) 
                return false;
                
            Directory.CreateDirectory(destDir);

            if(File.Exists(filePath))
                File.Delete(filePath);

            using var fileStream = File.Create(filePath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);

            return true;
        }
    }
}

