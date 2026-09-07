using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShayCommon.Mvvm.Helpers
{
    public static class TabManager
    {
        private static readonly ConditionalWeakTable<FrameworkElement, StrongBox<string>> _controlRegistry = new();
        private static readonly ConditionalWeakTable<FrameworkElement, TabManagerHandlers> _handlers = new();

        private class TabManagerHandlers
        {
            public EventHandler TabOrderChangedHandler;
            public EventHandler TabStopChangedHandler;
        }

        public static readonly DependencyProperty ControlNameProperty =
            DependencyProperty.RegisterAttached("ControlName", typeof(string), typeof(TabManager),
                new PropertyMetadata(onControlNameChanged));

        public static readonly DependencyProperty TabManagerProperty =
            DependencyProperty.RegisterAttached("TabManager", typeof(ITabManager), typeof(TabManager),
                new PropertyMetadata(onTabManagerChanged));

        public static void SetControlName(DependencyObject obj, string value)
            => obj.SetValue(ControlNameProperty, value);

        public static string GetControlName(DependencyObject obj)
            => (string)obj.GetValue(ControlNameProperty);

        public static void SetTabManager(DependencyObject obj, ITabManager value)
            => obj.SetValue(TabManagerProperty, value);

        public static ITabManager GetTabManager(DependencyObject obj)
            => (ITabManager)obj.GetValue(TabManagerProperty);

        private static void onControlNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && e.NewValue is string controlName)
            {
                _controlRegistry.Remove(element);
                _controlRegistry.Add(element, new StrongBox<string>(controlName));

                var tabManager = findTabManager(element);
                if (tabManager != null)
                {
                    updateTabProperties(element, tabManager, controlName);
                }
                else
                {
                    element.Loaded += onElementLoaded;
                }
            }
        }

        private static void onElementLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.Loaded -= onElementLoaded;

                if (_controlRegistry.TryGetValue(element, out var box))
                {
                    var tabManager = findTabManager(element);
                    if (tabManager != null)
                    {
                        updateTabProperties(element, tabManager, box.Value);
                    }
                }
            }
        }


        private static void onTabManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement container)
            {
                if (e.OldValue is ITabManager oldManager && _handlers.TryGetValue(container, out var oldHandlers))
                {
                    oldManager.TabOrderChanged -= oldHandlers.TabOrderChangedHandler;
                    oldManager.TabStopChanged -= oldHandlers.TabStopChangedHandler;
                    _handlers.Remove(container);
                }

                if (e.NewValue is ITabManager newManager)
                {
                    var handlers = new TabManagerHandlers
                    {
                        TabOrderChangedHandler = (s, args) => onTabManagerPropertiesChanged(container),
                        TabStopChangedHandler = (s, args) => onTabManagerPropertiesChanged(container)
                    };

                    newManager.TabOrderChanged += handlers.TabOrderChangedHandler;
                    newManager.TabStopChanged += handlers.TabStopChangedHandler;

                    _handlers.Remove(container);
                    _handlers.Add(container, handlers);

                    updateAllChildControls(container, newManager);
                }
            }
        }

        private static void onTabManagerPropertiesChanged(FrameworkElement container)
        {
            var tabManager = GetTabManager(container);
            if (tabManager != null)
            {
                updateAllChildControls(container, tabManager);
            }
        }

        private static void updateAllChildControls(DependencyObject parent, ITabManager tabManager)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is FrameworkElement element && _controlRegistry.TryGetValue(element, out var box))
                {
                    updateTabProperties(element, tabManager, box.Value);
                }

                updateAllChildControls(child, tabManager);
            }
        }

        private static ITabManager findTabManager(DependencyObject element)
        {
            var current = element;
            while (current != null)
            {
                var tabManager = GetTabManager(current);
                if (tabManager != null)
                    return tabManager;

                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static void updateTabProperties(FrameworkElement element, ITabManager tabManager, string controlName)
        {
            // Update TabIndex
            var tabIndex = tabManager.GetTabIndex(controlName);
            element.SetCurrentValue(Control.TabIndexProperty, tabIndex);

            // Update IsTabStop
            var isTabStop = tabManager.GetIsTabStop(controlName);
            if (isTabStop.HasValue)
                element.SetCurrentValue(Control.IsTabStopProperty, isTabStop.Value);
        }
    }

}
