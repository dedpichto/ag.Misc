using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class ControlNameBehaivior : Behavior<Control>
    {
        public static readonly DependencyProperty CommandProperty =
               DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ControlNameBehaivior), new PropertyMetadata(null));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += controlLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= controlLoaded;
        }

        private void controlLoaded(object sender, RoutedEventArgs e)
        {
            if (Command != null && Command.CanExecute(AssociatedObject.Name))
                Command.Execute(AssociatedObject.Name);
        }

    }
}
