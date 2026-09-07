using System.Windows;
using System.Windows.Controls;

namespace ShayCommon.Mvvm.Helpers
{
    public static class PasswordBoxHelper
    {
        private static readonly DependencyProperty _isUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating",
                typeof(bool),
                typeof(PasswordBoxHelper));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

        public static string GetPassword(DependencyObject obj) => (string)obj.GetValue(PasswordProperty);

        public static void SetPassword(DependencyObject obj, string value) => obj.SetValue(PasswordProperty, value);

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                passwordBox.PasswordChanged -= passwordBox_PasswordChanged;
                passwordBox.PasswordChanged += passwordBox_PasswordChanged;

                if (!(bool)passwordBox.GetValue(_isUpdatingProperty))
                {
                    passwordBox.Password = (string)e.NewValue ?? string.Empty;
                }
            }
        }

        private static void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                passwordBox.SetValue(_isUpdatingProperty, true);
                SetPassword(passwordBox, passwordBox.Password);
                passwordBox.SetValue(_isUpdatingProperty, false);
            }
        }

    }
}
