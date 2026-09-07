using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace ShayCommon.Mvvm.Helpers
{

    public class TreeItemViewModel : INotifyPropertyChanged
    {
#nullable enable
        public event EventHandler? Expanded;
        public event EventHandler? Collapsed;
#nullable disable
        private DrawingImage _iconSource;
        private string _header;
        private object _tag;
        private bool _isExpanded;

        protected virtual void OnExpanded() => Expanded?.Invoke(this, EventArgs.Empty);
        protected virtual void OnCollapsed() => Collapsed?.Invoke(this, EventArgs.Empty);

        public ObservableCollection<TreeItemViewModel> Children { get; set; } = new();

        public string Header
        {
            get => _header;
            set { if (_header == value) return; _header = value; OnPropertyChanged(); }
        }

        public DrawingImage IconSource
        {
            get => _iconSource;
            set { if (_iconSource == value) return; _iconSource = value; OnPropertyChanged(); }
        }

        public object Tag { get => _tag; set => _tag = value; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return; _isExpanded = value; OnPropertyChanged();
                if (_isExpanded)
                {
                    if (Children.Count == 1 && Children[0].Header == "dummy")
                        Children.Clear();
                    OnExpanded();
                }
                else
                    OnCollapsed();
            }
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
