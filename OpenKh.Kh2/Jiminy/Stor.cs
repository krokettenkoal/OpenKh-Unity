using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Stor
    {
        public const int MagicCode = 0x54534D4A;

        [Data] public byte World { get; set; }
        [Data(Count = 3)] public byte[] Padding { get; set; }
        [Data] public ushort SummaryText { get; set; }
        [Data] public ushort ObjectiveText { get; set; }
        [Data] public ushort StoryText { get; set; }
        [Data] public ushort StoryFlag { get; set; }

        public List<Stor> Read(Stream stream) => BaseJiminy<Stor>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Stor> items) => BaseJiminy<Stor>.Write(stream, MagicCode, version, items.ToList());
    }
}
