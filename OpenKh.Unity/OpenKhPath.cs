using System;
using System.IO;
using UnityEngine;
using OpenKh.Unity.Settings;

namespace OpenKh.Unity
{
    public static class OpenKhPath
    {
        #region Constants

        private const string _packageName = "OpenKh";
        private const string _defaultAssetImportDirName = _packageName + " Imported Assets";

        #endregion

        #region Base directories

        private static string TempCacheDir = Path.GetFullPath(Application.temporaryCachePath);
        private static string AssetDir = Path.GetFullPath(Application.dataPath);
        private static string ProgramFilesX86 = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

        #endregion

        #region OpenKh directories

        /// <summary>
        /// Path to the base folder of the OpenKh package
        /// </summary>
        public static string PackageRoot => Path.Combine(AssetDir, OpenKhPrefs.GetString("PackageName", _packageName));
        /// <summary>
        /// Path to the base folder of temporary files created by OpenKh
        /// </summary>
        public static string TempDir => Path.Combine(TempCacheDir, OpenKhPrefs.GetString("PackageName", _packageName));
        /// <summary>
        /// Path to the folder where imported assets are saved
        /// </summary>
        public static string AssetImportDir => Path.Combine(AssetDir, OpenKhPrefs.GetString("AssetImportDirName", _defaultAssetImportDirName));
        /// <summary>
        /// Path to the Noesis binary
        /// </summary>
        public static string NoesisBin =>
            Path.GetFullPath(OpenKhPrefs.GetString("NoesisBinPath", @"Noesis\Noesis.exe")
                    .Replace("{ProgramFiles}", ProgramFilesX86, StringComparison.OrdinalIgnoreCase),
                ProgramFilesX86);

        #endregion
    }
}
