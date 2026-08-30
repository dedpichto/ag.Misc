using System;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Commands
{
    public interface IUICommand : ICommand, IDisposable
    {
        string Name { get; }
        string Text { get; }
        string ToolTip { get; }
        KeyGesture HotKey { get; }
        object CommandParameter { get; set; }
        void Bind(Action<object> executed,
            Func<object, bool> canExecute);
    }
}
