
using System.IO;
using UnityEngine;

namespace OpenKh.Unity
{
    public static class PackageInfo
    {
        public static string PackageRoot => Path.Combine(Application.dataPath, "OpenKh");
        public static string TempDir => Path.Combine(Application.dataPath, "OpenKh_tmp");
        public static string AssetImportDir => Path.Combine(Application.dataPath, "OpenKh Import");
    }
}
