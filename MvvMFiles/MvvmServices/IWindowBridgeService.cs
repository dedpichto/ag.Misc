using ShayCommon.Mvvm.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Services
{
    public interface IWindowBridgeService
    {
        event EventHandler Loaded;
        event EventHandler Closed;
        event EventHandler<CancelEventArgs> Closing;
        event EventHandler SourceInitialized;
        event EventHandler ContentRendered;

        Dispatcher Dispatcher { get; }

        void HideWindow();
        void ShowWindow();
        void CloseWindow();
        void ApplyPositionAndSize(Point point, Size size);
        void StartWindowAnimation();
        string GetWindowTypeName();
        Size GetWindowActualSize();
        Point GetWindowActualPosition();
        double GetWindowFontSize();
        FontFamily GetWindowFontFamily();
        FontStyle GetFontStyle();
        FontWeight GetFontWeight();
        WindowState GetWindowActualState();
        ResizeMode GetWindowResizeMode();
        HwndSource GetWindowHwndSource();
        Window GetWindow();
        string GetWindowTitle();
        void AttachWindow(Window window);
        void RegisterBindings(IUICommand command);
        void RegisterGlobalBindings(Dictionary<string, CommandGesture> gestures, Action<string> action);
        object GetResource(string resourceName);
        void CentralizeWindow();
        void SetWindowCursor(Cursor cursor);
        Cursor GetWindowCursor();
    }
}
