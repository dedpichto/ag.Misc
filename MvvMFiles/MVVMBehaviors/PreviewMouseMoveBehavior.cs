using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ShayCommon.Mvvm.Behaviors
{
    public class PreviewMouseMoveBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PreviewMouseMoveBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(PreviewMouseMoveBehavior), new PropertyMetadata(null));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseMove += onPreviewMouseMove;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseMove -= onPreviewMouseMove;
        }

        private void onPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                if (AssociatedObject is DataGrid dataGrid)
                {
                    var mousePosition = e.GetPosition(dataGrid);
                    var hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePosition);
                    if (hitTestResult != null)
                    {
                        var cell = findAncestor<DataGridCell>(hitTestResult.VisualHit);
                        if (cell == null)
                            return;
                    }
                }
                var parameter =(e.LeftButton, CommandParameter);
                if (Command.CanExecute(parameter))
                    Command.Execute(parameter);
            }
        }

        private static T findAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
