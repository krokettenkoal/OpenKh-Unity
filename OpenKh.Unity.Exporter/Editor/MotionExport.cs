using System;
using System.IO;
using Noesis;
using OpenKh.Kh2;
using OpenKh.Unity.Exporter.Aset;
using OpenKh.Unity.Exporter.Mson;
using OpenKh.Unity.Exporter.Progress;
using OpenKh.Unity.Tools.IdxImg.IO;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using UnityEngine;

namespace OpenKh.Unity.Exporter
{
    public static class MotionExport
    {
        private const string _msonExportTitle = "MSON export";

        private static NoesisClient _noesis { get; } = new();

        #region ASET export

        /// <summary>
        /// Exports an ASET file from the MDLX file at the specified. The method requires a corresponding MSET file in the same directory as the MDLX file.
        /// </summary>
        /// <param name="mdlxPath">The path of the MDLX file to export data from</param>
        /// <param name="onProgress">Cancellable callback function for the export progress. The function is called at several states of the export process containing the current state/status information. If the callback function returns true, the export is cancelled.</param>
        /// <param name="outFile">Path of the exported file</param>
        /// <param name="phase">The current operation phase (optional)</param>
        /// <returns>True if the conversion has been successful, false if it failed or was cancelled</returns>
        public static bool ToAset(string mdlxPath, Func<OperationStatus, bool> onProgress, out string outFile, int phase = -1)
        {
            if (!AsetExport.IsValidMdlx(mdlxPath))
            {
                outFile = string.Empty;
                return false;
            }

            //  Create progress handler
            var progress = new OperationProgress();
            progress.OnProgressCancellable += onProgress;

            //  Initialize converter
            using var exporter = new AsetExport(mdlxPath);

            //  Export ASET file
            return exporter.ExportASET(progress, out outFile, phase);
        }

        #endregion

        #region Noesis FBX export/convert

        /// <summary>
        /// Convert an MDLX file into an FBX file. If a corresponding ASET file in the same directory is found, animations are contained in the output file.
        /// </summary>
        /// <param name="mdlxPath">Path to the MDLX file to convert</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToFbx(string mdlxPath)
        {
            var destPath = AssetImporter.GetExportPath(mdlxPath, ".fbx");
            return _noesis.Export(mdlxPath, destPath, OpProgress.Cancellable);
        }

        #endregion

        #region MSON export

        /// <summary>
        /// Export motion clip information from an MSET file
        /// </summary>
        /// <param name="msetPath">Path to the MSET file to export motion info from</param>
        /// <param name="onProgress">Progress handler callback</param>
        /// <param name="outFile">Path to the resulting MSON file</param>
        /// <param name="phase">The current operation phase (optional)</param>
        /// <returns>True if the export was successful, false if it failed or was cancelled</returns>
        public static bool ToMson(string msetPath, Func<OperationStatus, bool> onProgress, out string outFile, int phase = 0)
        {
            //  File does not exist or is not an MSET file
            if (!File.Exists(msetPath) || ViewModelExtensions.GetAssetFormat(msetPath) != AssetFormat.Mset)
            {
                Debug.LogWarning($"MSON export failed: {Path.GetFileName(msetPath)} not found or not an MSET file.");
                outFile = null;
                return false;
            }

            //  Assemble export/output path
            var msonPath = AssetImporter.GetExportPath(msetPath, ".json");
            var status = new OperationStatus
            {
                OperationType = "MSON export",
                Message = Path.GetFileName(msonPath),
                Total = 1,
                Current = 1,
                ProgressFactor = 0,
            };

            if(phase > 0) 
                status.Phase = phase;
            
            //  Display cancellable progress
            var cancel = onProgress?.Invoke(status);
            if (cancel is true)
            {
                outFile = null;
                return false;
            }

            //  Load file as BAR
            using var stream = File.OpenRead(msetPath);
            if (!Bar.IsValid(stream))
            {
                Debug.LogWarning($"MSON export failed: {Path.GetFileName(msetPath)} is not a valid MSET/BAR file.");
                outFile = null;
                return false;
            }

            status.ProgressFactor = .1f;
            status.State = OperationState.Processing;

            //  Display cancellable progress
            cancel = onProgress?.Invoke(status);
            if (cancel is true)
            {
                outFile = null;
                return false;
            }

            //  Create MSON file
            MsonFile.Create(msonPath, Bar.Read(stream));

            status.ProgressFactor = 1;
            status.State = OperationState.Finished;

            //  Display cancellable progress
            cancel = onProgress?.Invoke(status);
            if (cancel is true)
            {
                outFile = null;
                return false;
            }

            outFile = msonPath;
            return true;
        }

        #endregion
    }
}
