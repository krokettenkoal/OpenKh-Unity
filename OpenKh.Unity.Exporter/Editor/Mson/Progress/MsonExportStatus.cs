using OpenKh.Unity.Exporter.Interfaces;

namespace OpenKh.Unity.Exporter.Mson.Progress
{
    public class MsonExportStatus : IExportStatus
    {
        public string ExportType => "MSON";
        public string FileName { get; set; }
        public float Progress { get; set; }
    }
}
