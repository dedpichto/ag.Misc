using ShayCommon.Mvvm.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace ShayCommon.Mvvm.Helpers
{
    public class MenuItemViewModel : DependencyObject, IMenuItemBase, INotifyPropertyChanged
    {
        public static readonly DependencyProperty IscCheckableProperty =
            DependencyProperty.Register(nameof(IsCheckable), typeof(bool), typeof(MenuItemViewModel), new PropertyMetadata(false));
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(MenuItemViewModel), new PropertyMetadata(false));

        private Visibility _visibility = Visibility.Visible;
        private string _header;
        private bool _isEnabled = true;
        private IUICommand _command;
        private string _inputGestureText;
        private DrawingImage _iconSource;

        public ObservableCollection<IMenuItemBase> Children { get; } = new();

        public Visibility Visibility
        {
            get => _visibility;
            set { if (_visibility == value) return; _visibility = value; OnPropertyChanged(); }
        }
        public string Header
        {
            get => string.IsNullOrEmpty(_header) ? _command?.Text : _header;
            set { if (_header == value) return; _header = value; OnPropertyChanged(); }
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set { if (_isEnabled == value) return; _isEnabled = value; OnPropertyChanged(); }
        }
        public IUICommand Command
        {
            get => _command;
            set
            {
                if (_command == value) return; _command = value;
                if (_command == null) return;
                if (_command.HotKey != null)
                {
                    if (!string.IsNullOrEmpty(_command.HotKey.DisplayString))
                    {
                        InputGestureText = _command.HotKey.DisplayString;
                    }
                    else
                    {
                        var mod = "";
                        if ((_command.HotKey.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                            mod = $"{mod}Alt+";
                        if ((_command.HotKey.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                            mod = $"{mod}Ctrl+";
                        if ((_command.HotKey.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                            mod = $"{mod}Shift+";
                        if ((_command.HotKey.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
                            mod = $"{mod}Win+";
                        InputGestureText = $"{mod}{_command.HotKey?.Key}";
                    }
                }

                OnPropertyChanged();
            }
        }

        public string InputGestureText
        {
            get => _inputGestureText;
            set { if (_inputGestureText == value) return; _inputGestureText = value; OnPropertyChanged(); }
        }

        public System.Windows.Controls.Image Icon => IconsFactory.CreateIcon(_iconSource);

        public DrawingImage IconSource
        {
            set { if (_iconSource == value) return; _iconSource = value; OnPropertyChanged(); }
        }

        public bool IsCheckable
        {
            get => (bool)GetValue(IscCheckableProperty);
            set => SetValue(IscCheckableProperty, value);
        }
        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
