using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace OpenKh.UnityEditor
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
        public string Name
        {
            get;
        }

        public bool Import
        {
            get;
            set;
        }

        public TreeViewItemData<IFolderOrFile> ItemData => new(++m_Index, this);

        public FileEntry(string name, bool import = false)
        {
            Name = name;
            Import = import;
        }
    }

    // Nested class that represents a folder.
    [Serializable]
    protected class FolderEntry : IFolderOrFile
    {
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

        public TreeViewItemData<IFolderOrFile> ItemData => new(++m_Index, this, Children.Select(a => a.ItemData).ToList());

        public readonly IReadOnlyList<IFolderOrFile> Children;

        public FolderEntry(string name, IReadOnlyList<IFolderOrFile> children)
        {
            Name = name;
            Children = children;
        }
    }

    // Folders inside the IMG file
    protected static List<FolderEntry> Folders = new()
    {
        new FolderEntry("anm", new List<IFolderOrFile>()
        {
            new FolderEntry("ex", new List<IFolderOrFile>()
            {
                new FolderEntry("p_ex_100", new List<FileEntry>()
                {
                    new("EEX00001C.anb"),
                    new("EEX00002C.anb"),
                    new("EEX00003C.anb"),
                    new("EEX00004C.anb"),
                    new("EEX00005C.anb"),
                    new("EEX00006C.anb"),
                    new("EEX00007C.anb"),
                    new("EEX00008C.anb"),
                    new("EEX00009C.anb"),
                    new("EEX00010C.anb"),
                })
            }),
            new FolderEntry("ex_r", new List<IFolderOrFile>()
            {
                new FolderEntry("w_ex_010", new List<FileEntry>()
                {
                    new("EEX00001C.anb"),
                    new("EEX00002C.anb"),
                    new("EEX00003C.anb"),
                    new("EEX00004C.anb"),
                    new("EEX00005C.anb"),
                    new("EEX00006C.anb"),
                    new("EEX00007C.anb"),
                    new("EEX00008C.anb"),
                    new("EEX00009C.anb"),
                    new("EEX00010C.anb"),
                })
            })

        }),

        new FolderEntry("obj", new List<FileEntry>()
        {
            new("P_EX100.mdlx"),
            new("P_EX100.mset"),
            new("P_EX100.a.us"),
        }),

    };

    //  Expresses folders and files as TreeViewItemData objects
    protected static IList<TreeViewItemData<IFolderOrFile>> TreeNodes
    {
        get
        {
            m_Index = -1;
            return Folders.Select(f => f.ItemData).ToList();
        }
    }
}
}
