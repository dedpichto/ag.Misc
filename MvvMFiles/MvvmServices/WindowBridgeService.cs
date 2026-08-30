using ShayCommon.Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Services
{
    public class WindowBridgeService : IWindowBridgeService
    {
        private Window _window;

        public event EventHandler Loaded;
        public event EventHandler SourceInitialized;
        public event EventHandler Closed;
        public event EventHandler ContentRendered;
        public event EventHandler<CancelEventArgs> Closing;

        public void AttachWindow(Window window)
        {
            _window = window;
            _window.Loaded += (_, _) => Loaded?.Invoke(this, EventArgs.Empty);
            _window.Closed += (_, _) => Closed?.Invoke(this, EventArgs.Empty);
            _window.SourceInitialized += (_, _) => SourceInitialized?.Invoke(this, EventArgs.Empty);
            _window.ContentRendered += (_, _) => ContentRendered?.Invoke(this, EventArgs.Empty);
            _window.Closing += (_, e) => Closing?.Invoke(this, e);
        }
        public void CloseWindow() => _window.Close();
        public void RegisterBindings(IUICommand command)
        {

            if (command.HotKey is KeyGesture gesture)
            {
                _window?.InputBindings.Add(new KeyBinding(command, gesture));
            }
        }

        public void HideWindow() => _window.Hide();
        public void ShowWindow()
        {
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();
        }

        public void ApplyPositionAndSize(Point point, Size size)
        {
            if (point != default)
            {
                if (point.X >= 0 && point.Y >= 0)
                {
                    _window.Left = point.X;
                    _window.Top = point.Y;
                }
                else
                {
                    _window.Top = _window.Left = 0;
                }
            }
            if (_window.ResizeMode == ResizeMode.CanResize || _window.ResizeMode == ResizeMode.CanResizeWithGrip)
            {
                if (size.Width > 0 && size.Height > 0)
                {
                    _window.Width = size.Width;
                    _window.Height = size.Height;
                }
            }
        }

        public string GetWindowTypeName() => _window.GetType().Name;

        double _height;
        private void Anim_Completed(object sender, EventArgs e)
        {
            if (sender is not AnimationClock animation) return;
            animation.Completed -= Anim_Completed;
            _window.BeginAnimation(Window.TopProperty, null);
            if (_window.Left < 0 || _window.Top < 0)
            {
                _window.Top = _window.Left = 0;
            }
            else if (_window.Top > Math.Ceiling((_height - _window.ActualHeight) / 2))
            {
                var anim = new DoubleAnimation(_window.Top, (_height - _window.ActualHeight) / 2, new Duration(TimeSpan.FromMilliseconds(100)))
                {
                    AccelerationRatio = 0.1
                };
                anim.Completed += Anim_Completed;
                _window.BeginAnimation(Window.TopProperty, anim);
            }
        }

        public void StartWindowAnimation()
        {
            var width = SystemParameters.WorkArea.Width;
            _height = SystemParameters.WorkArea.Height;
            if (_window.Owner != null)
            {
                _window.Left = width < _window.Owner.Left - _window.ActualWidth
                    ? _window.Owner.Left - _window.ActualWidth
                    : _window.Owner.Left + (_window.Owner.ActualWidth - _window.ActualWidth) / 2;
                var anim = new DoubleAnimation(-100, _window.Owner.Top + (_window.Owner.ActualHeight - _window.ActualHeight) / 2, new Duration(TimeSpan.FromMilliseconds(300)))
                {
                    AccelerationRatio = 0.1
                };
                _window.BeginAnimation(Window.TopProperty, anim);
            }
            else
            {
                _window.Left = (width - _window.ActualWidth) / 2;

                var anim = new DoubleAnimation(-_window.ActualHeight, (_height - _window.ActualHeight) / 2, new Duration(TimeSpan.FromMilliseconds(500)))
                {
                    AccelerationRatio = 0.1
                };
                anim.Completed += Anim_Completed;
                _window.BeginAnimation(Window.TopProperty, anim);
            }
        }

        public Size GetWindowActualSize() => new(_window.ActualWidth, _window.ActualHeight);
        public Point GetWindowActualPosition() => new(_window.Left, _window.Top);
        public WindowState GetWindowActualState() => _window.WindowState;
        public ResizeMode GetWindowResizeMode() => _window.ResizeMode;
        public HwndSource GetWindowHwndSource() => HwndSource.FromHwnd(new WindowInteropHelper(_window).Handle);
        public string GetWindowTitle() => _window.Title;
        public object GetResource(string resourceName) => _window.TryFindResource(resourceName);
        public double GetWindowFontSize() => _window.FontSize;
        public FontFamily GetWindowFontFamily() => _window.FontFamily;
        public FontStyle GetFontStyle() => _window.FontStyle;
        public FontWeight GetFontWeight() => _window.FontWeight;
        public void RegisterGlobalBindings(Dictionary<string, CommandGesture> gestures,
            Action<string> action)
        {
            var names = gestures.Keys.ToArray();
            var commands = createBatchCommands(gestures,
               action).ToArray();
            for (var i = 0; i < gestures.Count; i++)
            {
                var name = names[i];
                _window?.InputBindings.Add(new KeyBinding(commands[i], new KeyGesture(gestures[name].Key, gestures[name].Modifiers)));
            }
        }
        public void SetWindowCursor(Cursor cursor)
        {
            _window.Cursor= cursor;
            _window.ForceCursor = cursor != null;
        }
        public Cursor GetWindowCursor() => _window.Cursor;
        public Dispatcher Dispatcher => _window.Dispatcher;

        private IEnumerable<Action> createBatchActions(
            Dictionary<string, CommandGesture> gestures,
            Action<string> action)
            => gestures.Keys.Select(name => (Action)(() => action(name)));

        private IEnumerable<ICommand> createBatchCommands(
            Dictionary<string, CommandGesture> gestures,
            Action<string> action)
            => createBatchActions(gestures, action).Select(act => new RelayCommand(act, canExecute: () => true));
        public Window GetWindow() => _window;
        public void CentralizeWindow()
        {
            var source = PresentationSource.FromVisual(_window);
            double dpiX = 1, dpiY = 1;
            if (source != null)
            {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }

            var hwnd = new WindowInteropHelper(_window).EnsureHandle();
            var (left, top, height, width) = Interop.GetCurrentMonitorSize(hwnd);
            left /= dpiX;
            top /= dpiY;
            width /= dpiX;
            height /= dpiY;

            _window.Left = left + (width - _window.ActualWidth) / 2;
            _window.Top = top + (height - _window.ActualHeight) / 2;
        }
    }
}
