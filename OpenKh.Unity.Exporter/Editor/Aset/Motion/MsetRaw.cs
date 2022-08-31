using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpenKh.Unity.Exporter.Aset.Motion
{
    public class MsetRM {
        public List<Matrix4x4> matrices = new();
    }
    public class MsetRawblk {
        public List<MsetRM> rms = new();
        public int boneCount;
        public int frameCount { get { return rms.Count; } }

        public MsetRawblk(Stream si) {
            var br = new BinaryReader(si);
            si.Position = 0x90;
            var v90 = br.ReadInt32();
            if (v90 != 1) throw new NotSupportedException("v90 != 1");
            si.Position = 0xA0;
            var va0 = boneCount = br.ReadInt32(); // @0xa0 cnt axbone
            si.Position = 0xB4;
            var vb4 = br.ReadInt32(); // @0xb4 cnt frames

            si.Position = 0xF0;

            rms.Capacity = vb4;
            for (var t = 0; t < vb4; t++) {
                var rm = new MsetRM
                {
                    matrices =
                    {
                        Capacity = va0
                    }
                };

                rms.Add(rm);

                for (var x = 0; x < va0; x++) {
                    var m1 = new Matrix4x4
                    {
                        [0, 0] = br.ReadSingle(),
                        [0, 1] = br.ReadSingle(),
                        [0, 2] = br.ReadSingle(),
                        [0, 3] = br.ReadSingle(),
                        [1, 0] = br.ReadSingle(),
                        [1, 1] = br.ReadSingle(),
                        [1, 2] = br.ReadSingle(),
                        [1, 3] = br.ReadSingle(),
                        [2, 0] = br.ReadSingle(),
                        [2, 1] = br.ReadSingle(),
                        [2, 2] = br.ReadSingle(),
                        [2, 3] = br.ReadSingle(),
                        [3, 0] = br.ReadSingle(),
                        [3, 1] = br.ReadSingle(),
                        [3, 2] = br.ReadSingle(),
                        [3, 3] = br.ReadSingle()
                    };

                    rm.matrices.Add(m1);
                }
            }
        }
    }
}
