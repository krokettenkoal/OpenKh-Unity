namespace OpenKh.Unity.Exporter.Interfaces
{
    public interface IExportStatus
    {
        public string ExportType { get; }
        public string FileName { get; set; }
        public float Progress { get; }
    }
}
