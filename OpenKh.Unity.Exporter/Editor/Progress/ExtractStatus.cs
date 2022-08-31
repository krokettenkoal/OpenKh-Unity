using OpenKh.Unity.Exporter.Interfaces;

namespace OpenKh.Unity.Exporter.Progress
{
    public struct ExtractStatus : IExportStatus
    {
        public string ExportType => "Asset dump";
        public string FileName { get; set; }
        public int current;
        public int total;
        public float Progress => (float)current / total;

    }
}
