using ShayCommon.Mvvm.Events;
using System;

namespace ShayCommon.Mvvm.ViewModels
{
    public interface IDialogViewModel : IViewModel
    {
        event EventHandler<DialogCloseEventArgs> RequestClose;
    }
}
