
using System.IO;
using UnityEngine;

namespace OpenKh.Unity
{
    public static class PackageInfo
    {
        public static string PackageRoot => Path.Combine(Application.dataPath, "OpenKh");
    }
}
