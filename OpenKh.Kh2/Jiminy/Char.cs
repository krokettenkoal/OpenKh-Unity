using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Char
    {
        public const int MagicCode = 0x48434D4A;

        [Data] public byte World { get; set; }
        [Data] public byte Picture { get; set; } //index jmface
        [Data] public byte PictureBgColor { get; set; }
        [Data] public byte Padding { get; set; }
        [Data] public ushort Id { get; set; }
        [Data] public ushort Title { get; set; }
        [Data] public ushort Description { get; set; }
        [Data] public ushort SourceTitle { get; set; } //used for disney and ff characters, describes where they come from
        [Data] public ushort ObjectId { get; set; } //00objentry
        [Data] public ushort Motion { get; set; }
        [Data] public ushort Stat { get; set; }
        [Data] public short PosX { get; set; } //z_un_0029e4c8
        [Data] public short PosY { get; set; }
        [Data] public short RotY { get; set; }
        [Data] public short PosX2 { get; set; }
        [Data] public short PosY2 { get; set; }
        [Data] public float Scale { get; set; } //this is read like a float, but the it behaves different?! third byte controls the size
        [Data] public float Scale2{ get; set; }

        public static List<Char> Read(Stream stream) => BaseJiminy<Char>.Read(stream).Items;
        public static void Write(Stream stream, int version, IEnumerable<Char> items) => BaseJiminy<Char>.Write(stream, MagicCode, version, items.ToList());

    }
}
