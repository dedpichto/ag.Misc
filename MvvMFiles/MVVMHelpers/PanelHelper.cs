using BLL.WPF.StandardStyles;
using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using ShayCommon;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    public class PanelBehavior:Behavior<Panel>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += onPanelLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= onPanelLoaded;
            base.OnDetaching();
        }

        private void onPanelLoaded(object sender, RoutedEventArgs e)
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
                viewModel.RegisterAddPanelChildAction(extTag, (child) =>
                {
                    AssociatedObject.Children.Add(child);
                });
                viewModel.RegisterRemovePanelChildAction(extTag, (child) =>
                {
                    AssociatedObject.Children.Remove(child);
                });
                viewModel.RegisterClearPanelChildrenAction(extTag, () =>
                {
                    AssociatedObject.Children.Clear();
                });
            }
            else if (viewModel != null && AssociatedObject.Tag is Guid guid)
            {
                viewModel.RegisterAddPanelChildAction(guid, (child) =>
                {
                    AssociatedObject.Children.Add(child);
                });
                viewModel.RegisterRemovePanelChildAction(guid, (child) =>
                {
                    AssociatedObject.Children.Remove(child);
                });
                viewModel.RegisterClearPanelChildrenAction(guid, () =>
                {
                    AssociatedObject.Children.Clear();
                });
            }
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
