using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenCover.Framework.Model;
using OpenKh.Kh2;
using OpenKh.Tools.IdxImg;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OpenKh.Unity.Tools
{
    // Base class for all windows that display planet information.
    public class FileHierarchyWindow : EditorWindow
{
    [SerializeField]
    protected VisualTreeAsset m_VisualTreeAsset = default;

    internal static int m_Index { get; set; } = -1;

    // Nested interface that can be either a an asset or a folder.
    protected interface IFolderOrFile
    {
        public int Id
        {
            get;
        }
        public string Name
        {
            get;
        }
        public bool Import
        {
            get;
            set;
        }

        public TreeViewItemData<IFolderOrFile> ItemData { get; }
    }

    // Nested class that represents an asset.
    [Serializable]
    protected class FileEntry : IFolderOrFile
    {
        public int Id
        {
            get;
        }
        public string Name
        {
            get;
        }

        public bool Import
        {
            get;
            set;
        }

        public Idx.Entry Data { get; }

        public TreeViewItemData<IFolderOrFile> ItemData => new(Id, this);

        public FileEntry(string name, Idx.Entry data, int id, bool import = false)
        {
            Id = id;
            Name = name;
            Data = data;
            Import = import;
        }
    }

    // Nested class that represents a folder.
    [Serializable]
    protected class FolderEntry : IFolderOrFile
    {
        public int Id
        {
            get;
        }
        public string Name
        {
            get;
        }

        public bool Import
        {
            get
            {
                return Children.All(a => a.Import);
            }
            set
            {
                foreach (var child in Children)
                {
                    child.Import = value;
                }
            }
        }

        public TreeViewItemData<IFolderOrFile> ItemData => new(Id, this, Children.OrderBy(c => c.Name).Select(a => a.ItemData).ToList());

        public List<IFolderOrFile> Children;

        public FolderEntry(string name, int id, List<IFolderOrFile> children = null)
        {
            Id = id;
            Name = name;
            Children = children ?? new List<IFolderOrFile>();
        }
    }

    // Representation of the IDX file hierarchy
    protected static IReadOnlyList<FolderEntry> Hierarchy { get; private set; } = new List<FolderEntry>();

    //  Expresses folders and files as TreeViewItemData objects
    protected static IList<TreeViewItemData<IFolderOrFile>> TreeHierarchy
    {
        get
        {
            m_Index = -1;
            return Hierarchy.Select(node => node.ItemData).ToList();
        }
    }

    protected static void SetHierarchyFromIdx(string idxFileName, List<Idx.Entry> entries)
    {
        var id = 0;
        var root = new FolderEntry(idxFileName, id++);
        var hierarchy = new List<FolderEntry>(1) { root };
        var fileNameTester = new Regex(@"^\w\+.\w+$");

        foreach (var entry in entries)
        {
            var segments = entry.GetFullName().Split('/');
            var parent = root;

            foreach (var segment in segments)
            {
                var idx = parent.Children.FindIndex(f => f.Name == segment);

                if (idx == -1)
                {
                    if (fileNameTester.IsMatch(segment))
                    {
                        //  Create FileEntry
                        parent.Children.Add(new FileEntry(segment, entry, id++));
                        break;
                    }

                    //  Create FolderEntry and continue crawling
                    var folder = new FolderEntry(segment, id++);
                    parent.Children.Add(folder);
                    parent = folder;
                }
                else
                {
                    if (parent.Children[idx] is FileEntry)
                        break; // File already existing (should not be the case)

                    if (parent.Children[idx] is not FolderEntry f)
                        break; // Neither File nor Folder (should not be the case either)

                    parent = f;
                }
            }
        }

        Hierarchy = hierarchy;
    }
}
}
