using System;
using OpenKh.Unity.Exporter.Interfaces;

namespace OpenKh.Unity.Exporter.Progress {
    public class ExportProgress<T> where T : IExportStatus
    {
        private bool _cancel;
        public bool CancellationPending {
            get {
                var c = _cancel;
                _cancel = false;
                return c; 
            }

            private set => _cancel = value;
        }

        public event Action<OperationState, T> OnProgress;
        public event Func<OperationState, T, bool> OnProgressCancellable; 
        public void Update(OperationState state, T status)
        {
            OnProgress?.Invoke(state, status);
            var cancel = OnProgressCancellable?.Invoke(state, status);

            if (cancel is true)
                Cancel();
        }
        public void Cancel() {
            CancellationPending = true;
        }
    }
}
