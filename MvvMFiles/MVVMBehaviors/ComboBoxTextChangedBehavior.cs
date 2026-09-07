using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ShayCommon.Mvvm.Behaviors
{
    public class ComboBoxTextChangedBehavior : Behavior<ComboBox>
    {
        private TextBox editableTextBox;

        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ComboBoxTextChangedBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ComboBoxTextChangedBehavior), new PropertyMetadata(null));
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
            if (AssociatedObject.IsLoaded)
            {
                attachToTextBox();
            }
            else
            {
                AssociatedObject.Loaded += onComboBoxLoaded;
            }
        }

        protected override void OnDetaching()
        {
            if (editableTextBox != null)
            {
                editableTextBox.TextChanged -= onTextBoxChanged;
            }
            AssociatedObject.Loaded -= onComboBoxLoaded;
            base.OnDetaching();
        }

        private void onComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            attachToTextBox();
        }

        private void attachToTextBox()
        {
            editableTextBox = AssociatedObject.Template?.FindName("PART_EditableTextBox", AssociatedObject) as TextBox;
            if (editableTextBox == null)
            {
                AssociatedObject.ApplyTemplate();
                editableTextBox = findVisualChild<TextBox>(AssociatedObject);
            }
            if (editableTextBox != null)
            {
                editableTextBox.TextChanged += onTextBoxChanged;
            }
        }

        private void onTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (Command == null) return;
            if (sender is not TextBox textBox) return;
            var text = textBox.Text;
            if (Command.CanExecute(CommandParameter ?? text))
            {
                CommandParameter = (text, AssociatedObject.Tag);
                Command.Execute(CommandParameter);
            }
        }

        private static T findVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T targetChild)
                    return targetChild;

                var childOfChild = findVisualChild<T>(child);
                if (child != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
