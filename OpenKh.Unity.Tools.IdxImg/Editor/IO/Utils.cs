using OpenKh.Unity.AsetExport;
using UnityEditor;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public static class Utils
    {
        public static void DisplayExtractProgress(ExtractState state, ExtractStatus status)
        {
            EditorUtility.DisplayProgressBar(
                $"[{state}]: Asset extraction ({status.current + 1} / {status.total})..",
                status.fileName,
                (float)status.current / status.total
            );
        }
        public static bool DisplayCancellableExtractProgress(ExtractState state, ExtractStatus status)
        {
            return EditorUtility.DisplayCancelableProgressBar(
                $"[{state}]: Asset extraction ({status.current + 1} / {status.total})..", 
                status.fileName, 
                (float)status.current / status.total
                );
        }
        public static void DisplayExportProgress(ExportState state, ExportStatus status)
        {
            EditorUtility.DisplayProgressBar(
                $"{state}: ASET export ({status.animIndex}/{status.animCount})",
                $"{status.fileName}: {status.animName}",
                (float)status.animIndex / status.animCount
            );
        }
        public static bool DisplayCancellableExportProgress(ExportState state, ExportStatus status) =>
            EditorUtility.DisplayCancelableProgressBar(
                $"{state}: ASET export ({status.animIndex}/{status.animCount})",
                $"{status.fileName}: {status.animName}",
                (float)status.animIndex / status.animCount
            );
    }

}
