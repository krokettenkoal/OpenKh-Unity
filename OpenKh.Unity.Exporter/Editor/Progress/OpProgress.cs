using OpenKh.Unity.Exporter.Aset.Progress;
using UnityEditor;

namespace OpenKh.Unity.Exporter.Progress
{
    public static class OpProgress
    {
        public static void Clear() => EditorUtility.ClearProgressBar();

        /// <summary>
        /// Display a progress bar for a running operation
        /// </summary>
        /// <param name="status">Operation status information</param>
        public static void Display(OperationStatus status) =>
            EditorUtility.DisplayProgressBar(
                status.Title,
                status.Message,
                status.Progress
            );
        /// <summary>
        /// Display a cancellable progress bar for a running operation
        /// </summary>
        /// <param name="status">Export status information</param>
        /// <returns>True if the 'Cancel' button was clicked</returns>
        public static bool Cancellable(OperationStatus status) =>
            EditorUtility.DisplayCancelableProgressBar(
                status.Title,
                status.Message,
                status.Progress
            );
    }

}
