using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class DropBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DropBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(DropBehavior), new PropertyMetadata(null));
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
            AssociatedObject.Drop += onDrop;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Drop -= onDrop;
        }

        private void onDrop(object sender, DragEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                var parameter = (e.Data, CommandParameter);
                if (Command.CanExecute(parameter))
                    Command.Execute(parameter);
            }
        }
    }
}
