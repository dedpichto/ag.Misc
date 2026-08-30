using System;

namespace ShayCommon.Mvvm.ViewModels
{
    public abstract class DialogViewModelBase<TResult> : BaseDialogViewModel, IDialogResult<TResult>, IDialogResultAsync<TResult>, IDisposable
    {
        private TResult _result;

        public TResult Result
        {
            get => _result;
            protected set { _result = value; OnPropertyChanged(); }
        }

        public event EventHandler<TResult> ResultCompleted;
        public event EventHandler ResultCancelled;

        protected virtual void CompleteWithResult(TResult result)
        {
            Result = result;
            ResultCompleted?.Invoke(this, result);
            CloseDialog(true);
        }

        protected virtual void Cancel()
        {
            Result = default(TResult);
            ResultCancelled?.Invoke(this, EventArgs.Empty);
            CloseDialog(false);
        }

        public void Dispose()
        {
            ResultCompleted = null;
            ResultCancelled = null;
            ReleaseCommands();
        }
    }
}
