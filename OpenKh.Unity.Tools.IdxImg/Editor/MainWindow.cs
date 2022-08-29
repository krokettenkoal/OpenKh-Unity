using System;
using System.Collections.Generic;
using System.Linq;
using OpenKh.Unity.Aset;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using OpenKh.Unity.Tools.IdxImg.IO;
using AssetImporter = OpenKh.Unity.Tools.IdxImg.IO.AssetImporter;

namespace OpenKh.Unity.Tools.IdxImg
{
    public class MainWindow : IdxManagerWindow
    {
        //  Toolbar
        private ToolbarButton m_OpenFile;
        private ToolbarButton m_ImportAssets;

        /*
        private NativeArray<int> m_ExtractQueue;
        private NativeArray<bool> m_ExtractResults;
        private NativeArray<int> m_ExportQueue;
        private NativeArray<bool> m_ExportResults;
        */

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

            var extractStatus = new ExtractStatus
            {
                current = -1,
                total = @checked.Count,
            };

            try
            {
                //  Extract checked assets
                foreach (var fvm in @checked)
                {
                    //Debug.Log($"Extracting {fvm.Name} ..");

                    extractStatus.fileName = fvm.FullName;
                    extractStatus.current++;

                    if (Utils.DisplayCancellableExtractProgress(ExtractState.Processing, extractStatus))
                        throw new OperationCanceledException();

                    AssetImporter.ExtractAsset(fvm);
                }

                if (Utils.DisplayCancellableExtractProgress(ExtractState.Finished, extractStatus))
                    throw new OperationCanceledException();

                //  Export all supported extracted assets
                foreach (var asset in AssetImporter.ExportableAssets)
                {
                    //Debug.Log($"Exporting {asset} ..");
                    MdlxConvert.ToAset(asset, Utils.DisplayExportProgress, out _);
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Debug.Log("Asset import cancelled by user.");
                }
                else
                {
                    Debug.LogWarning("Asset import failed!");
                    Debug.LogError(ex);
                }
            }
            finally
            {
                //tokenSource.Dispose();
                ExtractQueue.Active.Clear();
                ExportQueue.Active.Clear();
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log("Import done.");
        }
    }
}

