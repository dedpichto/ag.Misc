using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class PopUpWithToggleButtonBehavior : Behavior<Popup>
    {
        public static readonly DependencyProperty CommandOpenProperty =
           DependencyProperty.Register(nameof(CommandOpen), typeof(ICommand), typeof(PopUpWithToggleButtonBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandCloseProperty =
           DependencyProperty.Register(nameof(CommandClose), typeof(ICommand), typeof(PopUpWithToggleButtonBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(PopUpWithToggleButtonBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty ToggleButtonProperty =
           DependencyProperty.Register(nameof(ToggleButton), typeof(ToggleButton), typeof(PopUpWithToggleButtonBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Opened += onPopUpOpened;
            AssociatedObject.Closed += onPopUpClosed;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Opened -= onPopUpOpened;
            AssociatedObject.Closed -= onPopUpClosed;
        }
        public ICommand CommandOpen
        {
            get => (ICommand)GetValue(CommandOpenProperty);
            set => SetValue(CommandOpenProperty, value);
        }
        public ICommand CommandClose
        {
            get => (ICommand)GetValue(CommandCloseProperty);
            set => SetValue(CommandCloseProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public ToggleButton ToggleButton
        {
            get => (ToggleButton)GetValue(ToggleButtonProperty);
            set => SetValue(ToggleButtonProperty, value);
        }

        private void onPopUpOpened(object sender, System.EventArgs e)
        {
            if (CommandOpen != null)
            {
                CommandParameter = AssociatedObject.Child;
                if (CommandOpen.CanExecute(CommandParameter))
                    CommandOpen.Execute(CommandParameter);
            }
        }

        private void onPopUpClosed(object sender, System.EventArgs e)
        {
            if (ToggleButton != null)
                ToggleButton.IsChecked = false;
        }
    }
}
