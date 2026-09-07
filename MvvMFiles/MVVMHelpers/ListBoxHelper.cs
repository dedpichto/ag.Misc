using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    public static class ListBoxHelper
    {
        #region Scroll into view
        public static readonly DependencyProperty ScrollIntoViewProperty =
           DependencyProperty.RegisterAttached("ScrollIntoView", typeof(object), typeof(ListBoxHelper), new PropertyMetadata(null, OnScrollIntoViewChanged));

        public static object GetScrollIntoView(DependencyObject obj) => obj.GetValue(ScrollIntoViewProperty);

        public static void SetScrollIntoView(DependencyObject obj, object value) => obj.SetValue(ScrollIntoViewProperty, value);

        private static void OnScrollIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox && e.NewValue != null)
            {
                listBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    listBox.UpdateLayout();
                    listBox.ScrollIntoView(e.NewValue);
                }));
            }
        }
        #endregion
    }

    #region Checked items behavior
    public class ListBoxWithCheckedItemsBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonUp += onListBoxPreviewMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonUp -= onListBoxPreviewMouseLeftButtonUp;
        }

        private void onListBoxPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject.SelectedItem is CheckedItem checkedItem)
            {
                var mousePosition = e.GetPosition(AssociatedObject);
                var hitTestResult = VisualTreeHelper.HitTest(AssociatedObject, mousePosition);
                if (hitTestResult == null || hitTestResult.VisualHit is IScrollInfo)
                    return;
                checkedItem.IsChecked = !checkedItem.IsChecked;
            }
            else if (AssociatedObject.SelectedItem is StackPanel panel)
            {
                var checkedItems = panel.Children.OfType<CheckedItem>().ToList();
                if (checkedItems.Any())
                {
                    checkedItems.First().IsChecked = !checkedItems.First().IsChecked;
                }
            }
        }
    }
    #endregion

    #region Registered behaviors
    public class ListBoxBehavior : Behavior<ListBox>
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
                    viewModel.RegisterScrollIntoAction(extTag, (item) =>
                    {
                        AssociatedObject.UpdateLayout();
                        AssociatedObject.ScrollIntoView(item);
                    });
                }
                else if (viewModel != null && AssociatedObject.Tag is Guid guid)
                {
                    viewModel.RegisterScrollIntoAction(guid, (item) =>
                    {
                        AssociatedObject.UpdateLayout();
                        AssociatedObject.ScrollIntoView(item);
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
    #endregion

    #region Double click
    public class ListBoxDoubleClickBehavior : Behavior<ListBox>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ListBoxDoubleClickBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += onMouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDoubleClick -= onMouseDoubleClick;
        }

        private void onMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject is ListBox listBox)
            {
                var selectedItem = listBox.SelectedItem;

                if (Command != null && Command.CanExecute(selectedItem))
                {
                    Command.Execute(selectedItem);
                }
            }
        }
    }
    #endregion
}
