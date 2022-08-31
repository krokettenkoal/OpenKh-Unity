using System;
using System.IO;
using OpenKh.Unity.Exporter.Aset.Binary;
using UnityEngine;

namespace OpenKh.Unity.Exporter.Aset.Model {
    public class SimaVU1 {
        public static Body Sima(VU1Mem vu1mem, Matrix4x4[] Ma, int tops, int top2, int tsel, int[] alaxi, Matrix4x4 Mv) {
            var si = new MemoryStream(vu1mem.vumem, true);
            var br = new BinaryReader(si);

            //si.Position = 16 * (top2);
            //for (int x = 0; x < alaxi.Length; x++) {
            //    UtilMatrixio.write(wr, matrix[alaxi[x]]);
            //}

            si.Position = 16 * (tops);

            var v00 = br.ReadInt32();
            if (v00 != 1 && v00 != 2) throw new ProtInvalidTypeException();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            _ = br.ReadInt32();
            var v10 = br.ReadInt32(); // cnt box2
            var v14 = br.ReadInt32(); // off box2 {tx ty vi fl}
            var v18 = br.ReadInt32(); // off box1
            _ = br.ReadInt32(); // off matrices
            _ = (v00 == 1) ? br.ReadInt32() : 0; // cntvertscolor
            _ = (v00 == 1) ? br.ReadInt32() : 0; // offvertscolor
            var v28 = (v00 == 1) ? br.ReadInt32() : 0; // cnt spec
            var v2c = (v00 == 1) ? br.ReadInt32() : 0; // off spec
            var v30 = br.ReadInt32(); // cnt verts 
            var v34 = br.ReadInt32(); // off verts
            var v38 = br.ReadInt32(); // off spec indices
            var v3c = br.ReadInt32(); // cnt box1
            //Trace.Assert(v1c == top2 - tops, "top2 isn't identical!");

            si.Position = 16 * (tops + v18);
            var box1 = new int[v3c];
            for (var x = 0; x < box1.Length; x++) {
                box1[x] = br.ReadInt32();
            }

            var body1 = new Body {
                t = tsel,
                vertices = new Vector3[v30],
                avail = (v28 == 0) && (v00 == 1)
            };

            var alv4 = new Vector3[v30];

            // joint weight and indices storage
            body1.boneIndices = new int[v30][];
            body1.boneWeights = new float[v30][];

            var vi = 0;
            si.Position = 16 * (tops + v34);
            for (var x = 0; x < box1.Length; x++) {
                var M1 = Ma[alaxi[x]] * Mv;
                var ct = box1[x];

                for (var t = 0; t < ct; t++, vi++) {
                    var fx = br.ReadSingle();
                    var fy = br.ReadSingle();
                    var fz = br.ReadSingle();
                    var fw = br.ReadSingle();
                    var v3 = new Vector3(fx, fy, fz);
                    var v3t = M1.MultiplyPoint(v3);
                    body1.vertices[vi] = v3t;
                    var v4 = new Vector4(fx, fy, fz, fw);
                    var v4t = M1 * v4;
                    alv4[vi] = new Vector3(v4t.x, v4t.y, v4t.z);

                    body1.boneIndices[vi] = new[] { alaxi[x] };
                    body1.boneWeights[vi] = new[] { fw };
                }
            }

            body1.uv = new Vector2[v10];
            body1.alvi = new int[v10];
            body1.alfl = new int[v10];

            si.Position = 16 * (tops + v14);
            for (var x = 0; x < v10; x++) {
                var tx = br.ReadUInt16() / 16; br.ReadUInt16();
                var ty = br.ReadUInt16() / 16; br.ReadUInt16();
                body1.uv[x] = new Vector2(tx / 256.0f, ty / 256.0f);
                body1.alvi[x] = br.ReadUInt16(); br.ReadUInt16();
                body1.alfl[x] = br.ReadUInt16(); br.ReadUInt16();
            }

            if (v28 == 0)
            {
                return body1;
            }

            var allocalvert = new Vector3[v30];
            var allocalbi = new int[v30][];
            var allocalbw = new float[v30][];
            var xi = 0;
            long cpos = 16 * (tops + v2c); // off spec
            long ipos = 16 * (tops + v38); // off spec idx

            // NOTE: Allows for arbitrary number of weights
            for (var weightLimit = 0; weightLimit < v28; ++weightLimit) {
                si.Position = cpos;
                var vt0 = br.ReadInt32(); // number of indices
                cpos = si.Position;

                si.Position = ipos;
                while (vt0 > 0) {
                    allocalvert[xi] = new Vector3();

                    // allocate space for new weight list size
                    allocalbi[xi] = new int[weightLimit + 1];
                    allocalbw[xi] = new float[weightLimit + 1];

                    for (var wi = 0; wi < (weightLimit + 1); ++wi) {
                        var ai = br.ReadInt32();

                        allocalvert[xi] += alv4[ai];

                        allocalbi[xi][wi] = body1.boneIndices[ai][0];
                        allocalbw[xi][wi] = body1.boneWeights[ai][0];
                    }

                    --vt0;
                    ++xi;
                }
                ipos = (si.Position + 15) & (~15);
            }

            #region Old code fragments

            /*
            //                 si.Position = 16 * (tops + v2c);
            //                 int vt0 = br.ReadInt32();
            //                 int vt1 = br.ReadInt32();
            //                 int vt2 = br.ReadInt32();
            //                 int vt3 = br.ReadInt32();
            //                 int vt4 = 0;
            //                 if (v28 >= 5)
            //                 {
            //                     vt4 = br.ReadInt32();
            //                     br.ReadInt32();
            //                     br.ReadInt32();
            //                     br.ReadInt32();
            //                 }
            //                 Vector3[] allocalvert = new Vector3[v30];
            //                 int xi = 0;
            //                 for (xi = 0; xi < vt0; xi++)
            //                 {
            //                     int ai = br.ReadInt32();
            //                     allocalvert[xi] = body1.vertices[ai];
            //                 }
            //                 if (v28 >= 2)
            //                 {
            //                     si.Position = (si.Position + 15) & (~15);
            //                     for (int x = 0; x < vt1; x++, xi++)
            //                     {
            //                         int i0 = br.ReadInt32();
            //                         int i1 = br.ReadInt32();
            //                         allocalvert[xi] = alv4[i0] + alv4[i1];
            //                     }
            //                 }
            //                 if (v28 >= 3)
            //                 {
            //                     si.Position = (si.Position + 15) & (~15);
            //                     for (int x = 0; x < vt2; x++, xi++)
            //                     {
            //                         int i0 = br.ReadInt32();
            //                         int i1 = br.ReadInt32();
            //                         int i2 = br.ReadInt32();
            //                         allocalvert[xi] = alv4[i0] + alv4[i1] + alv4[i2];
            //                     }
            //                 }
            //                 if (v28 >= 4)
            //                 {
            //                     si.Position = (si.Position + 15) & (~15);
            //                     for (int x = 0; x < vt3; x++, xi++)
            //                     {
            //                         int i0 = br.ReadInt32();
            //                         int i1 = br.ReadInt32();
            //                         int i2 = br.ReadInt32();
            //                         int i3 = br.ReadInt32();
            //                         allocalvert[xi] = alv4[i0] + alv4[i1] + alv4[i2] + alv4[i3];
            //                     }
            //                 }
            //                 if (v28 >= 5)
            //                 {
            //                     si.Position = (si.Position + 15) & (~15);
            //                     for (int x = 0; x < vt4; x++, xi++)
            //                     {
            //                         int i0 = br.ReadInt32();
            //                         int i1 = br.ReadInt32();
            //                         int i2 = br.ReadInt32();
            //                         int i3 = br.ReadInt32();
            //                         int i4 = br.ReadInt32();
            //                         allocalvert[xi] = alv4[i0] + alv4[i1] + alv4[i2] + alv4[i3] + alv4[i4];
            //                     }
            //                 }
            */
            

            #endregion

            body1.vertices = allocalvert;
            body1.boneIndices = allocalbi;
            body1.boneWeights = allocalbw;

            return body1;
        }
    }

    public class Body {
        public Vector3[] vertices;
        public int[][] boneIndices; // joint indices
        public float[][] boneWeights; // joint weights
        public Vector2[] uv;
        public int[] alvi;
        public int[] alfl;
        public int t = -1;
        public bool avail = false;
    }

    public class ProtInvalidTypeException : ApplicationException {
        public ProtInvalidTypeException() : base("Has to be typ1 or typ2") { }
    }
}
