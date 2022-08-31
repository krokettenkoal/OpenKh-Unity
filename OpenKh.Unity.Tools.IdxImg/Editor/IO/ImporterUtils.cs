using OpenKh.Unity.Exporter.Aset.Progress;
using OpenKh.Unity.Exporter.Interfaces;
using OpenKh.Unity.Exporter.Progress;
using UnityEditor;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public static class ImporterUtils
    {
        public static void ClearProgress() => EditorUtility.ClearProgressBar();

        #region General progress

        /// <summary>
        /// Display a general-purpose progress bar
        /// </summary>
        /// <param name="title">Progress bar title</param>
        /// <param name="info">Progress bar message</param>
        /// <param name="progress">Progress value from 0 (no progress) to 1 (finished)</param>
        public static void Progress(string title, string info, float progress) =>
            EditorUtility.DisplayProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {title}",
                info,
                progress);

        /// <summary>
        /// Display a cancellable general-purpose progress bar
        /// </summary>
        /// <param name="title">Progress bar title</param>
        /// <param name="info">Progress bar message</param>
        /// <param name="progress">Progress value from 0 (no progress) to 1 (finished)</param>
        /// <returns>True if the 'Cancel' button was clicked</returns>
        public static bool ProgressCancel(string title, string info, float progress) =>
            EditorUtility.DisplayCancelableProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {title}",
                info,
                progress);
        
        #endregion
        
        #region Dump progress

        /// <summary>
        /// Display a progress bar for an asset dump operation
        /// </summary>
        /// <param name="state">Current extract state</param>
        /// <param name="status">Extract status information</param>
        public static void DumpProgress(OperationState state, ExtractStatus status) =>
        EditorUtility.DisplayProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {state}: Asset extraction ({status.current + 1} / {status.total})..",
                status.FileName,
                status.Progress
            );
            /// <summary>
        /// Display a cancellable progress bar for an asset dump operation
        /// </summary>
        /// <param name="state">Current extract state</param>
        /// <param name="status">Extract status information</param>
        /// <returns>True if the 'Cancel' button was clicked</returns>
        public static bool DumpProgressCancel(OperationState state, ExtractStatus status) =>
            EditorUtility.DisplayCancelableProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {state}: Asset extraction ({status.current + 1} / {status.total})..", 
                status.FileName,
                status.Progress
                );

        #endregion

        #region Export progress

        /// <summary>
        /// Display a progress bar for a general export operation
        /// </summary>
        /// <param name="state">Current export state</param>
        /// <param name="status">Export status information</param>
        public static void ExportProgress(OperationState state, IExportStatus status) =>
            EditorUtility.DisplayProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {state}: {status.ExportType} export",
                status.FileName,
                status.Progress
            );
        /// <summary>
        /// Display a cancellable progress bar for a general export operation
        /// </summary>
        /// <param name="state">Current export state</param>
        /// <param name="status">Export status information</param>
        /// <returns>True if the 'Cancel' button was clicked</returns>
        public static bool ExportProgressCancel(OperationState state, IExportStatus status) =>
            EditorUtility.DisplayCancelableProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {state}: {status.ExportType} export",
                status.FileName,
                status.Progress
            );
        /// <summary>
        /// Display a progress bar for an ASET export operation
        /// </summary>
        /// <param name="state">Current export state</param>
        /// <param name="status">Export status information</param>
        public static void AsetExportProgress(OperationState state, AsetExportStatus status) =>
            EditorUtility.DisplayProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {state}: {status.ExportType} export ({status.animIndex}/{status.animCount})",
                $"{status.FileName}: {status.animName}",
                status.Progress
            );
        /// <summary>
        /// Display a cancellable progress bar for an ASET export operation
        /// </summary>
        /// <param name="state">Current export state</param>
        /// <param name="status">ASET export status information</param>
        /// <returns>True if the 'Cancel' button was clicked</returns>
        public static bool AsetExportProgressCancel(OperationState state, AsetExportStatus status) =>
            EditorUtility.DisplayCancelableProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {state}: {status.ExportType} export ({status.animIndex}/{status.animCount})",
                $"{status.FileName}: {status.animName}",
                status.Progress
            );

        #endregion

        #region Process progress

        /// <summary>
        /// Display a progress bar for a specific process
        /// </summary>
        /// <param name="status">The process status containing all necessary information</param>
        public static void ProcessProgress(ProcessStatus status) =>
            EditorUtility.DisplayProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {status.Title}",
                status.Message,
                status.Progress);

        /// <summary>
        /// Display a cancellable progress bar for a specific process
        /// </summary>
        /// <param name="status">The process status containing all necessary information</param>
        /// <returns>True if the 'Cancel' button was clicked</returns>
        public static bool ProcessProgressCancel(ProcessStatus status) =>
            EditorUtility.DisplayCancelableProgressBar(
                $"[Phase {AssetImporter.CurrentPhase}] {status.Title}",
                status.Message,
                status.Progress);

        #endregion
    }

}
