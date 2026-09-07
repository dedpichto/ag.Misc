using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class PreviewKeyUpBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PreviewKeyUpBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(PreviewKeyUpBehavior), new PropertyMetadata(null));
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
            AssociatedObject.PreviewKeyUp += onPreviewKeyUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyUp -= onPreviewKeyUp;
        }

        private void onPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                var parameter = new Tuple<Key, ModifierKeys, Action<bool>, object>(e.Key, e.KeyboardDevice.Modifiers, handled => e.Handled = handled, CommandParameter);
                Command.Execute(parameter);
            }
        }
    }
}
