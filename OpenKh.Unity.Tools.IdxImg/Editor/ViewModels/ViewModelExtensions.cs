using System;
using System.IO;

namespace OpenKh.Unity.Tools.IdxImg.ViewModels
{
    [Flags]
    public enum AssetFormat : byte
    {
        Unknown = 0,
        Mdlx = 1 << 0,
        Mset = 1 << 1,
    }

    public static class ViewModelExtensions
    {
        public static bool IsMdlx(this FileViewModel fvm) =>
            fvm.GetAssetFormat() == AssetFormat.Mdlx;
        public static bool IsMset(this FileViewModel fvm) =>
            fvm.GetAssetFormat() == AssetFormat.Mset;

        public static AssetFormat GetAssetFormat(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".mdlx" => AssetFormat.Mdlx,
                ".mset" => AssetFormat.Mset,
                _ => AssetFormat.Unknown,
            };
        }
        public static AssetFormat GetAssetFormat(this FileViewModel fvm) => GetAssetFormat(fvm.FullName);

    }
}
