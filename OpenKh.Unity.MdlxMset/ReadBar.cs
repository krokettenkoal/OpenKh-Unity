using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace OpenKh.Unity.MdlxMset.Binary
{
    public class ReadBar {
        public class BarEntry {
            public int k;
            public string id;
            public int offset, length;
            public byte[] binaryData;
        }
        public static BarEntry[] Explode(Stream si) {
            var br = new BinaryReader(si);

            if (br.ReadByte() != 'B' || br.ReadByte() != 'A' || br.ReadByte() != 'R' || br.ReadByte() != 1)
                throw new NotSupportedException();
            var cx = br.ReadInt32();
            br.ReadBytes(8);

            var entries = new List<BarEntry>();
            for (var x = 0; x < cx; x++) {
                var entry = new BarEntry {
                    k = br.ReadInt32(),
                    id = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4)).TrimEnd((char)0),
                    offset = br.ReadInt32(),
                    length = br.ReadInt32()
                };
                entries.Add(entry);
            }
            for (var x = 0; x < cx; x++) {
                var entry = entries[x];
                si.Position = entry.offset;
                entry.binaryData = br.ReadBytes(entry.length);
                Debug.Assert(entry.binaryData.Length == entry.length);
            }
            return entries.ToArray();
        }
        public static BarEntry[] Explode2(MemoryStream si) {
            var br = new BinaryReader(si);

            if (br.ReadByte() != 'B' || br.ReadByte() != 'A' || br.ReadByte() != 'R' || br.ReadByte() != 1)
                throw new NotSupportedException();
            var cx = br.ReadInt32();
            br.ReadBytes(8);

            var entries = new List<BarEntry>();
            for (var x = 0; x < cx; x++) {
                var entry = new BarEntry {
                    k = br.ReadInt32(),
                    id = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4)).TrimEnd((char)0),
                    offset = br.ReadInt32(),
                    length = br.ReadInt32()
                };

                if (((entry.offset + entry.length) & 15) != 0) entry.length += 16 - ((entry.offset + entry.length) & 15);
                entries.Add(entry);
            }
            for (var x = 0; x < cx; x++) {
                var ent = entries[x];
                si.Position = ent.offset;
                ent.binaryData = new byte[ent.length];
                _ = si.Read(ent.binaryData, 0, ent.length);
            }
            return entries.ToArray();
        }
    }
}
