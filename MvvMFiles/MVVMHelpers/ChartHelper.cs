using ag.WPF.Chart;
using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShayCommon.Mvvm.Helpers
{
    public class ChartConfigBehavior : Behavior<Chart>
    {
        public static readonly DependencyProperty ConfigureProperty =
            DependencyProperty.Register(nameof(Configure), typeof(Action<Chart>),
                typeof(ChartConfigBehavior),
                new PropertyMetadata(null, onConfigureChanged));

        public Action<Chart> Configure
        {
            get => (Action<Chart>)GetValue(ConfigureProperty);
            set => SetValue(ConfigureProperty, value);
        }

        private static void onConfigureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChartConfigBehavior b && e.NewValue is Action<Chart> action)
                action(b.AssociatedObject);
        }
    }

    public class SaveChartBehavior : Behavior<Chart>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += onChartLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= onChartLoaded;
        }

        private void onChartLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = AssociatedObject.DataContext as ChartBaseViewModel;
            if (viewModel == null)
            {
                var rootView = findRootView(AssociatedObject);
                if (rootView != null)
                    viewModel = rootView.DataContext as ChartBaseViewModel;
            }
            if (viewModel != null && AssociatedObject.Tag is Enum extTag)
            {
                viewModel.RegisterSaveChartAction(extTag, (fileName) =>
                {
                    AssociatedObject.SaveAsImage(fileName);
                });
            }
            if (viewModel != null && AssociatedObject.Tag is Guid guid)
            {
                viewModel.RegisterSaveChartAction(guid, (fileName) =>
                {
                    AssociatedObject.SaveAsImage(fileName);
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
