using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace OpenKh.Unity.MdlxMset.Motion {

    public class AxBone {
        public int cur, parent, v08, v0c;
        public float x1, y1, z1, w1; // S
        public float x2, y2, z2, w2; // R
        public float x3, y3, z3, w3; // T

        public Matrix4x4 BoneMatrix => Matrix4x4.TRS(new Vector3(x3, y3, z3), Quaternion.Euler(x2, y2, z2), new Vector3(x1, y1, z1));

        public AxBone Clone() {
            return (AxBone)MemberwiseClone();
        }
    }
    public class T31 {
        public int off, len;
        public int postbl3;
        public List<T11> al11 = new();
        public List<T12> al12 = new();
        public List<T13vif> al13 = new();
        public T21 t21;
        public T32 t32;

        public Matrix4x4[] skeleton; // skeleton base pose
        public Matrix4x4[] skeletonInverse; // skeleton inverse transforms

        public T31(int off, int len, int postbl3) {
            this.off = off;
            this.len = len;
            this.postbl3 = postbl3;
        }
    }

    public class T11 {
        public int off, len;
        public int c1;

        public T11(int off, int len, int c1) {
            this.off = off;
            this.len = len;
            this.c1 = c1;
        }
    }

    public class T12 {
        public int off, len;
        public int c1;

        public T12(int off, int len, int c1) {
            this.off = off;
            this.len = len;
            this.c1 = c1;
        }
    }

    public class T13vif {
        public int off, len;
        public int texi;
        public int[] alaxi;
        public byte[] bin;

        public T13vif(int off, int len, int texi, int[] alaxi, byte[] bin) {
            this.off = off;
            this.len = len;
            this.texi = texi;
            this.alaxi = alaxi;
            this.bin = bin;
        }
    }

    public class T21 {
        public int offset, length;
        public List<AxBone> axBones = new();

        public T21(int offset, int length) {
            this.offset = offset;
            this.length = length;
        }
    }

    public class T32 {
        public int offset, length;

        public T32(int offset, int length) {
            this.offset = offset;
            this.length = length;
        }
    }

    internal class RUtil {
        public static int RoundUpto16(int val) {
            return (val + 15) & (~15);
        }
    }

    internal class UtilAxBoneReader {
        public static AxBone Read(BinaryReader br) {
            var o = new AxBone {
                cur = br.ReadInt32(),
                parent = br.ReadInt32(),
                v08 = br.ReadInt32(),
                v0c = br.ReadInt32(),
                x1 = br.ReadSingle(),
                y1 = br.ReadSingle(),
                z1 = br.ReadSingle(),
                w1 = br.ReadSingle(),
                x2 = br.ReadSingle(),
                y2 = br.ReadSingle(),
                z2 = br.ReadSingle(),
                w2 = br.ReadSingle(),
                x3 = br.ReadSingle(),
                y3 = br.ReadSingle(),
                z3 = br.ReadSingle(),
                w3 = br.ReadSingle()
            };
            return o;
        }
    }

    public class Mdlxfst {
        public List<T31> t31s = new();

        public Mdlxfst(Stream fs) {
            var br = new BinaryReader(fs);
            var aler = new Queue<int>();
            aler.Enqueue(0x90);
            var postbl3 = 0;
            while (aler.Count != 0) {
                var eroff = aler.Dequeue();
                fs.Position = eroff + 0x10;
                int cnt2 = br.ReadUInt16();
                fs.Position = eroff + 0x1C;
                int cnt1 = br.ReadUInt16();

                T31 t31;
                t31s.Add(t31 = new T31(eroff, 0x20 * (1 + cnt1), postbl3)); postbl3++;

                for (var c1 = 0; c1 < cnt1; c1++) {
                    fs.Position = eroff + 0x20u + 0x20u * c1 + 0x10u;
                    var pos1 = br.ReadInt32() + eroff;
                    var pos2 = br.ReadInt32() + eroff;
                    fs.Position = eroff + 0x20 + 0x20 * c1 + 0x04;
                    var texi = br.ReadInt32();
                    fs.Position = pos2;
                    var cnt1a = br.ReadInt32();
                    t31.al11.Add(new T11(pos2, RUtil.RoundUpto16(4 + 4 * cnt1a), c1));
                    var alv1 = new List<int>(cnt1a);
                    for (int c1a = 0; c1a < cnt1a; c1a++) alv1.Add(br.ReadInt32());

                    var aloffDMAtag = new List<int>();
                    var alaxi = new List<int[]>();
                    var alaxref = new List<int>();
                    aloffDMAtag.Add(pos1);
                    fs.Position = pos1 + 16;
                    for (var c1a = 0; c1a < cnt1a; c1a++) {
                        if (alv1[c1a] == -1) {
                            aloffDMAtag.Add((int)fs.Position + 0x10);
                            fs.Position += 0x20;
                        }
                        else {
                            fs.Position += 0x10;
                        }
                    }
                    for (var c1a = 0; c1a < cnt1a; c1a++) {
                        if (c1a + 1 == cnt1a) {
                            alaxref.Add(alv1[c1a]);
                            alaxi.Add(alaxref.ToArray()); alaxref.Clear();
                        }
                        else if (alv1[c1a] == -1) {
                            alaxi.Add(alaxref.ToArray()); alaxref.Clear();
                        }
                        else {
                            alaxref.Add(alv1[c1a]);
                        }
                    }

                    var pos1a = (int)fs.Position;
                    t31.al12.Add(new T12(pos1, pos1a - pos1, c1));

                    var tpos = 0;
                    foreach (int offDMAtag in aloffDMAtag) {
                        fs.Position = offDMAtag;
                        // Source Chain Tag
                        var qwcsrc = (br.ReadInt32() & 0xFFFF);
                        var offsrc = (br.ReadInt32() & 0x7FFFFFFF) + eroff;

                        fs.Position = offsrc;
                        var bin = br.ReadBytes(16 * qwcsrc);
                        T13vif t13;
                        t31.al13.Add(t13 = new T13vif(offsrc, 16 * qwcsrc, texi, alaxi[tpos], bin)); tpos++;
                    }
                }

                fs.Position = eroff + 0x14;
                var off2 = br.ReadInt32();
                if (off2 != 0) {
                    off2 += eroff;
                    var len2 = 0x40 * cnt2;
                    t31.t21 = new T21(off2, len2);

                    // matrices used for skinning
                    t31.skeleton = new Matrix4x4[cnt2];
                    t31.skeletonInverse = new Matrix4x4[cnt2];

                    fs.Position = off2;
                    for (var x = 0; x < cnt2/*len2 / 0x40*/; x++)
                    {
                        var b = UtilAxBoneReader.Read(br);

                        // get base transform matrix
                        t31.skeleton[x] = b.BoneMatrix;

                        // update hierarchy transform
                        if (b.parent >= 0)
                        {
                            t31.skeleton[x] *= t31.skeleton[b.parent];
                        }
                        // calculate inverse transform
                        t31.skeletonInverse[x] = t31.skeleton[x].inverse;

                        t31.t21.axBones.Add(b);
                    }
                }

                fs.Position = eroff + 0x18;
                int off4 = br.ReadInt32();
                if (off4 != 0) {
                    off4 += eroff;
                    int len4 = off2 - off4;
                    t31.t32 = new T32(off4, len4);
                }

                fs.Position = eroff + 0xC;
                int off3 = br.ReadInt32();
                if (off3 != 0) {
                    off3 += eroff;
                    aler.Enqueue(off3);
                }
            }
        }
    }
}
