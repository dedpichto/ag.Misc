using System.Windows;

namespace ShayCommon.Mvvm.Helpers
{
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new FrameworkPropertyMetadata(null));
        protected override Freezable CreateInstanceCore() => new BindingProxy();

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
    }
}
