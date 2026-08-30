using ShayCommon.Mvvm.Commands;
using ShayCommon.Mvvm.Services;
using System.Collections.Generic;
using System.ComponentModel;

namespace ShayCommon.Mvvm.ViewModels
{
    public interface IViewModel : INotifyPropertyChanged
    {
        List<IUICommand> Commands { get; }
        IWindowBridgeService WindowBridgeService { set; }
        bool CommandCanExecute(IUICommand command);
        void CommandExecuted(IUICommand command);
    }
}
