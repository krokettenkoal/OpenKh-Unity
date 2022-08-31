using OpenKh.Unity.Exporter.Progress;

namespace OpenKh.Unity.Exporter.Aset.Progress
{
    public class AsetExportStatus : OperationStatus
    {
        public override string OperationType => "ASET export";
        public override string Message => $"{FileName}: {AnimName}";
        
        public string FileName { get; set; }
        public string AnimName { get; set; }

        public int AnimCount { get; set; }
        public int AnimIndex { get; set; }

        public int FrameCount { get; set; }
        public int FrameIndex { get; set; }

        public int JointCount { get; set; }
        public int JointIndex { get; set; }

        public override int Current
        {
            get => AnimIndex;
            set => AnimIndex = value;
        }
        public override int Total
        {
            get => AnimCount;
            set => AnimCount = value;
        }

    }
}
