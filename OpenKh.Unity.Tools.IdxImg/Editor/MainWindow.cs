using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using FileViewModel = OpenKh.Unity.Tools.IdxImg.ViewModels.FileViewModel;
using OpenKh.Unity.Tools.IdxImg.IO;
using Unity.Collections;
using Unity.Jobs;

namespace OpenKh.Unity.Tools.IdxImg
{
    public class MainWindow : IdxManagerWindow
    {
        //  Toolbar
        private ToolbarButton m_OpenFile;
        private ToolbarButton m_ImportAssets;

        private NativeArray<int> m_ExtractQueue;
        private NativeArray<bool> m_ExtractResults;
        private NativeArray<int> m_ExportQueue;
        private NativeArray<bool> m_ExportResults;

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

            try
            {
                var extractHandle = ExtractAssets(@checked);
                var exportHandle = ExportAssets(extractHandle);

                exportHandle.Complete();

                m_ExtractQueue.Dispose();
                m_ExtractResults.Dispose();
                m_ExportQueue.Dispose();
                m_ExportResults.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Asset import failed!");
                Debug.LogError(ex);
            }
            finally
            {
                ExtractQueue.Active.Clear();
                ExportQueue.Active.Clear();
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log("Import done.");
        }

        protected JobHandle ExtractAssets(List<FileViewModel> assets)
        {
            //Debug.Log($"Importing {@checked.Count} assets..");

            ExtractQueue.Active.AddRange(assets);

            m_ExtractQueue = new NativeArray<int>(ExtractQueue.Active.Select((e, i) => i).ToArray(), Allocator.Persistent);
            m_ExtractResults = new NativeArray<bool>(ExtractQueue.Active.Count, Allocator.Persistent);
            var job = new ExtractJobParallel()
            {
                QueueIds = m_ExtractQueue,
                Result = m_ExtractResults,
            };

            return job.Schedule(m_ExtractQueue.Length, 1);
        }

        /// <summary>
        /// Convert asset files into file formats supported in Unity
        /// </summary>
        protected JobHandle ExportAssets(JobHandle extractJobHandle)
        {
            Debug.Log("Exporting asset files..");

            if(ExportQueue.Active.Count > 0)
                ExportQueue.Active.Clear();

            if (Directory.Exists(PackageInfo.TempDir))
            {
                ExportQueue.Active.AddRange(
                    Directory.GetFiles(PackageInfo.TempDir, "*.mdlx", SearchOption.AllDirectories)
                        .Where(IsConvertible)
                    );
            }

            m_ExportQueue = new NativeArray<int>(ExportQueue.Active.Select((e, i) => i).ToArray(), Allocator.Persistent);
            m_ExportResults = new NativeArray<bool>(ExportQueue.Active.Count, Allocator.Persistent);
            var job = new ExportJobParallel()
            {
                QueueIds = m_ExportQueue,
                Format = ExportFormat.Aset,
                Result = m_ExportResults,
            };

            return job.Schedule(m_ExportQueue.Length, 1, extractJobHandle);
        }

        protected virtual bool IsConvertible(string filePath) => Path.GetExtension(filePath).ToLowerInvariant() == ".mdlx";
    }
}

