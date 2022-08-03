using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Ques
    {
        public const int MagicCode = 0x55514D4A;

        public enum QuestStatus : ushort
        {
            Disabled = 0,
            Draw = 1,
            Cleared = 2,
            FullyCleared = 3
        }

        [Data] public ushort World { get; set; }
        [Data] public ushort CategoryText { get; set; }
        [Data] public ushort Title { get; set; }
        [Data] public QuestStatus Status { get; set; } //z_un_002a99c8
        [Data] public ushort StoryFlag { get; set; }
        [Data] public ushort GameId { get; set; }
        [Data] public ushort Score { get; set; }
        [Data] public ushort ClearCondition { get; set; }

        public List<Ques> Read(Stream stream) => BaseJiminy<Ques>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Ques> items) => BaseJiminy<Ques>.Write(stream, MagicCode, version, items.ToList());
    }
}
