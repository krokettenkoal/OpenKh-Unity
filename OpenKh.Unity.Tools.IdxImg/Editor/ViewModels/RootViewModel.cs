using OpenKh.Kh2;
using OpenKh.Unity.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Unity.Tools.IdxImg.ViewModels
{
    internal class RootViewModel : NodeViewModel
    {
        private readonly IIdxManager _idxManager;

        public RootViewModel(string name, List<Idx.Entry> entries, IIdxManager idxManager) :
            base(name, EntryParserModel.GetChildren(entries, idxManager))
        {
            _idxManager = idxManager;
        }

        public string ShortName => Path.GetFileNameWithoutExtension(Name);

        public override void Extract(string outputPath)
        {
            foreach (var child in Children)
            {
                child.Extract(Path.Combine(outputPath, ShortName));
            }
        }

        public void ExtractAndMerge(string outputPath)
        {
            foreach (var child in Children)
            {
                var childOutputPath = Path.Combine(outputPath, ShortName);
                if (child is IdxViewModel idxVm)
                    idxVm.ExtractAndMerge(childOutputPath);
                else
                    child.Extract(childOutputPath);
            }
        }
    }
}
