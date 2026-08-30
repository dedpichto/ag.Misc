using ShayCommon.Mvvm.Events;
using ShayCommon.Mvvm.Services;
using System;

namespace ShayCommon.Mvvm.ViewModels
{
    public abstract class DialogViewModelBaseWithActions<TResult> : BaseViewModelWithActions, IDialogViewModelWithActions, IDialogResult<TResult>, IDialogResultAsync<TResult>, IParametersReceiver
    {
        private TResult _result;

        public TResult Result
        {
            get => _result;
            protected set { _result = value; OnPropertyChanged(); }
        }

        public event EventHandler<TResult> ResultCompleted;
        public event EventHandler ResultCancelled;
        public event EventHandler<DialogCloseEventArgs> RequestClose;

        protected virtual void CompleteWithResult(TResult result)
        {
            Result = result;
            ResultCompleted?.Invoke(this, result);
            CloseDialog(true);
        }

        protected virtual void Cancel()
        {
            Result = default;
            ResultCancelled?.Invoke(this, EventArgs.Empty);
            CloseDialog(false);
        }

        public virtual void ReceiveParameters(object parameters) { }
        protected virtual void CloseDialog(bool? dialogResult = null)
            => RequestClose?.Invoke(this, new DialogCloseEventArgs(dialogResult));

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ResultCompleted = null;
                ResultCancelled = null;
                RequestClose = null;
            }
            base.Dispose(disposing);
        }
    }
}
