using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    public static class SelectorHelper
    {
        #region Get tag
        public static readonly DependencyProperty TagCallbackProperty =
            DependencyProperty.RegisterAttached("TagCallback",
                typeof(Action<object>),
                typeof(SelectorHelper),
                new PropertyMetadata(null, onTagCallbackChanged));

        public static Action<object> GetTagCallback(DependencyObject obj) => (Action<object>)obj.GetValue(TagCallbackProperty);

        public static void SetTagCallback(DependencyObject obj, Action<object> value) => obj.SetValue(TagCallbackProperty, value);

        private static void onTagCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Selector selector)
            {
                if (e.OldValue != null)
                    selector.Loaded -= onSelectorTagLoaded;
                if (e.NewValue != null)
                    selector.Loaded += onSelectorTagLoaded;
            }
        }

        private static void onSelectorTagLoaded(object s, RoutedEventArgs args)
        {
            if (s is Selector selector) reportTag(selector);
        }

        private static void reportTag(Selector selector)
        {
            var callback = GetTagCallback(selector);
            if (callback != null)
            {
                callback(selector.Tag);
            }
        }
        #endregion
    }

    public class SelectorSelectedItemBevavior : Behavior<Selector>
    {
        private RoutedEventHandler _loadedHandler;

        protected override void OnAttached()
        {
            base.OnAttached();
            _loadedHandler=async(sender,e) => await Dispatcher.InvokeAsync(() =>
            {
                var viewModel = AssociatedObject.DataContext as BaseViewModelWithActions;
                if (viewModel == null)
                {
                    var rootView = findRootView(AssociatedObject);
                    if (rootView != null)
                        viewModel = rootView.DataContext as BaseViewModelWithActions;
                }
                if (viewModel != null && AssociatedObject.Tag is Enum extTag)
                {
                    viewModel.RegisterSetSelectedItemAction(extTag, (item) => AssociatedObject.SelectedItem = item);
                    viewModel.RegisterSetSelectedIndexAction(extTag, (index) => AssociatedObject.SelectedIndex = index);
                    viewModel.RegisterGetSelectedItemFunction(extTag, () => AssociatedObject.SelectedItem);
                    viewModel.RegisterSetItemsSourceAction(extTag, (source) =>
                    {
                        AssociatedObject.ItemsSource = source;
                    });
                }
                else if (viewModel != null && AssociatedObject.Tag is Guid guid)
                {
                    viewModel.RegisterSetSelectedItemAction(guid, (item) => AssociatedObject.SelectedItem = item);
                    viewModel.RegisterSetSelectedIndexAction(guid, (index) => AssociatedObject.SelectedIndex = index);
                    viewModel.RegisterGetSelectedItemFunction(guid, () => AssociatedObject.SelectedItem);
                    viewModel.RegisterSetItemsSourceAction(guid, (source) =>
                    {
                        AssociatedObject.ItemsSource = source;
                    });
                }
            }, DispatcherPriority.DataBind);
            AssociatedObject.Loaded += _loadedHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (_loadedHandler != null)
                AssociatedObject.Loaded -= _loadedHandler;
        }

        private static FrameworkElement findRootView(DependencyObject element)
        {
            while (element != null)
            {
                if (element is Window || element is UserControl)
                    return element as FrameworkElement;

                if (element is FrameworkElement frameworkElement && frameworkElement.TemplatedParent != null)
                {
                    element = frameworkElement.TemplatedParent;
                }
                else
                {
                    element = LogicalTreeHelper.GetParent(element) ?? VisualTreeHelper.GetParent(element);
                }
            }
            return null;
        }
    }

    public class SelectorSelectionChangedBehavior : Behavior<Selector>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SelectorSelectionChangedBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += onSelectorSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= onSelectorSelectionChanged;
        }
        private void onSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject is Selector)
            {
                var parameter = new SelectorSelectionChangedParameters { Tag = AssociatedObject.Tag };
                foreach (var item in e.RemovedItems)
                    parameter.RemovedItems.Add(item);
                foreach (var item in e.AddedItems)
                    parameter.AddedItems.Add(item);
                parameter.BindingExpression = AssociatedObject.GetBindingExpression(Selector.SelectedItemProperty);
                if (Command != null && Command.CanExecute(parameter))
                {
                    Command.Execute(parameter);
                }
            }
        }
    }

    public class SelectorSelectionChangedParameters
    {
        public List<object> RemovedItems { get; set; } = new();
        public List<object> AddedItems { get; set; } = new();
        public object Tag { get; set; }
        public BindingExpression BindingExpression { get; set; }
    }
}
