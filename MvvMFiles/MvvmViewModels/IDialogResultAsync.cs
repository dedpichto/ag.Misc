using System;

namespace ShayCommon.Mvvm.ViewModels
{
    public interface IDialogResultAsync<T>
    {
        event EventHandler<T> ResultCompleted;
        event EventHandler ResultCancelled;
    }
}
