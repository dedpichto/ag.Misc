using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Commands
{
    public class UICommand : IUICommand
    {
        private static readonly TimeSpan _activeInterval = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan _idleInterval = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan _idleThreshold = TimeSpan.FromMinutes(5);

        // Shared across all UICommand instances — any user interaction resets the idle clock
        private static DateTime _lastActivity = DateTime.UtcNow;

        private readonly string _toolTip;

        private Action<object> _executed;
        private Func<object, bool> _canExecute;
        private bool _isDisposed;
        private readonly DispatcherTimer _timer;

        public string Name { get; }
        public string Text { get; set; }
        public string ToolTip => string.IsNullOrEmpty(_toolTip) ? Text : _toolTip;
        public KeyGesture HotKey { get; }
        public object CommandParameter { get; set; }
        public void Bind(Action<object> executed,
            Func<object, bool> canExecute)
        {
            _executed = executed ?? throw new ArgumentNullException(nameof(executed));
            _canExecute = canExecute;
        }

        public UICommand(
            string name = null,
            string text = null,
            string toolTip = null,
            KeyGesture hotKey = null,
            object commandParameter = null)
        {
            Name = name ?? string.Empty;
            Text = text ?? string.Empty;
            _toolTip = toolTip ?? string.Empty;
            HotKey = hotKey;
            CommandParameter = commandParameter;
            _isDisposed = false;
            _timer = new DispatcherTimer
            {
                Interval = _activeInterval
            };
            _timer.Tick += onTimerTick;
            _timer.Start();
        }

        private void onTimerTick(object sender, EventArgs e)
        {
            var idle = DateTime.UtcNow - _lastActivity;
            if (idle >= _idleThreshold && _timer.Interval != _idleInterval)
                _timer.Interval = _idleInterval;
            else if (idle < _idleThreshold && _timer.Interval != _activeInterval)
                _timer.Interval = _activeInterval;
            RaiseCanExecuteChanged();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public bool CanExecute(object parameter)
        {
            if (_isDisposed)
                return false;
            // Any CanExecute call driven by WPF requery means the user is interacting
            _lastActivity = DateTime.UtcNow;
            return _canExecute?.Invoke(parameter) ?? true;
        }
        public void Execute(object parameter)
        {
            if (!_isDisposed)
            {
                _lastActivity = DateTime.UtcNow;
                _executed(parameter);
            }
        }
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _timer.Stop();
                _timer.Tick -= onTimerTick;
                _executed = null;
                _canExecute = null;
            }
        }
        private void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
