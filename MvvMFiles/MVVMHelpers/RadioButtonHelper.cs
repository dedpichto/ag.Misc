using Microsoft.Xaml.Behaviors;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ShayCommon.Mvvm.Helpers
{
    public static class RadioButtonHelper
    {
        public static readonly DependencyProperty CheckByTagProperty =
            DependencyProperty.RegisterAttached("CheckByTag",
                typeof(object),
                typeof(RadioButtonHelper),
                new PropertyMetadata(null, onCheckByTagChanged));

        public static object GetCheckByTag(DependencyObject obj)
        {
            return obj.GetValue(CheckByTagProperty);
        }
        public static void SetCheckByTag(DependencyObject obj, object value)
        {
            obj.SetValue(CheckByTagProperty, value);
        }
        private static void onCheckByTagChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Grid grid && e.NewValue != null)
            {
                var radioButton = findVisualChildren<RadioButton>(grid).FirstOrDefault(rb => Equals(rb.Tag, e.NewValue));
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }

        private static IEnumerable<T> findVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    var child = VisualTreeHelper.GetChild(obj, i);
                    if (child != null && child is T)
                        yield return (T)child;

                    foreach (T childOfChild in findVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }
    }

    public class RadioButtonCheckedBehavior : Behavior<RadioButton>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(RadioButtonCheckedBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Checked += onRadioButtonCheckedChanged;
            AssociatedObject.Unchecked += onRadioButtonCheckedChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Checked -= onRadioButtonCheckedChanged;
            AssociatedObject.Unchecked -= onRadioButtonCheckedChanged;
        }
        private void onRadioButtonCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject is RadioButton radioButton)
            {
                var parameters = new RadioButtonCheckedParameters { Tag = radioButton.Tag, IsChecked = radioButton.IsChecked };
                if (Command != null && Command.CanExecute(parameters))
                {
                    Command.Execute(parameters);
                }
            }
        }
    }

    public class RadioButtonCheckedParameters
    {
        public object Tag { get; set; }
        public bool? IsChecked { get; set; }
    }
}
