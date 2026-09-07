using Microsoft.Xaml.Behaviors;
using ShayX.UserControls;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class SimpleUpDownBehavior : Behavior<SimpleUpDown>
    {
        public static readonly DependencyProperty UpCommandProperty =
           DependencyProperty.Register(nameof(UpCommand), typeof(ICommand), typeof(SimpleUpDownBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty DownCommandProperty =
           DependencyProperty.Register(nameof(DownCommand), typeof(ICommand), typeof(SimpleUpDownBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SimpleUpDownBehavior), new PropertyMetadata(null));

        public ICommand UpCommand
        {
            get => (ICommand)GetValue(UpCommandProperty);
            set => SetValue(UpCommandProperty, value);
        }

        public ICommand DownCommand
        {
            get => (ICommand)GetValue(DownCommandProperty);
            set => SetValue(DownCommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.DownClicked += onDownClicked;
            AssociatedObject.UpClicked += onUpClicked;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.DownClicked -= onDownClicked;
            AssociatedObject.UpClicked -= onUpClicked;
        }

        private void onUpClicked(object sender, System.EventArgs e)
        {
            if (UpCommand != null && UpCommand.CanExecute(CommandParameter))
            {
                UpCommand.Execute(CommandParameter);
            }
        }

        private void onDownClicked(object sender, System.EventArgs e)
        {
            if (DownCommand != null && DownCommand.CanExecute(CommandParameter))
            {
                DownCommand.Execute(CommandParameter);
            }
        }

    }
}
