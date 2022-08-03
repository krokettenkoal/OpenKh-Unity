using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Kh2.Messages.Internals
{
    internal class TurkishSystemDecode : IMessageDecode
    {
        private static readonly char[] _tableGeneric = Enumerable.Range(0, 0x100).Select(x => '?').ToArray();

        public static readonly Dictionary<byte, BaseCmdModel> _table = new Dictionary<byte, BaseCmdModel>
        {
            [0x00] = new SimpleCmdModel(MessageCommand.End),
            [0x01] = new TextCmdModel(' '),
            [0x02] = new TextCmdModel('\n'),
            [0x03] = new SimpleCmdModel(MessageCommand.Reset),
            [0x04] = new SingleDataCmdModel(MessageCommand.Theme),
            [0x05] = new DataCmdModel(MessageCommand.Unknown05, 6),
            [0x06] = new SingleDataCmdModel(MessageCommand.Unknown06),
            [0x07] = new DataCmdModel(MessageCommand.Color, 4),
            [0x08] = new DataCmdModel(MessageCommand.Unknown08, 3),
            [0x09] = new SingleDataCmdModel(MessageCommand.PrintIcon),
            [0x0a] = new SingleDataCmdModel(MessageCommand.TextScale),
            [0x0b] = new SingleDataCmdModel(MessageCommand.TextWidth),
            [0x0c] = new SingleDataCmdModel(MessageCommand.LineSpacing),
            [0x0d] = new SimpleCmdModel(MessageCommand.Unknown0d),
            [0x0e] = new SingleDataCmdModel(MessageCommand.Unknown0e),
            [0x0f] = new DataCmdModel(MessageCommand.Unknown0f, 5),
            [0x10] = new SimpleCmdModel(MessageCommand.Clear),
            [0x11] = new DataCmdModel(MessageCommand.Position, 4),
            [0x12] = new DataCmdModel(MessageCommand.Unknown12, 2),
            [0x13] = new DataCmdModel(MessageCommand.Unknown13, 4),
            [0x14] = new DataCmdModel(MessageCommand.Delay, 2),
            [0x15] = new DataCmdModel(MessageCommand.CharDelay, 2),
            [0x16] = new SingleDataCmdModel(MessageCommand.Unknown16),
            [0x17] = new DataCmdModel(MessageCommand.DelayAndFade, 2),
            [0x18] = new DataCmdModel(MessageCommand.Unknown18, 2),
            [0x19] = new TableCmdModel(MessageCommand.Table2, _tableGeneric),
            [0x1a] = new TableCmdModel(MessageCommand.Table3, _tableGeneric),
            [0x1b] = new TableCmdModel(MessageCommand.Table4, _tableGeneric),
            [0x1c] = new TableCmdModel(MessageCommand.Table5, _tableGeneric),
            [0x1d] = new TableCmdModel(MessageCommand.Table6, _tableGeneric),
            [0x1e] = new TableCmdModel(MessageCommand.Table7, _tableGeneric),
            [0x1f] = new TableCmdModel(MessageCommand.Table8, _tableGeneric),
            [0x20] = new TextCmdModel('⬛'),
            [0x21] = new TextCmdModel('０'),
            [0x22] = new TextCmdModel('１'),
            [0x23] = new TextCmdModel('２'),
            [0x24] = new TextCmdModel('３'),
            [0x25] = new TextCmdModel('４'),
            [0x26] = new TextCmdModel('５'),
            [0x27] = new TextCmdModel('６'),
            [0x28] = new TextCmdModel('７'),
            [0x29] = new TextCmdModel('８'),
            [0x2a] = new TextCmdModel('９'),
            [0x2b] = new TextCmdModel('+'),
            [0x2c] = new TextCmdModel('−'),
            [0x2d] = new TextCmdModel('ₓ'),
            [0x2e] = new TextCmdModel('A'),
            [0x2f] = new TextCmdModel('B'),
            [0x30] = new TextCmdModel('C'),
            [0x31] = new TextCmdModel('D'),
            [0x32] = new TextCmdModel('E'),
            [0x33] = new TextCmdModel('F'),
            [0x34] = new TextCmdModel('G'),
            [0x35] = new TextCmdModel('H'),
            [0x36] = new TextCmdModel('I'),
            [0x37] = new TextCmdModel('J'),
            [0x38] = new TextCmdModel('K'),
            [0x39] = new TextCmdModel('L'),
            [0x3a] = new TextCmdModel('M'),
            [0x3b] = new TextCmdModel('N'),
            [0x3c] = new TextCmdModel('O'),
            [0x3d] = new TextCmdModel('P'),
            [0x3e] = new TextCmdModel('Q'),
            [0x3f] = new TextCmdModel('R'),
            [0x40] = new TextCmdModel('S'),
            [0x41] = new TextCmdModel('T'),
            [0x42] = new TextCmdModel('U'),
            [0x43] = new TextCmdModel('V'),
            [0x44] = new TextCmdModel('W'),
            [0x45] = new TextCmdModel('X'),
            [0x46] = new TextCmdModel('Y'),
            [0x47] = new TextCmdModel('Z'),
            [0x48] = new TextCmdModel('!'),
            [0x49] = new TextCmdModel('?'),
            [0x4a] = new TextCmdModel('%'),
            [0x4b] = new TextCmdModel('/'),
            [0x4c] = new TextCmdModel('※'),
            [0x4d] = new TextCmdModel('、'),
            [0x4e] = new TextCmdModel('。'),
            [0x4f] = new TextCmdModel('.'),
            [0x50] = new TextCmdModel(','),
            [0x51] = new TextCmdModel(';'),
            [0x52] = new TextCmdModel(':'),
            [0x53] = new TextCmdModel('…'),
            [0x54] = new TextCmdModel("-"),
            [0x55] = new TextCmdModel('–'),
            [0x56] = new TextCmdModel('〜'),
            [0x57] = new TextCmdModel("'"),
            [0x58] = new UnsupportedCmdModel(0x58), // Unused
            [0x59] = new UnsupportedCmdModel(0x59), // Unused
            [0x5a] = new TextCmdModel('('),
            [0x5b] = new TextCmdModel(')'),
            [0x5c] = new TextCmdModel('「'),
            [0x5d] = new TextCmdModel('」'),
            [0x5e] = new TextCmdModel('『'),
            [0x5f] = new TextCmdModel('』'),
            [0x60] = new TextCmdModel('“'),
            [0x61] = new TextCmdModel('”'),
            [0x62] = new TextCmdModel('['),
            [0x63] = new TextCmdModel(']'),
            [0x64] = new TextCmdModel('<'),
            [0x65] = new TextCmdModel('>'),
            [0x66] = new TextCmdModel('-'),
            [0x67] = new TextCmdModel("–"),
            [0x68] = new TextCmdModel('⤷'), // Used only in EVT
            [0x69] = new TextCmdModel('♩'),
            [0x6a] = new TextCmdModel('⇾'), // Used only in EVT
            [0x6b] = new TextCmdModel('⇽'), // Used only in EVT
            [0x6c] = new TextCmdModel('◯'),
            [0x6d] = new TextCmdModel('✕'),
            [0x6e] = new UnsupportedCmdModel(0x6e), // Unused
            [0x6f] = new UnsupportedCmdModel(0x6e), // Unused
            [0x70] = new UnsupportedCmdModel(0x70), // Unused
            [0x71] = new UnsupportedCmdModel(0x71), // Unused
            [0x72] = new UnsupportedCmdModel(0x72), // Unused
            [0x73] = new SimpleCmdModel(MessageCommand.Tabulation),
            [0x74] = new TextCmdModel("I"),
            [0x75] = new TextCmdModel("II"),
            [0x76] = new TextCmdModel("III"),
            [0x77] = new TextCmdModel("IV"),
            [0x78] = new TextCmdModel("V"),
            [0x79] = new TextCmdModel("VI"),
            [0x7a] = new TextCmdModel("VII"),
            [0x7b] = new TextCmdModel("VIII"),
            [0x7c] = new TextCmdModel("IX"),
            [0x7d] = new TextCmdModel("X"),
            [0x7e] = new TextCmdModel("XIII"),
            [0x7f] = new TextCmdModel('α'),
            [0x80] = new TextCmdModel('β'),
            [0x81] = new TextCmdModel('γ'),
            [0x82] = new TextCmdModel('⭑'),
            [0x83] = new TextCmdModel('⭒'),
            [0x84] = new TextCmdModel("XI"),
            [0x85] = new TextCmdModel("XII"),
            [0x86] = new TextCmdModel('&'),
            [0x87] = new TextCmdModel('#'),
            [0x88] = new TextCmdModel('®'),
            [0x89] = new TextCmdModel('▴'),
            [0x8a] = new TextCmdModel('▾'),
            [0x8b] = new TextCmdModel('▸'),
            [0x8c] = new TextCmdModel('◂'),
            [0x8d] = new TextCmdModel('°'),
            [0x8e] = new TextCmdModel("♪"),
            [0x8f] = new UnsupportedCmdModel(0x8f), // Unused
            [0x90] = new TextCmdModel('0'),
            [0x91] = new TextCmdModel('1'),
            [0x92] = new TextCmdModel('2'),
            [0x93] = new TextCmdModel('3'),
            [0x94] = new TextCmdModel('4'),
            [0x95] = new TextCmdModel('5'),
            [0x96] = new TextCmdModel('6'),
            [0x97] = new TextCmdModel('7'),
            [0x98] = new TextCmdModel('8'),
            [0x99] = new TextCmdModel('9'),
            [0x9a] = new TextCmdModel('a'),
            [0x9b] = new TextCmdModel('b'),
            [0x9c] = new TextCmdModel('c'),
            [0x9d] = new TextCmdModel('d'),
            [0x9e] = new TextCmdModel('e'),
            [0x9f] = new TextCmdModel('f'),
            [0xa0] = new TextCmdModel('g'),
            [0xa1] = new TextCmdModel('h'),
            [0xa2] = new TextCmdModel('i'),
            [0xa3] = new TextCmdModel('j'),
            [0xa4] = new TextCmdModel('k'),
            [0xa5] = new TextCmdModel('l'),
            [0xa6] = new TextCmdModel('m'),
            [0xa7] = new TextCmdModel('n'),
            [0xa8] = new TextCmdModel('o'),
            [0xa9] = new TextCmdModel('p'),
            [0xaa] = new TextCmdModel('q'),
            [0xab] = new TextCmdModel('r'),
            [0xac] = new TextCmdModel('s'),
            [0xad] = new TextCmdModel('t'),
            [0xae] = new TextCmdModel('u'),
            [0xaf] = new TextCmdModel('v'),
            [0xb0] = new TextCmdModel('w'),
            [0xb1] = new TextCmdModel('x'),
            [0xb2] = new TextCmdModel('y'),
            [0xb3] = new TextCmdModel('z'),
            [0xb4] = new TextCmdModel('Æ'),
            [0xb5] = new TextCmdModel('æ'),
            [0xb6] = new TextCmdModel('ß'),
            [0xb7] = new TextCmdModel('ş'),
            [0xb8] = new TextCmdModel('ğ'),
            [0xb9] = new TextCmdModel('â'),
            [0xba] = new TextCmdModel('ä'),
            [0xbb] = new TextCmdModel('è'),
            [0xbc] = new TextCmdModel('é'),
            [0xbd] = new TextCmdModel('ê'),
            [0xbe] = new TextCmdModel('ë'),
            [0xbf] = new TextCmdModel('ì'),
            [0xc0] = new TextCmdModel('í'),
            [0xc1] = new TextCmdModel('î'),
            [0xc2] = new TextCmdModel('ï'),
            [0xc3] = new TextCmdModel('ñ'),
            [0xc4] = new TextCmdModel('ò'),
            [0xc5] = new TextCmdModel('ó'),
            [0xc6] = new TextCmdModel('ô'),
            [0xc7] = new TextCmdModel('ö'),
            [0xc8] = new TextCmdModel('ù'),
            [0xc9] = new TextCmdModel('ú'),
            [0xca] = new TextCmdModel('û'),
            [0xcb] = new TextCmdModel('ü'),
            [0xcc] = new TextCmdModel('º'),
            [0xcd] = new TextCmdModel('—'),
            [0xce] = new TextCmdModel('»'),
            [0xcf] = new TextCmdModel('«'),
            [0xd0] = new TextCmdModel('Ş'),
            [0xd1] = new TextCmdModel('Ğ'),
            [0xd2] = new TextCmdModel('Â'),
            [0xd3] = new TextCmdModel('Ä'),
            [0xd4] = new TextCmdModel('È'),
            [0xd5] = new TextCmdModel('É'),
            [0xd6] = new TextCmdModel('Ê'),
            [0xd7] = new TextCmdModel('Ë'),
            [0xd8] = new TextCmdModel('Ì'),
            [0xd9] = new TextCmdModel('Í'),
            [0xda] = new TextCmdModel('Î'),
            [0xdb] = new TextCmdModel('İ'),
            [0xdc] = new TextCmdModel('Ñ'),
            [0xdd] = new TextCmdModel('Ò'),
            [0xde] = new TextCmdModel('Ó'),
            [0xdf] = new TextCmdModel('Ô'),
            [0xe0] = new TextCmdModel('Ö'),
            [0xe1] = new TextCmdModel('Ù'),
            [0xe2] = new TextCmdModel('Ú'),
            [0xe3] = new TextCmdModel('Û'),
            [0xe4] = new TextCmdModel('Ü'),
            [0xe5] = new TextCmdModel('ı'),
            [0xe6] = new TextCmdModel('¿'),
            [0xe7] = new TextCmdModel('Ç'),
            [0xe8] = new TextCmdModel('ç'),
            [0xe9] = new TextCmdModel('‛'),
            [0xea] = new TextCmdModel('’'),
            [0xeb] = new TextCmdModel('`'),
            [0xec] = new TextCmdModel('´'),
            [0xed] = new TextCmdModel('"'),
            [0xee] = new TextCmdModel('\''),
            [0xef] = new TextCmdModel('★'),
            [0xf0] = new TextCmdModel('☆'),
            [0xf1] = new TextCmdModel('■'),
            [0xf2] = new TextCmdModel('□'),
            [0xf3] = new TextCmdModel('▲'),
            [0xf4] = new TextCmdModel('△'),
            [0xf5] = new TextCmdModel('●'),
            [0xf6] = new TextCmdModel('○'),
            [0xf7] = new TextCmdModel('♪'),
            [0xf8] = new TextCmdModel('♫'),
            [0xf9] = new TextCmdModel('→'),
            [0xfa] = new TextCmdModel('←'),
            [0xfb] = new TextCmdModel('↑'),
            [0xfc] = new TextCmdModel('↓'),
            [0xfd] = new TextCmdModel('・'),
            [0xfe] = new TextCmdModel('❤'),
            [0xff] = new UnsupportedCmdModel(0xff), // Unused
        };

        public List<MessageCommandModel> Decode(byte[] data) =>
            new BaseMessageDecoder(_table, data).Decode();
    }
}
