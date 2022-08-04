using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Kh2;
using OpenKh.Tools.IdxImg;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenKh.Unity.Tools.IdxImg
{
    public class IdxImg : FileHierarchyWindow
    {
        //  Visual tree assets
        [SerializeField]
        private VisualTreeAsset m_TreeViewAsset = default;
        [SerializeField]
        private VisualTreeAsset m_EntryViewAsset = default;

        //  Views
        private TwoPaneSplitView m_MainPanel;
        private VisualElement m_TreeView;
        private VisualElement m_EntryView;
        private MultiColumnTreeView m_Tree;
        private VisualElement m_Entry;

        //  Toolbar
        private ToolbarButton m_OpenFile;
        private ToolbarButton m_ImportAssets;

        //  File streams
        private Stream _imgStream;
        private Img _img;
        private string _idxFilePath;
        private string _imgFilePath;

        [MenuItem("OpenKh/Asset Importer")]
        public static void ShowIdxImg()
        {
            var wnd = GetWindow<IdxImg>();
            wnd.titleContent = new GUIContent("OpenKh Asset Importer");
        }

        public void CreateGUI()
        {
            if (m_TreeViewAsset == null || m_EntryViewAsset == null)
                return;

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            InitViews(root);
            BindViews(TreeHierarchy);
            AddListeners();
        }

        private void OpenIdxImgFiles()
        {
            //  Select IDX file
            string idxFilePath;
            do
            {
                idxFilePath = EditorUtility.OpenFilePanel("Select IDX file", "", "IDX");

            } while (idxFilePath != string.Empty && !idxFilePath.ToLower().EndsWith("idx"));

            if (string.IsNullOrEmpty(idxFilePath))
                return;     // Cancelled by user

            //  Select IMG file
            string imgFilePath;
            do
            {
                imgFilePath = EditorUtility.OpenFilePanel("Select IMG file", "", "IMG");

            } while (imgFilePath != string.Empty && !imgFilePath.ToLower().EndsWith("img"));

            if (string.IsNullOrEmpty(imgFilePath))
                return;     // Cancelled by user
            
            //Debug.Log($"Opening IDX ({idxFilePath}) and IMG ({imgFilePath})");

            //  Validate & read IDX file
            using var idxStream = File.OpenRead(idxFilePath);
            if (!Idx.IsValid(idxStream))
                throw new ArgumentException($"The file '{idxFilePath}' is not a valid IDX file.");
            var idx = Idx.Read(idxStream);


            //  Read IMG file
            _imgStream?.Dispose();
            _imgStream = File.OpenRead(imgFilePath);
            _img = new Img(_imgStream, idx, false);

            _idxFilePath = idxFilePath;
            _imgFilePath = imgFilePath;

            SetHierarchyFromIdx(Path.GetFileName(_idxFilePath), idx);
            m_Tree.SetRootItems(TreeHierarchy);
            m_Tree.Rebuild();
        }

        private void ImportAssets()
        {
            foreach (var folder in Hierarchy)
            {
                ImportAsset(folder);
            }
        }

        private void ImportAsset(IFolderOrFile entry)
        {
            switch (entry)
            {
                case FileEntry file:
                {
                    if (!file.Import)
                        return;

                    Debug.Log($"Importing {file.Name}..");
                    return;
                }
                case FolderEntry folder:
                {
                    foreach (var child in folder.Children)
                    {
                        ImportAsset(child);
                    }

                    break;
                }
            }
        }
        private void InitViews(VisualElement root)
        {
            // Instantiate UXML
            VisualElement template = m_VisualTreeAsset.Instantiate();
            root.Add(template);

            m_MainPanel = new TwoPaneSplitView(0, 260, TwoPaneSplitViewOrientation.Horizontal);
            m_TreeView = m_TreeViewAsset.Instantiate();
            m_EntryView = m_EntryViewAsset.Instantiate();
            m_Tree = m_TreeView.Q<MultiColumnTreeView>("IdxImgTree");
            m_Entry = m_EntryView.Q<VisualElement>("IdxImgEntry");
            m_OpenFile = template.Q<ToolbarButton>("OpenFiles");
            m_ImportAssets = template.Q<ToolbarButton>("ImportAssets");

            m_MainPanel.Add(m_TreeView);
            m_MainPanel.Add(m_EntryView);
            rootVisualElement.Add(m_MainPanel);
        }
        private void BindViews(IList<TreeViewItemData<IFolderOrFile>> rootItems)
        {
            m_Tree.SetRootItems(rootItems);

            m_Tree.columns["name"].makeCell = () => new Label();
            m_Tree.columns["import"].makeCell = () => new Toggle();

            m_Tree.columns["name"].bindCell = (element, index) =>
            {
                if (element is Label l)
                    l.text = m_Tree.GetItemDataForIndex<IFolderOrFile>(index).Name;
            };

            m_Tree.columns["import"].bindCell = (element, index) =>
            {
                if (element is Toggle t)
                {
                    var entry = m_Tree.GetItemDataForIndex<IFolderOrFile>(index);
                    t.SetValueWithoutNotify(entry.Import);
                    t.showMixedValue = entry is FolderEntry {Import: false} f && f.Children.Any(a => a.Import);
                    t.RegisterValueChangedCallback(ev => ToggleImport(entry, ev));
                }
            };
        }
        private void AddListeners()
        {
            m_OpenFile.clicked += OpenIdxImgFiles;
            m_ImportAssets.clicked += ImportAssets;
        }
        private void ToggleImport(IFolderOrFile entry, ChangeEvent<bool> ev)
        {
            if (entry.Import == ev.newValue)
                return;
            
            //Debug.Log($"{entry.Name}: {ev.previousValue} -> {ev.newValue}");
            entry.Import = ev.newValue;

            if (entry is FolderEntry)
                m_Tree.RefreshItems();
        }
    }
}

