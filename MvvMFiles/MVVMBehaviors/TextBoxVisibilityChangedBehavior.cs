using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace ShayCommon.Mvvm.Behaviors
{
    public class TextBoxVisibilityChangedBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged += onTextBoxVisibleChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.IsVisibleChanged -= onTextBoxVisibleChanged;
        }

        private void onTextBoxVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                AssociatedObject.Focus();
                AssociatedObject.SelectAll();
            }
        }

    }
}
