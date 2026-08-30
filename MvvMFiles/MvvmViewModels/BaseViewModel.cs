using ShayCommon.Mvvm.Commands;
using ShayCommon.Mvvm.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ShayCommon.Mvvm.ViewModels
{
    public abstract class BaseViewModel : DependencyObject, IViewModel
    {

        public abstract IWindowBridgeService WindowBridgeService { set; }

        public List<IUICommand> Commands { get; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
        public virtual void ReloadBrushes() { }

        private string _themeName;
        public virtual string ThemeName
        {
            get => _themeName;
            set { if (_themeName == value) return; _themeName = value; OnPropertyChanged(); }
        }
    }
}
