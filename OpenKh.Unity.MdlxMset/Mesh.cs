using ef1Declib;
using System.Collections.Generic;
using System;
using OpenKh.Unity.MdlxMset.Motion;
using OpenKh.Unity.MdlxMset.Texture;
using UnityEngine;

namespace OpenKh.Unity.MdlxMset.Model {

    internal class Mesh : IDisposable {
        public Mdlxfst mdlx;
        public Texex2[] timc;
        public Texex2 timf;
        public Msetfst mset;
        public List<Texture2D> textures = new();
        public List<Texture2D> textures_1 = new();
        public List<Texture2D> textures_2 = new();
        public List<Body> bodies = new();
        public byte[] binMdlx;
        public byte[] binMset;
        public CaseTris caseTris = new();
        public Mlink mlink;
        public PatTexSel[] pts = Array.Empty<PatTexSel>();

        public Matrix4x4[] matrices; // for keyblade
        public Mesh parent = null; // for keyblade
        public int matrixIndex = -1; // for keyblade

        public bool Present { get { return mdlx != null && mset != null; } }

        #region IDisposable ƒƒ“ƒo

        public void Dispose() {
            DisposeMdlx();
            DisposeMset();
        }

        #endregion

        public void DisposeMset() {
            mset = null;
            binMset = null;
            mlink = null;
        }

        public void DisposeMdlx() {
            mdlx = null;
            timc = null;
            timf = null;
            foreach (var t in textures)
                UnityEngine.Object.Destroy(t);
            textures.Clear();
            foreach (var t in textures_1)
                UnityEngine.Object.Destroy(t);
            textures_1.Clear();
            foreach (var t in textures_2)
                UnityEngine.Object.Destroy(t);
            textures_2.Clear();
            bodies.Clear();
            binMdlx = null;
            caseTris.Close();
            mlink = null;
            matrices = null;
        }
    }
}
