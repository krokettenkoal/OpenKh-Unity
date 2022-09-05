using System;
using System.IO;
using OpenKh.Kh2;

namespace OpenKh.Unity.Tools.IdxImg
{
    [Flags]
    public enum AssetFormat : byte
    {
        Unknown = 0,
        Mdlx = 1 << 0,
        Mset = 1 << 1,
        Anb = 1 << 2,
        Bin = 1 << 3,
        Pax = 1 << 4,
    }

    public static class IdxEntryExtensions
    {
        public static bool IsMdlx(this Idx.Entry entry) =>
            entry.GetAssetFormat() == AssetFormat.Mdlx;
        public static bool IsMset(this Idx.Entry entry) =>
            entry.GetAssetFormat() == AssetFormat.Mset;

        public static AssetFormat GetAssetFormat(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".mdlx" => AssetFormat.Mdlx,
                ".mset" => AssetFormat.Mset,
                ".anb" => AssetFormat.Anb,
                ".bin" => AssetFormat.Bin,
                ".pax" => AssetFormat.Pax,
                _ => AssetFormat.Unknown,
            };
        }
        public static AssetFormat GetAssetFormat(this Idx.Entry entry) => GetAssetFormat(entry.GetFullName());

    }
}
