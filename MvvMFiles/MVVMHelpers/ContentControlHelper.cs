using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ShayCommon.Mvvm.Helpers
{
    public static class ContentControlHelper
    {
        #region Loading
        public static readonly DependencyProperty ContentControlLoadedCallbackProperty =
            DependencyProperty.RegisterAttached("ContentControlLoadedCallback",
                typeof(Action<object>),
                typeof(ContentControlHelper),
                new PropertyMetadata(null, onContentControlLoaded));

        public static Action<object> GetContentControlLoadedCallback(DependencyObject obj) => (Action<object>)obj.GetValue(ContentControlLoadedCallbackProperty);

        public static void SetContentControlLoadedCallback(DependencyObject obj, Action<object> value) => obj.SetValue(ContentControlLoadedCallbackProperty, value);

        private static readonly ConditionalWeakTable<ContentControl, RoutedEventHandler> _handlers = new();

        private static void onContentControlLoaded(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContentControl contentControl)
            {
                if (e.OldValue != null && _handlers.TryGetValue(contentControl, out var oldHandler))
                {
                    contentControl.Loaded -= oldHandler;
                    _handlers.Remove(contentControl);
                }

                if (e.NewValue != null)
                {
                    RoutedEventHandler handler = (s, args) => reportLoaded(contentControl);
                    contentControl.Loaded += handler;
                    _handlers.Add(contentControl, handler);
                }
            }
        }

        private static void reportLoaded(ContentControl contentControl)
        {
            var callback = GetContentControlLoadedCallback(contentControl);
            if (callback != null )
            {
                callback(contentControl);
            }
        }
        #endregion
    }
}
