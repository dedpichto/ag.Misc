using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class ComboTest : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> Currencies { get; } = new();
        private string _selectedCurrency;
        private bool _isStarted;
        private bool _isPreventingChange;

        public ComboTest()
        {
            InitializeComponent();
        }

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (_selectedCurrency == value) return;
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Currencies.Add("USD");
            Currencies.Add("ILS");
            Currencies.Add("GBP");
            Currencies.Add("AUD");
            Currencies.Add("JPY");
            SelectedCurrency = Currencies[0];
            _isStarted = true;
        }

        //private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        //{
        //    if (_isPreventingChange) return;

        //    var comboBox = sender as System.Windows.Controls.ComboBox;

        //    if (_isStarted && !ConfirmSelectionChange())
        //    {
        //        _isPreventingChange = true;
        //        comboBox.SelectedItem = _selectedCurrency;
        //        _isPreventingChange = false;
        //    }
        //}

        private bool _isSelectionChangeHandled = false;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSelectionChangeHandled)
            {
                _isSelectionChangeHandled = false;
                return;
            }

            if (_isStarted && !ConfirmSelectionChange())
            {
                _isSelectionChangeHandled = true;

                // Revert selection
                var comboBox = sender as ComboBox;
                comboBox.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;

                // Prevent selection from changing in the ViewModel
                var viewModel = DataContext as ComboTest;
                if (viewModel != null)
                {
                    viewModel.SelectedCurrency = comboBox.SelectedItem as string;
                }
            }
        }

        private bool ConfirmSelectionChange()
        {
            return MessageBox.Show("Continue?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
