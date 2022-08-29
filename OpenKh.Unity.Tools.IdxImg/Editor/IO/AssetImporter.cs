using System;
using System.Collections.Generic;
using OpenKh.Kh2;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenKh.Unity.Tools.AsetExport;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public static class AssetImporter
    {
        public static string ActiveIdxPath { get; set; }
        public static string ActiveImgPath { get; set; }
        public static Img ActiveImg { get; set; }

        public static IEnumerable<string> ExtractedAssets =>
            Directory.GetFiles(PackageInfo.TempDir, "*.*", SearchOption.AllDirectories);

        public static IEnumerable<string> ExportableAssets => ExtractedAssets.Where(IsConvertible);

        private static bool IsConvertible(string filePath) => Path.GetExtension(filePath).ToLowerInvariant() == ".mdlx";


        /// <summary>
        /// Extract an asset file contained in an IDX/IMG file pair
        /// </summary>
        /// <param name="fvm">View model of the asset file to extract</param>
        /// <param name="outFile">Path of the extracted file</param>
        /// <returns>True if extraction has been successful</returns>
        public static bool ExtractAsset(FileViewModel fvm, out string outFile)
        {
            //Debug.Log($"Extracting asset '{fvm.Name}'..");

            //  Retrieve file stream
            if (!ActiveImg.TryFileOpen(fvm.Entry.GetFullName(), out var stream))
            {
                //Debug.LogWarning($"Could not import '{fvm.Entry.GetFullName()}'");
                outFile = null;
                return false;
            }

            //  Write file to temp directory
            outFile = Path.Combine(PackageInfo.TempDir, $"{Path.GetFileNameWithoutExtension(ActiveIdxPath)}/{fvm.Entry.GetFullName()}");
            var destDir = Path.GetDirectoryName(outFile);

            if (destDir == null)
                return false;

            //  Create directory if necessary
            Directory.CreateDirectory(destDir);

            //  Delete existing file
            if (File.Exists(outFile))
                File.Delete(outFile);

            //  Write file contents
            using var fileStream = File.Create(outFile);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);

            return true;
        }
        public static void ExtractAsset(FileViewModel fvm) => ExtractAsset(fvm, out _);
        public static Task RunExtractTask(FileViewModel fvm, CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                ct.ThrowIfCancellationRequested();
                ExtractAsset(fvm);
            }, ct);
        }

        public static Task RunExportTask(string mdlxPath, CancellationToken ct,
            Action<ExportState, ExportStatus> onProgress) =>
            Task.Factory.StartNew(() =>
            {
                ct.ThrowIfCancellationRequested();
                MdlxConvert.ToAset(mdlxPath, onProgress, out _);
            }, ct);
    }
}
