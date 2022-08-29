using Unity.Collections;
using Unity.Jobs;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using System.IO;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public struct ExtractJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> QueueIds;
        public NativeArray<bool> Result;

        public void Execute(int i)
        {
            if (ExtractQueue.Active.Count <= QueueIds[i])
            {
                Result[i] = false;
                return;
            }

            var fvm = ExtractQueue.Active[QueueIds[i]];

            Result[i] = ExtractAsset(fvm, out _);
        }


        /// <summary>
        /// Extract an asset file contained in an IDX/IMG file pair
        /// </summary>
        /// <param name="fvm">View model of the asset file to extract</param>
        /// <param name="outFile">Path of the extracted file</param>
        /// <returns>True if extraction has been successful</returns>
        private bool ExtractAsset(FileViewModel fvm, out string outFile)
        {
            //Debug.Log($"Extracting asset '{fvm.Name}'..");

            //  Retrieve file stream
            if (!AssetImporter.ActiveImg.TryFileOpen(fvm.Entry.GetFullName(), out var stream))
            {
                //Debug.LogWarning($"Could not import '{fvm.Entry.GetFullName()}'");
                outFile = null;
                return false;
            }

            //  Write file to temp directory
            outFile = Path.Combine(PackageInfo.TempDir, $"{Path.GetFileNameWithoutExtension(AssetImporter.ActiveIdxPath)}/{fvm.Entry.GetFullName()}");
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
    }
}
