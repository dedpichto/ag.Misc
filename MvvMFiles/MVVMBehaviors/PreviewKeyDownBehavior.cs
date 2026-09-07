using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class PreviewKeyDownBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PreviewKeyDownBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(PreviewKeyDownBehavior), new PropertyMetadata(null));
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
            AssociatedObject.PreviewKeyDown += onPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= onPreviewKeyDown;
        }

        private void onPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                var parameter = new Tuple<Key, ModifierKeys, Action<bool>, object>(e.Key, e.KeyboardDevice.Modifiers, handled => e.Handled = handled, CommandParameter);
                Command.Execute(parameter);
            }
        }
    }
}
