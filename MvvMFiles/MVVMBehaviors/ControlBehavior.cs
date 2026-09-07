using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class ControlBehavior : Behavior<Control>
    {
        private bool _isLoaded;
        private bool _hasExecuted;

        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ControlBehavior), new PropertyMetadata(null, onCommandPropertyChanged));
        public static readonly DependencyProperty EnabledCommandProperty =
           DependencyProperty.Register(nameof(EnabledCommand), typeof(ICommand), typeof(ControlBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ControlBehavior), new PropertyMetadata(null, onCommandPropertyChanged));
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public ICommand EnabledCommand
        {
            get => (ICommand)GetValue(EnabledCommandProperty);
            set => SetValue(EnabledCommandProperty, value);
        }
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += onLoaded;
            AssociatedObject.IsEnabledChanged += onIsEnabledChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= onLoaded;
            AssociatedObject.IsEnabledChanged -= onIsEnabledChanged;
        }

        private void onLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            tryExecuteCommand();
        }

        private static void onCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ControlBehavior)d).tryExecuteCommand();
        }

        private void tryExecuteCommand()
        {
            if (_isLoaded && !_hasExecuted && Command != null && CommandParameter != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
                _hasExecuted = true;
            }
        }

        private void onIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var parameters = new EnabledChangedParameters
            {
                Sender = sender,
                IsEnabled = (bool)e.NewValue
            };
            if (EnabledCommand != null && EnabledCommand.CanExecute(parameters))
            {
                EnabledCommand.Execute(parameters);
            }
        }
    }

    public class EnabledChangedParameters
    {
        public object Sender { get; set; }
        public bool IsEnabled { get; set; }
    }
}
