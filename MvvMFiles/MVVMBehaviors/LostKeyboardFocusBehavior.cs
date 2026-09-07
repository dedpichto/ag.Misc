using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class LostKeyboardFocusBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(LostKeyboardFocusBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(LostKeyboardFocusBehavior), new PropertyMetadata(null));
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
            AssociatedObject.LostKeyboardFocus += onLostKeyboardFocus;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.LostKeyboardFocus -= onLostKeyboardFocus;
        }

        private void onLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
