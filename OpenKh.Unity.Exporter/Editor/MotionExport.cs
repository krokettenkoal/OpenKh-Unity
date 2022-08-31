using System;
using System.IO;
using Noesis;
using OpenKh.Kh2;
using OpenKh.Unity.Exporter.Aset;
using OpenKh.Unity.Exporter.Aset.Progress;
using OpenKh.Unity.Exporter.Interfaces;
using OpenKh.Unity.Exporter.Mson;
using OpenKh.Unity.Exporter.Mson.Progress;
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
        /// <param name="onProgress">Callback function for the export progress. The function is called at several states of the export process containing the current state/status information.</param>
        /// <param name="outFile">Path of the exported file</param>
        /// <returns>True if the conversion has been successful</returns>
        public static bool ToAset(string mdlxPath, Action<OperationState, AsetExportStatus> onProgress, out string outFile)
        {
            if (!AsetExport.IsValidMdlx(mdlxPath))
            {
                outFile = string.Empty;
                return false;
            }

            //  Create progress handler
            var progress = new ExportProgress<AsetExportStatus>();
            progress.OnProgress += onProgress;

            //  Initialize converter
            using var exporter = new AsetExport(mdlxPath);

            //  Export ASET file
            return exporter.ExportASET(progress, out outFile);
        }
        /// <summary>
        /// Exports an ASET file from the MDLX file at the specified. The method requires a corresponding MSET file in the same directory as the MDLX file.
        /// </summary>
        /// <param name="mdlxPath">The path of the MDLX file to export data from</param>
        /// <param name="onProgress">Cancellable callback function for the export progress. The function is called at several states of the export process containing the current state/status information. If the callback function returns true, the export is cancelled.</param>
        /// <param name="outFile">Path of the exported file</param>
        /// <returns>True if the conversion has been successful, false if it failed or was cancelled</returns>
        public static bool ToAset(string mdlxPath, Func<OperationState, AsetExportStatus, bool> onProgress, out string outFile)
        {
            if (!AsetExport.IsValidMdlx(mdlxPath))
            {
                outFile = string.Empty;
                return false;
            }

            //  Create progress handler
            var progress = new ExportProgress<AsetExportStatus>();
            progress.OnProgressCancellable += onProgress;

            //  Initialize converter
            using var exporter = new AsetExport(mdlxPath);

            //  Export ASET file
            return exporter.ExportASET(progress, out outFile);
        }

        #endregion

        #region Noesis export/convert

        /// <summary>
        /// Convert an MDLX file into an FBX file. If a corresponding ASET file in the same directory is found, animations are contained in the output file.
        /// </summary>
        /// <param name="mdlxPath">Path to the MDLX file to convert</param>
        /// <returns>True if the conversion was successful</returns>
        public static bool ToFbx(string mdlxPath)
        {
            return _noesis.Export(mdlxPath, AssetImporter.GetExportPath(mdlxPath, ".fbx"));
        }

        #endregion

        #region MSON export

        /// <summary>
        /// Export motion clip information from an MSET file
        /// </summary>
        /// <param name="msetPath">Path to the MSET file to export motion info from</param>
        /// <param name="onProgress">Progress handler callback</param>
        /// <param name="outFile">Path to the resulting MSON file</param>
        /// <returns>True if the export was successful, false if it failed or was cancelled</returns>
        public static bool ToMson(string msetPath, Func<OperationState, IExportStatus, bool> onProgress, out string outFile)
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
            var status = new MsonExportStatus
            {
                FileName = Path.GetFileName(msonPath),
                Progress = 0,
            };
            
            //  Display cancellable progress
            var cancel = onProgress?.Invoke(OperationState.Initialization, status);
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

            status.Progress = .1f;

            //  Display cancellable progress
            cancel = onProgress?.Invoke(OperationState.Processing, status);
            if (cancel is true)
            {
                outFile = null;
                return false;
            }

            //  Create MSON file
            MsonFile.Create(msonPath, Bar.Read(stream));

            status.Progress = 1;

            //  Display cancellable progress
            cancel = onProgress?.Invoke(OperationState.Finished, status);
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
