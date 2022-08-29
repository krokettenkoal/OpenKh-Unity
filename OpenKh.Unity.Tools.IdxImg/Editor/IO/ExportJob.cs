using Unity.Collections;
using Unity.Jobs;
using OpenKh.Unity.Tools.AsetExport;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public struct ExportJob : IJob
    {
        [ReadOnly]
        public int QueueId;
        public ExportFormat Format;
        public NativeArray<bool> Result;

        public void Execute()
        {
            if (ExportQueue.Active.Count <= QueueId)
            {
                Result[0] = false;
                return;
            }

            var filePath = ExportQueue.Active[QueueId];

            Result[0] = Format switch
            {
                ExportFormat.Aset => ExportAset(filePath),
                _ => false,
            };
        }

        private bool ExportAset(string filePath) => MdlxConvert.ToAset(filePath, Utils.DisplayExportProgress, out _);

    }
}
