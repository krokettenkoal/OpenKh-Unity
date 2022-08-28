using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;

namespace OpenKh.Unity.MdlxMset.Texture {
    internal static class TextureExtensions {
        public static Texture2D ToTexture2D(this Bitmap p) {
            var stream = new MemoryStream();
            p.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            var tex = new Texture2D(p.Width, p.Height);

            tex.LoadRawTextureData(stream.ToArray());
            tex.Apply();

            return tex;
        }
    }
}
