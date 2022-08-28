using OpenKh.Unity.Tools.AsetExport;
using UnityEditor;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public static class Utils
    {
        public static bool DisplayExtractProgress(string fileName, int current, int total)
        {
            return EditorUtility.DisplayCancelableProgressBar(
                $"Importing assets ({current + 1} / {total})..", 
                fileName, 
                (float)current / total
                );
        }
        public static void DisplayExportProgress(ExportState state, ExportStatus status)
        {
            EditorUtility.DisplayCancelableProgressBar(
                $"{state}: ASET export ({status.animIndex}/{status.animCount})",
                status.animName,
                (float)status.animIndex / status.animCount
            );
        }
    }

}
