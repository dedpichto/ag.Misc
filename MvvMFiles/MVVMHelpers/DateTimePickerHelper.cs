using BLL.WPF.DateTimePicker;
using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    public class DateTimePickerBehavior : Behavior<DateTimePicker>
    {
        private RoutedEventHandler _loadedHandler;

        protected override void OnAttached()
        {
            base.OnAttached();
            _loadedHandler = async (sender, e) => await Dispatcher.InvokeAsync(() =>
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
                    viewModel.RegisterRefreshDateStatusAction(extTag, () =>
                    {
                        AssociatedObject.RefreshStatus();
                    });
                }
                else if (viewModel != null && AssociatedObject.Tag is Guid guid)
                {
                    viewModel.RegisterRefreshDateStatusAction(guid, () =>
                    {
                        AssociatedObject.RefreshStatus();
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
}
