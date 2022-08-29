using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using ef1Declib;
using OpenKh.Unity.MdlxMset.Model;
using OpenKh.Unity.MdlxMset.Motion;
using OpenKh.Unity.MdlxMset.Binary;
using OpenKh.Unity.MdlxMset.Texture;
using OpenKh.Unity.MdlxMset.Utils;
using OpenKh.Unity.Aset.Interfaces;
using OpenKh.Unity.Aset.IO;
using Mesh = OpenKh.Unity.MdlxMset.Model.Mesh;

namespace OpenKh.Unity.Aset
{
    public class MdlxConvert : IControllerBindable, IDisposable {

        #region Fields
        private bool disposed;
        #endregion

        #region Properties
        public string MdlxPath { get; private set; } = string.Empty;
        public string MsetPath { get; private set; } = string.Empty;
        public bool IsLoaded => !(string.IsNullOrEmpty(MdlxPath) || string.IsNullOrEmpty(MsetPath));

        /// <summary>
        /// Currently loaded motions. By default, these are loaded from the MSET file corresponding to the loaded MDLX file. However, it is possible to bind the motions of another model. For reference, check the IControllerBindable interface.
        /// </summary>
        private List<MotionInformation> Motions { get; } = new List<MotionInformation>();
        /// <summary>
        /// Mesh array of the currently loaded model
        /// </summary>
        private Mesh[] Model { get; } = new Mesh[] { new Mesh(), new Mesh(), new Mesh(), };
        #endregion

        #region Flags
        [Flags]
        private enum UpdateFlags : uint {
            None = 0x00,
            Body = 0x01,
            Transforms = 0x02,
            Motion = 0x04,
            Indices = 0x08,
            Vertices = 0x10,

            Buffers = Indices | Vertices,
            Base = Body | Transforms | Motion | Buffers,
            Animate = Motion | Vertices,
        }
        #endregion


        private MdlxConvert() {
            Model[1].parent = Model[0];
            Model[1].matrixIndex = 0xB2;
            Model[2].parent = Model[0];
            Model[2].matrixIndex = 0x56;

            //  TODO: Remove if possible
            ResetDevice();
        }

        #region ILoadf

        public void LoadOf(int x, string fp) {
            switch (x) {
                case 0: LoadMdlx(fp, 0); break;
                case 1: LoadMset(fp, 0); break;

                case 2: LoadMdlx(fp, 1); break;
                case 3: LoadMset(fp, 1); break;

                case 4: LoadMdlx(fp, 2); break;
                case 5: LoadMset(fp, 2); break;

                default: throw new NotSupportedException();
            }
        }

        public void SetJointOf(int x, int joint) {
            switch (x) {
                case 1: Model[1].matrixIndex = joint; break;
                case 2: Model[2].matrixIndex = joint; break;

                default: throw new NotSupportedException();
            }
        }

        #endregion

        #region IDisposable

        // Implement IDisposable.
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // Check to see if Dispose has already been called.
            if (disposed)
                return;

            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing) {
                // Dispose managed resources.
                foreach (var m in Model) {
                    m?.Dispose();
                }
            }

            //  Clean up unmanaged resources
            //CloseHandle(_handle);
            disposed = true;
        }

        // Clean up the unmanaged resource by using interop services.
        [System.Runtime.InteropServices.DllImport("Kernel32")]
        private static extern bool CloseHandle(IntPtr handle);

        //  Finalizer
        ~MdlxConvert() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Load the specified MDLX file
        /// </summary>
        /// <param name="fmdlx">Path of the MDLX file to load</param>
        /// <param name="ty">Internal id/index to load the model at. 0 is the main/default model.</param>
        private void LoadMdlx(string fmdlx, int ty = 0) {
            if (ty == 0) {
                Motions.Clear();
            }

            var mesh = Model[ty];
            mesh.DisposeMdlx();

            ReadBar.BarEntry[] entries;
            using (var fs = File.OpenRead(fmdlx)) {
                entries = ReadBar.Explode(fs);

                foreach (var ent in entries) {
                    if (ent.binaryData is null)
                        continue;

                    switch (ent.k) {
                        case 7:
                            mesh.timc = TIMCollection.Load(new MemoryStream(ent.binaryData, false));
                            if (mesh.timc != null)
                                mesh.timf = mesh.timc.Length >= 1 ? mesh.timc[0] : null;
                            break;
                        case 4:
                            mesh.mdlx = new Mdlxfst(new MemoryStream(ent.binaryData, false));
                            break;
                    }
                }
            }
            mesh.binMdlx = File.ReadAllBytes(fmdlx);
            mesh.mlink = null;
            MdlxPath = fmdlx;

            //  TODO: Remove if possible
            ReloadTextures(ty);
            CalcBody(mesh.caseTris, mesh, null, 0, UpdateFlags.Base);
        }
        private void LoadMset(string fmset, int ty = 0) {
            var mesh = Model[ty];
            mesh.DisposeMset();

            if (File.Exists(fmset)) {
                using (FileStream fs = File.OpenRead(fmset)) {
                    mesh.mset = new Msetfst(fs, Path.GetFileName(fmset));

                    //Msetblk MB = new Msetblk(new MemoryStream(mset.al1[0].bin, false));
                    //Console.WriteLine();
                }

                if (ty == 0) {
                    foreach (var mt1 in mesh.mset.al1) {
                        var mi = new MotionInformation() {
                            motion = mt1
                        };

                        if (mt1.isRaw && mt1.binaryData != null) {
                            var blk = new MsetRawblk(new MemoryStream(mt1.binaryData, false));
                            mi.maxtick = blk.frameCount;
                            mi.mintick = 0;
                        }
                        else if (mt1.binaryData != null) {
                            var blk = new Msetblk(new MemoryStream(mt1.binaryData, false));
                            mi.maxtick = (blk.to.al11 != null && blk.to.al11.Length != 0) ? blk.to.al11[^1] : 0;
                            mi.mintick = (blk.to.al11 != null && blk.to.al11.Length != 0) ? blk.to.al11[0] : 0;
                        }

                        Motions.Add(mi);
                    }
                }
                mesh.binMset = File.ReadAllBytes(fmset);
                MsetPath = fmset;
            }
            mesh.mlink = null;
        }

        private void CalcBody(CaseTris ct, Mesh mesh, SingleMotion motion, float _tick, UpdateFlags flags) {
            if (mesh is null || motion is null || mesh.mdlx is null || mesh.mset is null)
                return;

            var mdlx = mesh.mdlx;
            var albody1 = mesh.bodies;
            var ol = mesh.mlink;

            if ((flags & UpdateFlags.Body) != UpdateFlags.None) {
                //ct.Close();
                albody1.Clear();
            }

            if (mdlx == null)
            {
                return;
            }

            var t31 = mdlx.t31s[0];
            var Ma = t31.skeleton;
            var Minv = t31.skeletonInverse;

            if (motion.binaryData != null &&
                ((flags & UpdateFlags.Motion) != UpdateFlags.None)) {

                if (motion.isRaw) {
                    var blk = new MsetRawblk(new MemoryStream(motion.binaryData, false));
                    var t0 = Math.Max(0, Math.Min(blk.frameCount - 1, (int)Math.Floor(_tick)));
                    var t1 = Math.Max(0, Math.Min(blk.frameCount - 1, (int)Math.Ceiling(_tick)));

                    if (t0 == t1) {
                        var rm = blk.rms[t0];
                        Ma = mesh.matrices = rm.matrices.ToArray();
                    }
                    else {
                        var rm0 = blk.rms[t0];
                        var f1 = _tick % 1.0f;
                        var rm1 = blk.rms[t1];
                        var f0 = 1.0f - f1;
                        Ma = mesh.matrices = new Matrix4x4[blk.boneCount];

                        for (var t = 0; t < Ma.Length; t++) {
                            var matrix = new Matrix4x4 {
                                m00 = rm0.matrices[t].m00 * f0 + rm1.matrices[t].m00 * f1,
                                m10 = rm0.matrices[t].m10 * f0 + rm1.matrices[t].m10 * f1,
                                m20 = rm0.matrices[t].m20 * f0 + rm1.matrices[t].m20 * f1,
                                m30 = rm0.matrices[t].m30 * f0 + rm1.matrices[t].m30 * f1,
                                m01 = rm0.matrices[t].m01 * f0 + rm1.matrices[t].m01 * f1,
                                m11 = rm0.matrices[t].m11 * f0 + rm1.matrices[t].m11 * f1,
                                m21 = rm0.matrices[t].m21 * f0 + rm1.matrices[t].m21 * f1,
                                m31 = rm0.matrices[t].m31 * f0 + rm1.matrices[t].m31 * f1,
                                m02 = rm0.matrices[t].m02 * f0 + rm1.matrices[t].m02 * f1,
                                m12 = rm0.matrices[t].m12 * f0 + rm1.matrices[t].m12 * f1,
                                m22 = rm0.matrices[t].m22 * f0 + rm1.matrices[t].m22 * f1,
                                m32 = rm0.matrices[t].m32 * f0 + rm1.matrices[t].m32 * f1,
                                m03 = rm0.matrices[t].m03 * f0 + rm1.matrices[t].m03 * f1,
                                m13 = rm0.matrices[t].m13 * f0 + rm1.matrices[t].m13 * f1,
                                m23 = rm0.matrices[t].m23 * f0 + rm1.matrices[t].m23 * f1,
                                m33 = rm0.matrices[t].m33 * f0 + rm1.matrices[t].m33 * f1
                            };
                            Ma[t] = matrix;
                        }
                    }
                }
                else {
                    var blk = new Msetblk(new MemoryStream(motion.binaryData, false));
                    var os = new MemoryStream();
                    ol ??= mesh.mlink = new Mlink();

                    if (mesh.binMdlx != null && mesh.binMset != null)
                        ol.Permit(new MemoryStream(mesh.binMdlx, false), blk.cntb1, new MemoryStream(mesh.binMset, false), blk.cntb2, motion.offset, _tick, os);

                    var br = new BinaryReader(os);
                    os.Position = 0;
                    Ma = mesh.matrices = new Matrix4x4[blk.cntb1];

                    for (var t = 0; t < blk.cntb1; t++) {
                        var matrix = new Matrix4x4 {
                            m00 = br.ReadSingle(),
                            m01 = br.ReadSingle(),
                            m02 = br.ReadSingle(),
                            m03 = br.ReadSingle(),
                            m10 = br.ReadSingle(),
                            m11 = br.ReadSingle(),
                            m12 = br.ReadSingle(),
                            m13 = br.ReadSingle(),
                            m20 = br.ReadSingle(),
                            m21 = br.ReadSingle(),
                            m22 = br.ReadSingle(),
                            m23 = br.ReadSingle(),
                            m30 = br.ReadSingle(),
                            m31 = br.ReadSingle(),
                            m32 = br.ReadSingle(),
                            m33 = br.ReadSingle()
                        };
                        Ma[t] = matrix;
                    }
                }
            }

            if (Ma != null && Minv != null && (flags & UpdateFlags.Transforms) != UpdateFlags.None) {
                var cnt = Ma.Length;
                for (var mn = 0; mn < cnt; ++mn) {
                    Minv[mn] = Ma[mn].inverse;
                }
            }

            var Mv = Matrix4x4.identity;
            if (mesh.parent != null && mesh.matrixIndex != -1 && mesh.parent.matrices != null) {
                Mv = mesh.parent.matrices[mesh.matrixIndex];
            }

            if (Ma != null && (flags & UpdateFlags.Body) != UpdateFlags.None) {
                foreach (T13vif t13 in t31.al13) {
                    var tops = 0x220;
                    var top2 = 0;
                    var vu1mem = new VU1Mem();
                    new ParseVIF1(vu1mem).Parse(new MemoryStream(t13.bin, false), tops);
                    var body1 = SimaVU1.Sima(vu1mem, Ma, tops, top2, t13.texi, t13.alaxi, Mv);
                    albody1.Add(body1);
                }
            }

            if ((flags & UpdateFlags.Indices) != UpdateFlags.None) {
                //if (ct.sepas == null || ct.tris == null)
                {
                    var altri3 = new List<uint>();
                    var alsepa = new List<Sepa>();

                    {
                        var svi = 0;
                        var bi = 0;
                        var alvi = new uint[4];
                        var ai = 0;
                        var il = (int)_tick;
                        var ord = new[] { 1, 2, 3 };

                        foreach (var b1 in albody1) {
                            if (b1.alvi is null || b1.alfl is null)
                                continue;

                            var cntPrim = 0;
                            for (var x = 0; x < b1.alvi.Length; x++) {
                                alvi[ai] = (uint)((b1.alvi[x]) | (bi << 12) | (x << 24));
                                ai = (ai + 1) & 3;
                                if (b1.alfl[x] == 0x20) {
                                    altri3.Add(alvi[(ai - ord[(0 + (il * 103)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(1 + (il * 103)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(2 + (il * 103)) % 3]) & 3]);
                                    cntPrim++;
                                }
                                else if (b1.alfl[x] == 0x30) {
                                    altri3.Add(alvi[(ai - ord[(0 + (il << 1)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(2 + (il << 1)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(1 + (il << 1)) % 3]) & 3]);
                                    cntPrim++;
                                }
                                else if (b1.alfl[x] == 0x00) // double sided
                                {
                                    altri3.Add(alvi[(ai - ord[(0 + (il * 103)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(1 + (il * 103)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(2 + (il * 103)) % 3]) & 3]);
                                    cntPrim++;
                                    altri3.Add(alvi[(ai - ord[(0 + (il << 1)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(2 + (il << 1)) % 3]) & 3]);
                                    altri3.Add(alvi[(ai - ord[(1 + (il << 1)) % 3]) & 3]);
                                    cntPrim++;
                                }
                            }
                            alsepa.Add(new Sepa(svi, cntPrim, b1.t, bi));
                            svi += 3 * cntPrim;
                            bi++;
                        }
                    }

                    ct.sepas = alsepa.ToArray();
                    ct.tris = altri3.ToArray();
                }
            }

            if ((flags & UpdateFlags.Vertices) != UpdateFlags.None) {
                ct.cntVert = ct.tris?.Length ?? 0;
                ct.cntPrimitives = 0;

                if (ct.cntVert != 0 && ct.tris != null) {

                    var vertexCount = ct.tris.Length;
                    var va = new PTex3[vertexCount];

                    for (var x = 0; x < vertexCount; ++x) {
                        var xx = ct.tris[x];
                        var vi = xx & 4095;
                        var bi = (xx >> 12) & 4095;
                        var xi = (xx >> 24) & 4095;
                        var b1 = albody1[(int)bi];

                        var tm = new Matrix4x4();

                        if (b1.vertices is null || b1.boneIndices is null || b1.boneWeights is null || b1.uv is null || Minv is null || Ma is null)
                            continue;

                        var v3 = b1.vertices[vi];

                        var weightCount = b1.boneIndices[vi].Length;

                        for (var wn = 0; wn < weightCount; ++wn)
                        {
                            tm =
                                MatrixUtil.Sum(
                                    tm,
                                    MatrixUtil.Scale(
                                        Minv[b1.boneIndices[vi][wn]] * Ma[b1.boneIndices[vi][wn]],
                                        b1.boneWeights[vi][wn])
                                );
                        }

                        v3 = tm.MultiplyPoint(v3);
                        va[x] = new PTex3(v3, new Vector2(b1.uv[xi].x, b1.uv[xi].y));
                    }

                    ct.vb ??= new GraphicsBuffer(
                        GraphicsBuffer.Target.Vertex,
                        ct.cntVert,
                        PTex3.Size
                    );

                    ct.vb.SetData(va);
                }
            }
        }

        private void ResetDevice() {
            ReloadTextures(-1);
        }
        private void ReloadTextures(int ty) {
            var x = 0;

            foreach (var mesh in Model) {

                if (x == ty || ty == -1) {
                    mesh.textures.Clear();
                    mesh.textures_1.Clear();
                    mesh.textures_2.Clear();

                    if (mesh.timf != null) {
                        //int t = 0;
                        foreach (var st in mesh.timf.bitmapList) {
                            mesh.textures.Add(st.bitmap.ToTexture2D());
                            //st.pic.Save(@"H:\Proj\khkh_xldM\MEMO\pattex\t" + ty + "." + t + ".png", ImageFormat.Png); t++;
                        }

                        if (x == 0) {

                            for (var p = 0; p < 2; p++) {
                                for (var pi = 0; ; pi++) {
                                    var pic = mesh.timf.GetPatchBitmap(p, pi);
                                    if (pic == null)
                                        break;
                                    //pic.Save(@"H:\Proj\khkh_xldM\MEMO\pattex\p" + p + "." + pi + ".png", ImageFormat.Png);
                                    switch (p) {
                                        case 0: mesh.textures_1.Add(pic.ToTexture2D()); break;
                                        case 1: mesh.textures_2.Add(pic.ToTexture2D()); break;
                                    }
                                }
                            }
                        }
                    }
                }
                x++;
            }
            
        }

        /// <summary>
        /// Export an ASET file of the currently loaded MDLX/MSET combo
        /// </summary>
        /// <param name="progress">Event handler for progress updates</param>
        /// <returns>True if the export has been successful</returns>
        private bool ExportASET(ExportProgress progress, out string outFilePath) {
            var status = new ExportStatus
            {
                fileName = Path.GetFileNameWithoutExtension(MdlxPath),
            };

            outFilePath = string.Empty;

            progress.Update(ExportState.Initialization, status);

            if (progress.CancellationPending)
            {
                Debug.Log("ASET export cancelled by user.");
                return false;
            }

            // get main mesh
            var m = Model[0];

            if (m?.mdlx == null || m.mset == null || Motions.Count <= 0)
            {
                Debug.LogWarning("ASET export failed: Mesh could not be loaded.");
                return false;
            }

            var t = m.mdlx.t31s[0];

            // Create a new memory stream
            var mat_data = new MemoryStream();
            var mat_writer = new BinaryWriter(mat_data);
            var bone_count = t.t21 != null ? t.t21.axBones.Count : 0;
            var anim_count = Motions.Count;

            status.animCount = anim_count;
            status.jointCount = bone_count;
            _ = mat_writer.BaseStream.Position;

            mat_writer.Write(new char[] { 'A', 'S', 'E', 'T' }); // bone count (4 bytes)
            mat_writer.Write(0); // padding (4 bytes)
            mat_writer.Write(bone_count); // bone count (4 bytes)
            mat_writer.Write(anim_count); // animation count (4 bytes)

            // get start of animation offset array
            var offsets_start = mat_writer.BaseStream.Position;

            // move past animation offset array
            mat_writer.BaseStream.Position += (anim_count * 4 + 0x0F) & ~0x0F;

            progress.Update(ExportState.Initialization, status);

            if (progress.CancellationPending) {
                return false;
            }

            var anim_num = 0;
            // Get the frame count (ticks)
            foreach (var motion in Motions) {
                if (motion.motion == null)
                    continue;

                var anim_name = motion.motion.id;

                status.animName = anim_name;
                status.animIndex = anim_num;

                anim_name = anim_name.Replace('#', '_');

                // get motion info
                var max_ticks = (int)motion.maxtick;

                status.frameIndex = 0;
                status.frameCount = max_ticks;

                progress.Update(ExportState.Processing, status);

                if (progress.CancellationPending) {
                    return false;
                }

                // animation data position
                var anim_start = mat_writer.BaseStream.Position;

                // data offset array position
                mat_writer.BaseStream.Position = offsets_start + (anim_num * 4);

                // write animation data offset
                mat_writer.Write((int)anim_start); // bone count (4 bytes)

                // reset animation data position
                mat_writer.BaseStream.Position = anim_start;

                // write animation header
                mat_writer.Write(anim_num); // animation index (4 bytes)
                mat_writer.Write(max_ticks); // frame (tick) count (4 bytes)
                mat_writer.Write(0); // padding (4 bytes)
                mat_writer.Write(0); // padding (4 bytes)

                // write animation name
                mat_writer.Write(anim_name.ToCharArray(0, anim_name.Length));

                // move to matrix data start
                mat_writer.BaseStream.Position += (32 - anim_name.Length);

                // increment animation index
                ++anim_num;

                // output all the matrix transforms for each frame
                for (var i = 0; i < max_ticks; i++) {
                    status.frameIndex = i;
                    status.jointIndex = 0;

                    progress.Update(ExportState.Processing, status);

                    if (progress.CancellationPending) {
                        return false;
                    }

                    //  TODO: Remove if possible -- UPDATE: Port method to Unity
                    CalcBody(m.caseTris, m, motion.motion, i, UpdateFlags.Motion);

                    // output each matrix for each bone (matrix4x4 * 4 bytes)

                    for (var bn = 0; bn < bone_count; ++bn) {
                        status.jointIndex = bn;

                        //worker.ReportProgress((int)ExportState.Processing, progress);

                        if (progress.CancellationPending) {
                            return false;
                        }

                        if (m.matrices is null)
                            continue;

                        var mat = m.matrices[bn];
                        /* Matrix Format
                           [M11][M12][M13][M14]
                           [M21][M22][M23][M24]
                           [M31][M32][M33][M34]
                           [M41][M42][M43][M44]
                        */
                        mat_writer.Write(mat.m00);
                        mat_writer.Write(mat.m01);
                        mat_writer.Write(mat.m02);
                        mat_writer.Write(mat.m03);

                        mat_writer.Write(mat.m10);
                        mat_writer.Write(mat.m11);
                        mat_writer.Write(mat.m12);
                        mat_writer.Write(mat.m13);

                        mat_writer.Write(mat.m20);
                        mat_writer.Write(mat.m21);
                        mat_writer.Write(mat.m22);
                        mat_writer.Write(mat.m23);

                        mat_writer.Write(mat.m30);
                        mat_writer.Write(mat.m31);
                        mat_writer.Write(mat.m32);
                        mat_writer.Write(mat.m33);
                    }

                    progress.Update(ExportState.Processing, status);
                }

                progress.Update(ExportState.Processing, status);
            }

            progress.Update(ExportState.Processing, status);

            // reset animation data position
            mat_writer.BaseStream.Position = 0x0C;
            mat_writer.Write(anim_num);

            if (progress.CancellationPending) {
                return false;
            }

            // Set the output file
            outFilePath = Path.ChangeExtension(MdlxPath, ".ASET");
            // Open the file
            var outfile = File.Open(outFilePath, FileMode.Create, FileAccess.ReadWrite);

            progress.Update(ExportState.Saving, status);

            // Output all the bytes to a file
            var b_data = mat_data.ToArray();

            outfile.Write(b_data, 0, b_data.Length);

            outfile.Close();

            progress.Update(ExportState.Finished, status);
            //MessageBox.Show("Animation transforms dumped.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            return true;
        }

        #endregion

        #region Static methods
        /// <summary>
        /// Converts the MDLX file at the specified path to an ASET file using the current process' handle. The method requires a corresponding MSET file in the same directory as the MDLX file.
        /// </summary>
        /// <param name="mdlxPath">The path of the MDLX file to convert</param>
        /// <param name="onProgress">Callback function for the export progress. The function is called at several states of the export process containing the current state/status information.</param>
        /// <param name="outFile">Path of the exported file</param>
        /// <returns>True if the conversion has been successful</returns>
        public static bool ToAset(string mdlxPath, Action<ExportState, ExportStatus> onProgress, out string outFile) {
            if (!Directory.Exists(Path.GetDirectoryName(mdlxPath)) ||
                !Path.GetExtension(mdlxPath).ToLower().Equals(".mdlx") ||
                !File.Exists(mdlxPath) ||
                !MdlxMatch.HasMset(mdlxPath))
            {
                outFile = string.Empty;
                return false;
            }

            //  Initialize converter
            using var converter = new MdlxConvert();
            converter.LoadMdlx(mdlxPath);
            converter.LoadMset(MdlxMatch.FindMset(mdlxPath));

            //  Export ASET file
            var progress = new ExportProgress();
            progress.OnProgress += onProgress;

            return converter.ExportASET(progress, out outFile);
        }

        #endregion
    }
}
