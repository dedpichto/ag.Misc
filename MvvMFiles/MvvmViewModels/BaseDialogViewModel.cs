using ShayCommon.Mvvm.Commands;
using ShayCommon.Mvvm.Events;
using ShayCommon.Mvvm.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShayCommon.Mvvm.ViewModels
{
    public abstract class BaseDialogViewModel : INotifyPropertyChanged, IParametersReceiver, IDialogViewModel
    {
        public List<IUICommand> Commands { get; } = new();

        public abstract IWindowBridgeService WindowBridgeService { set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DialogCloseEventArgs> RequestClose;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public virtual void ReceiveParameters(object parameters) { }
        protected virtual void CloseDialog(bool? dialogResult = null)
        {
            RequestClose?.Invoke(this, new DialogCloseEventArgs(dialogResult));
        }
        public abstract bool CommandCanExecute(IUICommand command);
        public abstract void CommandExecuted(IUICommand command);
        public void ReleaseCommands()
        {
            foreach (var command in Commands)
            {
                command.Dispose();
            }
        }

        private string _themeName;
        public virtual string ThemeName
        {
            get => _themeName;
            set { if (_themeName == value) return; _themeName = value; OnPropertyChanged(); }
        }
    }
}
