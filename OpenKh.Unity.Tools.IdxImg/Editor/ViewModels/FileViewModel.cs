using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Unity.Tools.IdxImg.Interfaces;
using System.IO;

namespace OpenKh.Unity.Tools.IdxImg.ViewModels
{
    public class FileViewModel : EntryViewModel
    {
        private readonly IIdxManager _idxManager;

        public Idx.Entry Entry { get; }
        public string FullName => Entry.GetFullName();
        public bool IsCompressed
        {
            get => Entry.IsCompressed;
            set { }
        }
        public bool IsStream
        {
            get => Entry.IsStreamed;
            set { }
        }
        //  Represents Toggle value in OpenKh.Unity.Tools.IdxImg.MainWindow
        public override bool IsChecked { get; set; }
        public long PhysicalOffset => Entry.Offset * Img.IsoBlockAlign;
        public long PhysicalLength => (Entry.BlockLength + 1) * Img.IsoBlockAlign;
        public long UncompressedLength => Entry.Length;

        internal FileViewModel(EntryParserModel entry, IIdxManager idxManager) :
            base(entry.Name)
        {
            _idxManager = idxManager;
            Entry = entry.Entry;
        }

        public override void Extract(string outputPath) =>
            ExtractForReal(Path.Combine(outputPath, Name));

        private void ExtractForReal(string fileName) =>
            File.Create(fileName).Using(stream =>
                _idxManager.OpenFileFromIdx(Entry).CopyTo(stream));
    }
}
