using System;
using System.Threading;
using System.Threading.Tasks;

namespace SterlingDB.Database
{
    public sealed class PendingOperationProgressChangedEventArgs : EventArgs
    {
        internal PendingOperationProgressChangedEventArgs(decimal progress)
        {
            Progress = progress;
        }

        public decimal Progress { get; }
    }

    public sealed class PendingOperationErrorEventArgs : EventArgs
    {
        internal PendingOperationErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception { get; }
    }

    public interface IPendingOperation
    {
        event EventHandler Completed;
        event EventHandler Canceled;
        event EventHandler<PendingOperationErrorEventArgs> ErrorOccured;
        event EventHandler<PendingOperationProgressChangedEventArgs> ProgressChanged;

        void Cancel();
    }

    public sealed class PendingOperation : IPendingOperation
    {
        private readonly CancellationTokenSource _cancelSource;
        private readonly Action _uow;

        internal PendingOperation(Action<CancellationToken> work)
        {
            _cancelSource = new CancellationTokenSource();
            _uow = () => work(_cancelSource.Token);
            Task = Task.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        internal PendingOperation(Action<CancellationToken> work, CancellationTokenSource cancelSource)
        {
            _cancelSource = cancelSource;
            _uow = () => work(_cancelSource.Token);
            Task = Task.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        internal PendingOperation(Action<CancellationToken, Action<decimal>> progressVisibleWork)
        {
            _cancelSource = new CancellationTokenSource();
            _uow = () => progressVisibleWork(_cancelSource.Token, ReportProgress);
            Task = Task.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        internal PendingOperation(Action<CancellationToken, Action<decimal>> progressVisibleWork,
            CancellationTokenSource cancelSource)
        {
            _cancelSource = cancelSource;
            _uow = () => progressVisibleWork(_cancelSource.Token, ReportProgress);
            Task = Task.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        public Task Task { get; }

        public event EventHandler Completed = delegate { };
        public event EventHandler Canceled = delegate { };
        public event EventHandler<PendingOperationErrorEventArgs> ErrorOccured = delegate { };
        public event EventHandler<PendingOperationProgressChangedEventArgs> ProgressChanged = delegate { };

        public void Cancel()
        {
            _cancelSource.Cancel();
        }

        private void DoWork()
        {
            try
            {
                _uow();
                Completed(this, EventArgs.Empty);
            }
            catch (OperationCanceledException)
            {
                Canceled(this, EventArgs.Empty);
                throw;
            }
            catch (Exception ex)
            {
                ErrorOccured(this, new PendingOperationErrorEventArgs(ex));
                throw;
            }
        }

        private void ReportProgress(decimal percentComplete)
        {
            ProgressChanged(this, new PendingOperationProgressChangedEventArgs(percentComplete));
        }
    }

    public sealed class PendingOperation<T> : IPendingOperation
    {
        private readonly CancellationTokenSource _cancelSource;
        private readonly Func<T> _uow;

        internal PendingOperation(Func<CancellationToken, T> work)
        {
            _cancelSource = new CancellationTokenSource();
            _uow = () => work(_cancelSource.Token);
            Task = Task<T>.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        internal PendingOperation(Func<CancellationToken, T> work, CancellationTokenSource cancelSource)
        {
            _cancelSource = cancelSource;
            _uow = () => work(_cancelSource.Token);
            Task = Task<T>.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        internal PendingOperation(Func<CancellationToken, Action<decimal>, T> progressVisibleWork)
        {
            _cancelSource = new CancellationTokenSource();
            _uow = () => progressVisibleWork(_cancelSource.Token, ReportProgress);
            Task = Task<T>.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        internal PendingOperation(Func<CancellationToken, Action<decimal>, T> progressVisibleWork,
            CancellationTokenSource cancelSource)
        {
            _cancelSource = cancelSource;
            _uow = () => progressVisibleWork(_cancelSource.Token, ReportProgress);
            Task = Task<T>.Factory.StartNew(DoWork, _cancelSource.Token);
        }

        public Task<T> Task { get; }

        public event EventHandler Completed = delegate { };
        public event EventHandler Canceled = delegate { };
        public event EventHandler<PendingOperationErrorEventArgs> ErrorOccured = delegate { };
        public event EventHandler<PendingOperationProgressChangedEventArgs> ProgressChanged = delegate { };

        public void Cancel()
        {
            _cancelSource.Cancel();
        }

        private T DoWork()
        {
            try
            {
                var result = _uow();

                Completed(this, EventArgs.Empty);

                return result;
            }
            catch (OperationCanceledException)
            {
                Canceled(this, EventArgs.Empty);
                throw;
            }
            catch (Exception ex)
            {
                ErrorOccured(this, new PendingOperationErrorEventArgs(ex));
                throw;
            }
        }

        private void ReportProgress(decimal percentComplete)
        {
            ProgressChanged(this, new PendingOperationProgressChangedEventArgs(percentComplete));
        }
    }
}