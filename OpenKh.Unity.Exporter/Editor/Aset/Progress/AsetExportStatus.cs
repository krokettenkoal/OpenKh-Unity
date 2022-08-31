using OpenKh.Unity.Exporter.Interfaces;

namespace OpenKh.Unity.Exporter.Aset.Progress
{
    public class AsetExportStatus : IExportStatus
    {
        public string ExportType => "ASET";
        public string FileName { get; set; }
        public string animName;

        public int animCount;
        public int animIndex;

        public int frameCount;
        public int frameIndex;

        public int jointCount;
        public int jointIndex;

        public float Progress => (float)animIndex / animCount;

    }
}
