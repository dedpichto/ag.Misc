using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class LostFocusBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(LostFocusBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(LostFocusBehavior), new PropertyMetadata(null));
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
            AssociatedObject.LostFocus += onLostFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.LostFocus -= onLostFocus;
        }

        private void onLostFocus(object sender, RoutedEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
