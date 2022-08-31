using System;

namespace OpenKh.Unity.Exporter.Progress
{
    public class OperationStatus
    {
        public virtual OperationState State { get; set; } = OperationState.Initialization;
        public virtual string OperationType { get; set; } = string.Empty;
        public virtual string Message { get; set; } = string.Empty;
        public virtual int Current { get; set; } = 0;
        public virtual int Total { get; set; }
        public virtual float Progress => Math.Clamp((float) Current / Total * ProgressFactor, 0, 1);
        public virtual float ProgressFactor { get; set; } = 1;
        public virtual int Phase { get; set; } = -1;

        public virtual string Title =>
            (Phase >= 0 ? $"[Phase {Phase}] " : "") + $"{State}: {OperationType} ({Current}/{Total})";
    }
}
