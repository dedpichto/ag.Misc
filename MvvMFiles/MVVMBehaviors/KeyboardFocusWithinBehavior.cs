using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class KeyboardFocusWithinBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(KeyboardFocusWithinBehavior), new PropertyMetadata(null));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsKeyboardFocusWithinChanged += onKeyboardFocusWithinChanged;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.IsKeyboardFocusWithinChanged -= onKeyboardFocusWithinChanged;
        }

        private void onKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Command == null) return;
            var parameters = new KeyboardFocusWithinChangedParameters
            {
                Sender = sender,
                Args = e
            };
            if (Command.CanExecute(parameters))
            {
                Command.Execute(parameters);
            }
        }
    }

    public class KeyboardFocusWithinChangedParameters
    {
        public object Sender { get; set; }
        public DependencyPropertyChangedEventArgs Args { get; set; }
    }
}
