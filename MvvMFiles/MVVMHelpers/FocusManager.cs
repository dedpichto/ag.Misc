using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    // Focus Manager Attached Behavior
    public static class FocusManager
    {
        // Attached property for registering focus targets
        public static readonly DependencyProperty FocusTargetProperty =
            DependencyProperty.RegisterAttached(
                "FocusTarget",
                typeof(string),
                typeof(FocusManager),
                new PropertyMetadata(null, onFocusTargetChanged));
        // Attached property for the focus command
        public static readonly DependencyProperty FocusCommandProperty =
            DependencyProperty.RegisterAttached(
                "FocusCommand",
                typeof(ICommand),
                typeof(FocusManager),
                new PropertyMetadata(null, onFocusCommandChanged));

        // Dictionary to store weak references to focus targets
        private static readonly Dictionary<string, WeakReference> _focusTargets = new();
        // Getter for FocusTarget attached property
        public static string GetFocusTarget(DependencyObject obj) => (string)obj.GetValue(FocusTargetProperty);
        // Setter for FocusTarget attached property
        public static void SetFocusTarget(DependencyObject obj, string value) => obj.SetValue(FocusTargetProperty, value);
        // Getter for FocusCommand attached property
        public static ICommand GetFocusCommand(DependencyObject obj) => (ICommand)obj.GetValue(FocusCommandProperty);
        // Setter for FocusCommand attached property
        public static void SetFocusCommand(DependencyObject obj, ICommand value) => obj.SetValue(FocusCommandProperty, value);

        // Callback when FocusTarget property changes
        private static void onFocusTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && e.NewValue is string targetName && !string.IsNullOrEmpty(targetName))
            {
                _focusTargets[targetName] = new WeakReference(element);
                cleanupDeadReferences();
            }
        }

        private static void cleanupDeadReferences()
        {
            var deadKeys = _focusTargets.Where(kvp => !kvp.Value.IsAlive).Select(kvp => kvp.Key).ToList();
            foreach (var key in deadKeys)
                _focusTargets.Remove(key);
        }

        // Callback when FocusCommand property changes
        private static void onFocusCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Loaded -= onElementLoaded;

                if (e.NewValue is ICommand newCommand)
                {
                    element.Loaded += onElementLoaded;
                    if (element.IsLoaded)
                    {
                        setupFocusCommand(element, newCommand);
                    }
                }
            }
        }

        // Handle element loaded event
        private static void onElementLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var command = GetFocusCommand(element);
                if (command != null)
                {
                    setupFocusCommand(element, command);
                }
            }
        }
        // Setup the focus command with the actual focus action
        private static void setupFocusCommand(FrameworkElement element, ICommand command)
        {
            if (command is FocusCommand focusCommand)
            {
                focusCommand.SetFocusAction((targetName) =>
                {
                    if (_focusTargets.TryGetValue(targetName, out var weakRef) &&
                        weakRef.Target is UIElement targetElement)
                    {
                        // Use dispatcher to ensure focus is set on UI thread
                        targetElement.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            targetElement.Focus();
                            // Special handling for TextBox to select all text
                            if (targetElement is TextBox textBox)
                            {
                                textBox.SelectAll();
                            }
                        }), DispatcherPriority.Input);
                    }
                });
            }
        }
    }

    // Custom command implementation for focus management
    public class FocusCommand : ICommand
    {
        private Action<string> _focusAction;
        // Set the action that will be called when Execute is invoked
        public void SetFocusAction(Action<string> focusAction) => _focusAction = focusAction;
        // Always return true - focus commands should always be executable
        public bool CanExecute(object parameter) => true;
        // Execute the focus action with the provided target name
        public void Execute(object parameter)
        {
            if (parameter is string targetName && !string.IsNullOrEmpty(targetName))
            {
                _focusAction?.Invoke(targetName);
            }
        }
        // Event required by ICommand interface
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
