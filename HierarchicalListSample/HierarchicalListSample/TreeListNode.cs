using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HierarchicalListSample
{
    public class TreeListNode : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _isChecked;

        public TreeListNode(TreeListNode parent = null)
        {
            Parent = parent;
            Children = new ObservableCollection<TreeListNode>();
            Level = parent?.Level + 1 ?? 0;
        }

        // Reference to parent (no circular ref - parent doesn't store children directly)
        public TreeListNode Parent { get; }

        public int Level { get; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        public ObservableCollection<TreeListNode> Children { get; }

        public bool HasChildren => Children.Count > 0;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ExpanderIcon));
                }
            }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ExpanderIcon => IsExpanded ? "▼" : "►";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    } 
}