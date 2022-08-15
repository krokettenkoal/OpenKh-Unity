using System;
using System.Collections.Generic;
using System.Linq;
using OpenKh.Kh2;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using UnityEngine.UIElements;

namespace OpenKh.Unity.Tools.IdxImg
{
    public static class Extensions
    {
        public static string GetFullName(this Idx.Entry entry) =>
            IdxName.Lookup(entry) ?? $"@{entry.Hash32:X08}_{entry.Hash16}";

        public static IEnumerable<T> Select<T>(this EntryViewModel evm, Func<EntryViewModel, T> fn)
        {
            var res = new List<T>
            {
                fn(evm)
            };

            if (evm is not NodeViewModel b)
                return res;

            foreach (var child in b.Children)
            {
                res.AddRange(child.Select(fn));
            }

            return res;
        }
        public static IEnumerable<EntryViewModel> Where(this EntryViewModel node, Func<EntryViewModel, bool> fn)
        {
            var res = new List<EntryViewModel>();

            if (fn(node))
                res.Add(node);

            if (node is not NodeViewModel nvm)
                return res;

            foreach (var child in nvm.Children)
            {
                res.AddRange(child.Where(fn));
            }

            return res;
        }
        public static void ForEach(this EntryViewModel node, Action<EntryViewModel> fn)
        {
            fn(node);

            if (node is not NodeViewModel nvm)
                return;

            foreach (var child in nvm.Children)
            {
                child.ForEach(fn);
            }
        }
        public static EntryViewModel FirstOrDefault(this EntryViewModel node, Func<EntryViewModel, bool> fn)
        {
            if (fn(node))
                return node;

            if (node is NodeViewModel nvm)
            {
                return nvm.Children.Select(child => child.FirstOrDefault(fn)).FirstOrDefault(n => n != null);
            }

            return null;
        }

        public static TreeViewItemData<EntryViewModel> GetTreeData(this EntryViewModel evm) => evm switch
        {
            NodeViewModel nvm => new TreeViewItemData<EntryViewModel>(nvm.GetHashCode(), nvm,
                nvm.Children.Select(GetTreeData).ToList()),
            _ => new TreeViewItemData<EntryViewModel>(evm.GetHashCode(), evm),
        };

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var elem in enumerable)
            {
                action(elem);
            }
        }

    }
}
