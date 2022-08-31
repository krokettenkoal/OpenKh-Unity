using System;
using UnityEngine;

namespace OpenKh.Unity.Exporter.Aset.Model {
    internal class CaseTris : IDisposable {
        public GraphicsBuffer vb;
        public int cntPrimitives, cntVert;
        public Sepa[] sepas;
        public uint[] tris;

        #region IDisposable ƒƒ“ƒo

        public void Dispose() {
            Close();
        }

        #endregion

        public void Close() {
            vb?.Release();
            vb = null;
            cntPrimitives = 0;
            cntVert = 0;
            sepas = null;
            tris = null;
        }

    }
}
