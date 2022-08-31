using System;
using System.Collections.Generic;
using System.IO;
using OpenKh.Unity.Exporter.Aset.Texture;

namespace OpenKh.Unity.Exporter.Aset.Binary {
    public class TIMCollection {
        public static Texex2[] Load(Stream fs) {
            var pos = Convert.ToInt32(fs.Position);
            var br = new BinaryReader(fs);
            var offsets = new List<int>();
            if (br.ReadUInt32() == 0xffffffffU) {
                var cnt = br.ReadInt32();
                for (var x = 0; x < cnt; x++) {
                    offsets.Add(pos + br.ReadInt32());
                }
            }
            else {
                offsets.Add(pos);
            }

            var al = new List<Texex2>();
            for (var x = 0; x < offsets.Count; x++) {
                var off0 = offsets[x];
                var off1 = (x + 1 < offsets.Count) ? offsets[x + 1] : Convert.ToInt32(fs.Length);
                var bin = new byte[off1 - off0];
                fs.Position = off0;
                _ = fs.Read(bin, 0, off1 - off0);

                al.Add(TIMReader.Load(new MemoryStream(bin, false)));
            }
            return al.ToArray();
        }
    }
}
