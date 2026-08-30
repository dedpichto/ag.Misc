using ShayCommon.Mvvm.Events;
using System;

namespace ShayCommon.Mvvm.ViewModels
{
    public interface IDialogViewModelWithActions
    {
        event EventHandler<DialogCloseEventArgs> RequestClose;
    }
}
