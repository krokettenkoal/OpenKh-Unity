using OpenKh.Unity.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Unity.Tools.IdxImg.ViewModels
{
    public class FolderViewModel : NodeViewModel
    {
        private readonly IIdxManager _idxManager;

        internal FolderViewModel(
            string name, int depth, IEnumerable<EntryParserModel> entries, IIdxManager idxManager) :
            base(name, EntryParserModel.GetEntries(entries.ToList(), depth, idxManager))
        {
            _idxManager = idxManager;
        }

        public override void Extract(string outputPath)
        {
            var childOutputPath = Path.Combine(outputPath, Name);
            Directory.CreateDirectory(childOutputPath);

            foreach (var child in Children)
            {
                child.Extract(childOutputPath);
            }
        }
    }
}
