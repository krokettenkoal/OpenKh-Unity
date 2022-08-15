using OpenKh.Kh2;
using OpenKh.Unity.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Unity.Tools.IdxImg.ViewModels
{
    public class IdxViewModel : NodeViewModel
    {
        private readonly IIdxManager _idxManager;

        internal IdxViewModel(string name, Idx.Entry entry, IIdxManager idxManager) :
            base(name, GetChildren(name, idxManager))
        {
            _idxManager = idxManager;
        }

        public string ShortName => Path.GetFileNameWithoutExtension(Name);

        private static IEnumerable<EntryViewModel> GetChildren(string idxName, IIdxManager idxManager)
        {
            var idxStream = idxManager.OpenFileFromIdx(idxName);
            if (idxStream == null)
                return new EntryViewModel[0];

            using (idxStream)
            {
                return EntryParserModel.GetChildren(Idx.Read(idxStream), idxManager);
            }
        }

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
                child.Extract(outputPath);
            }    
        }
    }
}
