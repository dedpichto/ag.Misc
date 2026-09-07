using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace ShayCommon.Mvvm.Behaviors
{
    public class MousePreviewButtonBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(MousePreviewButtonBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(MousePreviewButtonBehavior), new PropertyMetadata(null));
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
            AssociatedObject.PreviewMouseRightButtonDown += onMouseButtonAction;
            AssociatedObject.PreviewMouseRightButtonUp += onMouseButtonAction;
            //AssociatedObject.MouseRightButtonDown += onMouseButtonAction;
            //AssociatedObject.MouseRightButtonUp += onMouseButtonAction;
            AssociatedObject.PreviewMouseLeftButtonDown += onMouseButtonAction;
            AssociatedObject.PreviewMouseLeftButtonUp += onMouseButtonAction;
            //AssociatedObject.MouseLeftButtonDown += onMouseButtonAction;
            //AssociatedObject.MouseLeftButtonUp += onMouseButtonAction;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseRightButtonDown -= onMouseButtonAction;
            AssociatedObject.PreviewMouseRightButtonUp -= onMouseButtonAction;
            //AssociatedObject.MouseRightButtonDown -= onMouseButtonAction;
            //AssociatedObject.MouseRightButtonUp -= onMouseButtonAction;
            AssociatedObject.PreviewMouseLeftButtonDown -= onMouseButtonAction;
            AssociatedObject.PreviewMouseLeftButtonUp -= onMouseButtonAction;
            //AssociatedObject.MouseLeftButtonDown -= onMouseButtonAction;
            //AssociatedObject.MouseLeftButtonUp -= onMouseButtonAction;
        }

        private void onMouseButtonAction(object sender, MouseButtonEventArgs e)
        {
            if (Command == null) return;
            var parameters = new MouseActionParameters { EventArgs = e, Parameter = CommandParameter, Source = AssociatedObject };
            if (Command.CanExecute(parameters))
                Command.Execute(parameters);
        }
    }

    public class MouseActionParameters
    {
        public MouseButtonEventArgs EventArgs { get; set; }
        public object Parameter { get; set; }
        public object Source { get; set; }
    }
}
