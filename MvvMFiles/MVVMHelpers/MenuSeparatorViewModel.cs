using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ShayCommon.Mvvm.Helpers
{
    public class MenuSeparatorViewModel : IMenuItemBase, INotifyPropertyChanged
    {
        private Visibility _visibility = Visibility.Visible;

        public Visibility Visibility
        {
            get => _visibility;
            set { if (_visibility == value) return; _visibility = value; OnPropertyChanged(); }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}

