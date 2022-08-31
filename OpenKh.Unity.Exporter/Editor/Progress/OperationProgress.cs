using System;

namespace OpenKh.Unity.Exporter.Progress {
    public class OperationProgress
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

        public event Action<OperationStatus> OnProgress;
        public event Func<OperationStatus, bool> OnProgressCancellable; 
        public void Update(OperationStatus status)
        {
            OnProgress?.Invoke(status);
            var cancel = OnProgressCancellable?.Invoke(status);

            if (cancel is true)
            {
                status.State = OperationState.Cancelling;
                Cancel();
            }
        }
        public void Cancel() {
            CancellationPending = true;
        }
    }
}
