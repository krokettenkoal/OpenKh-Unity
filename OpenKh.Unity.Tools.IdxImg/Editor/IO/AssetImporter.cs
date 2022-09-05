using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OpenKh.Kh2;
using OpenKh.Unity.Exporter;
using OpenKh.Unity.Exporter.Progress;
using OpenKh.Unity.Tools.IdxImg.ViewModels;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public static class AssetImporter
    {

        #region Properties

        public static string ActiveIdxPath { get; set; }
        public static string ActiveImgPath { get; set; }
        public static Img ActiveImg { get; set; }
        public static int CurrentPhase { get; private set; }
        private static Dictionary<AssetFormat, ImportFormat> ImportFormats = new()
        {
            { AssetFormat.Mdlx, ImportFormat.Fbx },
            { AssetFormat.Mset, ImportFormat.Json },
        };
        public static AssetFormat ExtractableFormats => AssetFormat.Mdlx | AssetFormat.Mset;
        public static AssetFormat ExportableFormats => AssetFormat.Mdlx | AssetFormat.Mset;

        public static IEnumerable<string> ExtractedAssets =>
            Directory.Exists(OpenKhPath.TempDir)
                ? Directory.GetFiles(OpenKhPath.TempDir, "*.*", SearchOption.AllDirectories)
                : Array.Empty<string>();
        public static IEnumerable<string> ImportedAssets =>
            Directory.Exists(OpenKhPath.AssetImportDir)
                ? Directory.GetFiles(OpenKhPath.AssetImportDir, "*.*", SearchOption.AllDirectories)
                : Array.Empty<string>();
        public static IEnumerable<string> ExportableAssets => ExtractedAssets.Where(IsExportable);

        #endregion

        #region Helper methods

        private static bool IsExtractable(FileViewModel fvm) =>
            ExtractableFormats.HasFlag(fvm.Entry.GetAssetFormat());
        private static bool IsExtractable(string filePath) =>
            ExtractableFormats.HasFlag(IdxEntryExtensions.GetAssetFormat(filePath));
        private static bool IsExportable(string filePath) =>
            ExportableFormats.HasFlag(IdxEntryExtensions.GetAssetFormat(filePath));

        public static bool IsImported(FileViewModel fvm)
        {
            var format = fvm.Entry.GetAssetFormat();

            if (!ImportFormats.TryGetValue(format, out var iFormat))
                return false;

            var ext = "." + iFormat.ToString().ToLowerInvariant();
            var assetPath = GetExportPath(fvm, ext);

            return ImportedAssets.Contains(assetPath);
        }

        public static string GetExtractPath(FileViewModel fvm, string extension = null)
        {
            var p = Path.Combine(
                OpenKhPath.TempDir,
                Path.GetFileNameWithoutExtension(ActiveIdxPath),
                fvm.FullName!
            );

            return string.IsNullOrEmpty(extension) ? p : Path.ChangeExtension(p, extension);
        }
        public static string GetExportPath(FileViewModel fvm, string extension = null, bool createSubfolder = true)
        {
            var basePath = Path.Combine(
                OpenKhPath.AssetImportDir,
                Path.GetFileNameWithoutExtension(ActiveIdxPath));

            string subPath;

            //  Insert subfolder into path
            if (createSubfolder)
            {
                subPath = Path.Combine(
                    Path.GetDirectoryName(fvm.FullName)!,
                    Path.GetFileNameWithoutExtension(fvm.Name),
                    fvm.Name!);
            }
            else
                subPath = fvm.FullName;

            var p = Path.Combine(basePath, subPath);

            return string.IsNullOrEmpty(extension) ? p : Path.ChangeExtension(p, extension);
        }
        public static string GetExportPath(string filePath, string extension = null, bool createSubfolder = true)
        {
            filePath = filePath.Replace(OpenKhPath.TempDir, OpenKhPath.AssetImportDir);

            //  Insert subfolder into path
            if (createSubfolder)
                filePath = Path.Combine(
                    Path.GetDirectoryName(filePath)!,
                    Path.GetFileNameWithoutExtension(filePath),
                    Path.GetFileName(filePath));

            return string.IsNullOrEmpty(extension) ? filePath : Path.ChangeExtension(filePath, extension);
        }

        public static void Reset()
        {
            ActiveIdxPath = null;
            ActiveImgPath = null;
            ActiveImg = null;
            CurrentPhase = 0;
        }

        #endregion

        #region Import phases

        /// <summary>
        /// [Phase 1] Extract an asset file contained in an IDX/IMG file pair
        /// </summary>
        /// <param name="fvm">View model of the asset file to extract</param>
        /// <param name="outFile">Path of the extracted file</param>
        /// <returns>True if extraction has been successful</returns>
        private static bool DumpAsset(FileViewModel fvm, out string outFile)
        {
            //Debug.Log($"Extracting asset '{fvm.Name}'..");

            //  Skip unsupported asset formats
            if (!IsExtractable(fvm))
            {
                outFile = null;
                return false;
            }

            //  Retrieve file stream
            if (!ActiveImg.TryFileOpen(fvm.FullName, out var stream))
            {
                //Debug.LogWarning($"Could not import '{fvm.Entry.GetFullName()}'");
                outFile = null;
                return false;
            }

            //  Write file to temp directory
            outFile = GetExtractPath(fvm);
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
        /// <summary>
        /// [Phase 1] Extract an asset file contained in an IDX/IMG file pair
        /// </summary>
        /// <param name="fvm">View model of the asset file to extract</param>
        private static bool DumpAsset(FileViewModel fvm) => DumpAsset(fvm, out _);
        /// <summary>
        /// [Phase 2] Export all previously extracted assets to file formats suitable for further conversion
        /// </summary>
        /// <returns>True if at least one asset failed or was cancelled, false if the export was successful.</returns>
        private static bool ExportAssets() =>
            ExportableAssets.Any(asset => IdxEntryExtensions.GetAssetFormat(asset) switch
            {
                AssetFormat.Mdlx => !MotionExport.ToAset(asset, OpProgress.Cancellable, out _, CurrentPhase),
                AssetFormat.Mset => !MotionExport.ToMson(asset, OpProgress.Cancellable, out _, CurrentPhase),
                _ => false,
            });
        /// <summary>
        /// [Phase 3] Convert extracted assets to file formats supported by Unity
        /// </summary>
        /// <returns></returns>
        private static bool ConvertAssets()
        {
            var current = 0;
            var total = ExportableAssets.Count();
            var status = new OperationStatus
            {
                OperationType = "FBX import",
                Total = total,
                Phase = CurrentPhase,
            };
            
            foreach (var asset in ExportableAssets)
            {
                status.Message = Path.GetFileNameWithoutExtension(asset) + ".fbx";
                status.Current = current;
                
                if (OpProgress.Cancellable(status))
                    return true;

                if (!MotionExport.ToFbx(asset))
                    throw new AssetConversionException($"Could not convert {asset} to FBX!");

                current++;
            }

            return false;
        }
        /// <summary>
        /// [Phase 4] Clean up temporary files
        /// </summary>
        /// <returns>True if the operation was successful, false if it failed or was cancelled by the user</returns>
        private static bool CleanUp()
        {
            if (!Directory.Exists(OpenKhPath.TempDir))
                return true;

            var total = ExtractedAssets.Count();
            var status = new OperationStatus
            {
                OperationType = "Cleanup",
                Phase = CurrentPhase,
                Message = "Starting cleanup..",
                Total = total,
            };

            if (OpProgress.Cancellable(status))
                return false;

            status.State = OperationState.Processing;

            foreach (var asset in ExtractedAssets)
            {
                status.Current++;
                status.Message = Path.GetFileName(asset);

                if (OpProgress.Cancellable(status))
                    return false;

                File.Delete(asset);
            }

            Directory.Delete(OpenKhPath.TempDir, true);

            status.State = OperationState.Finished;
            status.Current = total;

            OpProgress.Display(status);

            return true;
        }
        #endregion

        #region Main import method

        /// <summary>
        /// Import assets contained in an IDX/IMG file pair
        /// </summary>
        /// <param name="assets">The assets to import</param>
        public static void ImportAssets(List<FileViewModel> assets)
        {
            try
            {
                #region PHASE 0: Pre-cleanup

                if (!CleanUp())
                    throw new OperationCanceledException();

                #endregion

                #region PHASE 1: Dump

                var extractStatus = new OperationStatus
                {
                    OperationType = "Asset dump",
                    Current = 0,
                    Total = assets.Count,
                    Phase = ++CurrentPhase,
                };

                //  Extract checked assets
                foreach (var fvm in assets)
                {
                    //Debug.Log($"Extracting {fvm.Name} ..");

                    extractStatus.Message = fvm.FullName;
                    extractStatus.Current++;
                    extractStatus.State = OperationState.Processing;

                    if (OpProgress.Cancellable(extractStatus))
                        throw new OperationCanceledException();

                    if(!DumpAsset(fvm))
                        Debug.LogWarning($"Could not extract {fvm.FullName}");
                }

                extractStatus.State = OperationState.Finished;

                if (OpProgress.Cancellable(extractStatus))
                    throw new OperationCanceledException();
                
                #endregion

                #region PHASE 2: Export

                //  Phase 2
                CurrentPhase++;
                
                //  Export extracted assets to file formats suitable for further conversion
                if (ExportAssets())
                    throw new OperationCanceledException();

                #endregion

                #region PHASE 3: Convert/import

                var tmpStatus = new OperationStatus
                {
                    OperationType = "FBX import",
                    Phase = ++CurrentPhase,
                };

                if (OpProgress.Cancellable(tmpStatus))
                    throw new OperationCanceledException();

                //  Convert assets to Unity-supported file formats
                if (ConvertAssets())
                    throw new OperationCanceledException();

                #endregion

            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException)
                {
                    Debug.Log("Asset import cancelled by user.");
                }
                else
                {
                    Debug.LogWarning("Asset import failed!");
                    Debug.LogError(ex);
                }
            }
            finally
            {
                #region PHASE 4: Cleanup

                CurrentPhase++;

                if (!CleanUp())
                    throw new OperationCanceledException();

                #endregion

                OpProgress.Clear();
                CurrentPhase = 0;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log("Import done.");
        }

        #endregion
    }
}
