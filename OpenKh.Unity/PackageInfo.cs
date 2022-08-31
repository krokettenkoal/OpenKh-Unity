using System.IO;
using UnityEngine;

namespace OpenKh.Unity
{
    public static class PackageInfo
    {
        private static string TempCacheDir = Path.GetFullPath(Application.temporaryCachePath);
        private static string AssetDir = Path.GetFullPath(Application.dataPath);
        public static string PackageRoot => Path.Combine(AssetDir, "OpenKh");
        public static string TempDir => Path.Combine(TempCacheDir, "OpenKh");
        public static string AssetImportDir => Path.Combine(AssetDir, "OpenKh Imported Assets");
    }
}
