using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class ActualSizeBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ActualSizeBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += onLoaded;
            AssociatedObject.SizeChanged += onSizeChanged;
            AssociatedObject.IsVisibleChanged += onIsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= onLoaded;
            AssociatedObject.SizeChanged -= onSizeChanged;
            AssociatedObject.IsVisibleChanged -= onIsVisibleChanged;
        }

        private void onIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var size = new Size(AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            if (Command != null && Command.CanExecute(size))
                Command.Execute(size);
        }

        private void onSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var size = new Size(AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            if (Command != null && Command.CanExecute(size))
                Command.Execute(size);
        }

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            var size = new Size(AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            if (Command != null && Command.CanExecute(size))
                Command.Execute(size);
        }
    }
}
