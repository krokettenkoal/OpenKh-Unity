using Unity.Collections;
using Unity.Jobs;
using OpenKh.Unity.Tools.AsetExport;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public struct ExportJobParallel : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> QueueIds;
        public ExportFormat Format;
        public NativeArray<bool> Result;

        public void Execute(int i)
        {
            if (ExportQueue.Active.Count <= QueueIds[i])
            {
                Result[i] = false;
                return;
            }

            var filePath = ExportQueue.Active[QueueIds[i]];
            
            Result[i] = Format switch
            {
                ExportFormat.Aset => ExportAset(filePath),
                _ => false,
            };
        }

        private bool ExportAset(string filePath) => MdlxConvert.ToAset(filePath, Utils.DisplayExportProgress, out _);
        
    }
}
