using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Kh2;
using OpenKh.Unity.Exporter.Progress;
using OpenKh.Unity.Tools.IdxImg.Interfaces;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using AssetImporter = OpenKh.Unity.Tools.IdxImg.IO.AssetImporter;
using UnityEditor.UIElements;

namespace OpenKh.Unity.Tools.IdxImg
{
    // Abstract base class for all windows that display IDX/IMG file contents.
    public abstract class IdxManagerWindow : EditorWindow, IIdxManager
    {
        #region Fields

        //  Visual tree assets
        [SerializeField]
        protected VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField]
        protected VisualTreeAsset m_TreeViewAsset = default;
        [SerializeField]
        protected VisualTreeAsset m_EntryViewAsset = default;
        [SerializeField]
        protected VisualTreeAsset m_TreeSearchAsset = default;

        //  Views
        protected VisualElement m_RootElement;
        protected TwoPaneSplitView m_MainPanel;
        protected VisualElement m_TreeView;
        protected VisualElement m_EntryView;
        protected MultiColumnTreeView m_Tree;
        protected VisualElement m_Entry;
        protected VisualElement m_TreeSearch;

        //  Toolbar
        protected ToolbarButton m_OpenFile;
        protected ToolbarButton m_ImportAssets;

        //  Miscellaneous Labels/VisualElements
        protected Label m_Title;

        //  Entry property controls
        protected VisualElement m_IsImported;
        protected TextField m_FullName;
        protected LongField m_PhysicalOffset;
        protected LongField m_PhysicalSize;
        protected LongField m_UncompressedSize;
        protected Toggle m_Compress;
        protected Toggle m_Stream;
        protected TextField m_Search;

        //  BAR entries
        protected MultiColumnListView m_BarEntries;
        protected Label m_BarTitle;
        protected Toggle m_FilterDummies;
        private string m_BarTitleOriginal;

        //  File streams
        protected Stream _imgStream;

        #endregion

        #region Properties

        internal List<RootViewModel> Root { get; private set; }
        public FileViewModel ActiveModel { get; protected set; }
        public Bar ActiveBar { get; protected set; }
        public bool FilterDummies
        {
            get => m_FilterDummies.value;
            set
            {
                m_FilterDummies.value = value;
                UpdateDetailView(value);
            }
        }

        #endregion

        #region Initialization

        protected virtual void Init(VisualElement root)
        {
            // Instantiate UXML
            m_RootElement = m_VisualTreeAsset.Instantiate();
            root.Add(m_RootElement);

            //  Instantiate views
            m_MainPanel = new TwoPaneSplitView(0, 260, TwoPaneSplitViewOrientation.Horizontal);
            m_TreeView = m_TreeViewAsset.Instantiate();
            m_EntryView = m_EntryViewAsset.Instantiate();
            m_TreeSearch = m_TreeSearchAsset.Instantiate();

            //  Assemble main view
            m_TreeView.Add(m_TreeSearch);
            m_MainPanel.Add(m_TreeView);
            m_MainPanel.Add(m_EntryView);
            rootVisualElement.Add(m_MainPanel);

            //  Query main containers
            m_Tree = m_TreeView.Q<MultiColumnTreeView>("IdxImgTree");
            m_Entry = m_EntryView.Q<VisualElement>("IdxImgEntry");
            m_Search = m_TreeSearch.Q<TextField>("IdxImgTreeSearch");
            m_BarEntries = m_EntryView.Q<MultiColumnListView>("BarEntries");

            //  Query toolbar buttons
            m_OpenFile = m_RootElement.Q<ToolbarButton>("OpenFiles");
            m_ImportAssets = m_RootElement.Q<ToolbarButton>("ImportAssets");

            //  Main title
            m_Title = m_Entry.Q<Label>("Title");

            //  Property controls
            m_IsImported = m_Entry.Q<VisualElement>("IsImportedImg");
            m_FullName = m_Entry.Q<TextField>("FullName");
            m_PhysicalOffset = m_Entry.Q<LongField>("PhysicalOffset");
            m_PhysicalSize = m_Entry.Q<LongField>("PhysicalSize");
            m_UncompressedSize = m_Entry.Q<LongField>("UncompressedSize");
            m_Compress = m_Entry.Q<Toggle>("Compress");
            m_Stream = m_Entry.Q<Toggle>("Stream");

            //  BAR entries
            m_BarTitle = m_EntryView.Q<Label>("BarTitle");
            m_BarTitleOriginal = m_BarTitle.text;
            m_FilterDummies = m_EntryView.Q<Toggle>("FilterDummies");
        }
        protected virtual void AddListeners()
        {
            m_Search.RegisterValueChangedCallback(SearchTree);
            m_FilterDummies.RegisterValueChangedCallback(ev => UpdateDetailView(ev.newValue));
        }

        protected virtual void BindViews()
        {
            m_BarEntries.columns["EntryName"].makeCell = () => new Label();
            m_BarEntries.columns["EntryType"].makeCell = () => new Label();
            m_BarEntries.columns["EntryDescription"].makeCell = () => new Label();

            m_BarEntries.columns["EntryName"].bindCell = (element, index) =>
            {
                if (element is Label l && m_BarEntries.itemsSource[index] is Bar.Entry entry)
                {
                    l.AddToClassList("bold");
                    l.text = entry.Name;
                }
            };

            m_BarEntries.columns["EntryType"].bindCell = (element, index) =>
            {
                if (element is Label l && m_BarEntries.itemsSource[index] is Bar.Entry entry)
                    l.text = entry.Type.ToString();
            };

            m_BarEntries.columns["EntryDescription"].bindCell = (element, index) =>
            {
                if (element is Label l && m_BarEntries.itemsSource[index] is Bar.Entry entry)
                {
                    l.AddToClassList("italic");
                    if(entry.Type == Bar.EntryType.Anb)
                        l.text = ((MotionSet.MotionName)(index / 4)).ToString();
                }
            };
        }

        #endregion

        #region Tree search

        private void SearchTree(ChangeEvent<string> ev)
        {
            if (Root is null || Root.Count == 0)
                return;

            var query = ev.newValue;

            if (string.IsNullOrEmpty(query))
            {
                ClearFilter();
                return;
            }

            if (query.Length < 3)
                return;

            //Debug.Log($"Searching tree for '{query}'");

            var results = Root[0]
                .Where(evm => FilterEntry(evm, query))
                .Select(evm => evm.GetTreeData())
                .ToList();

            //Debug.Log($"{results.Count} results:");
            //Debug.Log(string.Join(", ", results.Select(r => r.data.Name)));

            m_Tree.SetRootItems(results);
            m_Tree.Rebuild();
        }
        private void ClearFilter()
        {
            m_Tree.SetRootItems(Root.Select(rvm => rvm.GetTreeData()).ToList());
            m_Tree.Rebuild();
        }
        protected virtual bool FilterEntry(EntryViewModel evm, string query)
        {
            return evm.Name.ToLowerInvariant().Contains(query.ToLowerInvariant());
        }

        #endregion

        #region IIdxManager

        //  IIdxManager implementation
        public Stream OpenFileFromIdx(string fileName) =>
            AssetImporter.ActiveImg.FileOpen(fileName);

        public Stream OpenFileFromIdx(Idx.Entry idxEntry) =>
            AssetImporter.ActiveImg.FileOpen(idxEntry);

        #endregion

        #region Entry view

        protected void UpdateEntryView(IEnumerable<object> selection)
        {
            var active = selection.OfType<FileViewModel>().FirstOrDefault();

            if (active is null)
            {
                HideEntryView();
                return;
            }

            SetEntryView(active);
        }
        protected void SetEntryView(FileViewModel fvm)
        {
            if(m_Title != null)
                m_Title.text = fvm.Name;
            
            m_FullName?.SetValueWithoutNotify(fvm.FullName);
            m_PhysicalOffset?.SetValueWithoutNotify(fvm.PhysicalOffset);
            m_PhysicalSize?.SetValueWithoutNotify(fvm.PhysicalLength);
            m_UncompressedSize?.SetValueWithoutNotify(fvm.UncompressedLength);
            m_Compress?.SetValueWithoutNotify(fvm.IsCompressed);
            m_Stream?.SetValueWithoutNotify(fvm.IsStream);

            m_IsImported?.EnableInClassList("hidden", !AssetImporter.IsImported(fvm));

            SetDetailView(fvm.Entry);

            m_Entry.RemoveFromClassList("hidden");
            ActiveModel = fvm;

        }
        protected void HideEntryView()
        {
            ActiveModel = null;
            m_Entry.AddToClassList("hidden");
        }

        protected void SetDetailView(Idx.Entry entry)
        {
            //  Retrieve file stream
            var stream = OpenFileFromIdx(entry);

            //  Validate
            if (!Bar.IsValid(stream))
            {
                Debug.LogWarning($"Cannot view details of {entry.GetFullName()}: Not a valid BAR file.");
                return;
            }

            ActiveBar = Bar.Read(stream);

            if(entry.IsMset())
                m_FilterDummies.RemoveFromClassList("hidden");
            else
                m_FilterDummies.AddToClassList("hidden");

            UpdateDetailView(FilterDummies);
        }

        protected void UpdateDetailView(bool filterDummies)
        {
            var src = filterDummies ? ActiveBar.Where(e => e.Name != "DUMM").ToList() : ActiveBar;

            m_BarEntries.itemsSource = src;
            m_BarEntries.Rebuild();

            m_BarTitle.text = $"{m_BarTitleOriginal} ({src.Count})";
        }

        #endregion

        #region Event handlers

        protected void OpenIdxImgFiles()
        {
            //  Select IDX file
            string idxFilePath;
            do
            {
                idxFilePath = EditorUtility.OpenFilePanel("Select IDX file", "", "IDX");

            } while (idxFilePath != string.Empty && Path.GetExtension(idxFilePath).ToLower() != ".idx");

            if (string.IsNullOrEmpty(idxFilePath))
                return;     // Cancelled by user

            //  Auto-detect corresponding IMG file; select manually if not found
            var imgFilePath = Path.ChangeExtension(idxFilePath, ".IMG");
            if (!File.Exists(imgFilePath))
            {
                do
                {
                    imgFilePath = EditorUtility.OpenFilePanel("Select IMG file", "", "IMG");

                } while (imgFilePath != string.Empty && Path.GetExtension(imgFilePath).ToLower() != ".img");

                if (string.IsNullOrEmpty(imgFilePath))
                    return;     // Cancelled by user
            }

            //Debug.Log($"Opening IDX ({idxFilePath}) and IMG ({imgFilePath})");

            var status = new OperationStatus
            {
                Current = 1,
                Total = 1,
                Message = Path.GetFileName(idxFilePath),
                OperationType = "IDX load",
                Phase = 0,
                
            };

            //  PROGRESS BAR
            if (OpProgress.Cancellable(status))
                return;

            //  Validate & read IDX file
            using var idxStream = File.OpenRead(idxFilePath);
            if (!Idx.IsValid(idxStream))
                throw new ArgumentException($"The file '{idxFilePath}' is not a valid IDX file.");

            //  Clear previous import data
            AssetImporter.Reset();

            //  PROGRESS BAR
            status.State = OperationState.Initialization;
            status.ProgressFactor = .1f;
            if (OpProgress.Cancellable(status))
                return;

            var idx = Idx.Read(idxStream);

            //  PROGRESS BAR
            status.ProgressFactor = .2f;
            if (OpProgress.Cancellable(status))
                return;

            //  Read IMG file
            _imgStream?.Dispose();
            _imgStream = File.OpenRead(imgFilePath);
            AssetImporter.ActiveImg = new Img(_imgStream, idx, false);

            //  PROGRESS BAR
            status.ProgressFactor = .8f;
            if (OpProgress.Cancellable(status))
            {
                _imgStream?.Dispose();
                _imgStream = null;
                return;
            }

            Root = new List<RootViewModel>
            {
                new(Path.GetFileName(idxFilePath), idx, this)
            };

            AssetImporter.ActiveIdxPath = idxFilePath;
            AssetImporter.ActiveImgPath = imgFilePath;

            //  PROGRESS BAR
            status.State = OperationState.Saving;
            status.ProgressFactor = .9f;
            if (OpProgress.Cancellable(status))
            {
                _imgStream?.Dispose();
                _imgStream = null;
                Root.Clear();
                return;
            }

            m_Tree.SetRootItems(Root.Select(rvm => rvm.GetTreeData()).ToList());
            m_Tree.Rebuild();

            status.State = OperationState.Finished;
            status.ProgressFactor = 1;
            if (OpProgress.Cancellable(status))
            {
                _imgStream?.Dispose();
                _imgStream = null;
                Root.Clear();
                m_Tree.Clear();
                return;
            }

            //  PROGRESS BAR
            OpProgress.Clear();
        }
        
        #endregion

        #region Static methods

        protected static bool HasMdlxMsetPair(IEnumerable<FileViewModel> assets)
        {
            var mdlx = assets.Where(fvm => fvm.Entry.IsMdlx());
            return mdlx.Any(mvm => assets.Any(fvm => fvm.FullName == Path.ChangeExtension(mvm.FullName, ".mset")));
        }
        
        #endregion
    }
}
