using HierarchicalListSample;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HierarchicalListSample
{
    public class TreeListViewModel : INotifyPropertyChanged
    {
        //private ObservableCollection<TreeListNode> _rootNodes;
        //private ObservableCollection<TreeListNode> _flattenedNodes;

        //public TreeListViewModel()
        //{
        //    RootNodes = new ObservableCollection<TreeListNode>();
        //    FlattenedNodes = new ObservableCollection<TreeListNode>();

        //    ToggleExpandCommand = new RelayCommand<TreeListNode>(ToggleExpand);

        //    // Sample data
        //    LoadSampleData();
        //}

        private ObservableCollection<TreeListNode> _rootNodes;
        private ObservableCollection<TreeListNode> _flattenedNodes;

        public TreeListViewModel()
        {
            // Initialize backing fields directly, not through properties
            _rootNodes = new ObservableCollection<TreeListNode>();
            _flattenedNodes = new ObservableCollection<TreeListNode>();

            ToggleExpandCommand = new RelayCommand<TreeListNode>(ToggleExpand);

            LoadSampleData();
        }

        public ObservableCollection<TreeListNode> RootNodes
        {
            get => _rootNodes;
            set
            {
                _rootNodes = value;
                OnPropertyChanged();
                RebuildFlattenedList();
            }
        }

        public ObservableCollection<TreeListNode> FlattenedNodes
        {
            get => _flattenedNodes;
            set
            {
                _flattenedNodes = value;
                OnPropertyChanged();
            }
        }

        public ICommand ToggleExpandCommand { get; }

        private void ToggleExpand(TreeListNode node)
        {
            if (!node.HasChildren) return;

            node.IsExpanded = !node.IsExpanded;
            RebuildFlattenedList();
        }

        private void RebuildFlattenedList()
        {
            FlattenedNodes.Clear();
            foreach (var node in FlattenHierarchy(RootNodes))
            {
                FlattenedNodes.Add(node);
            }
        }

        private IEnumerable<TreeListNode> FlattenHierarchy(IEnumerable<TreeListNode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node;

                if (node.IsExpanded && node.HasChildren)
                {
                    foreach (var child in FlattenHierarchy(node.Children))
                    {
                        yield return child;
                    }
                }
            }
        }

        private void LoadSampleData()
        {
            var root1 = new TreeListNode
            {
                Name = "Project 1",
                Description = "Main project",
                ImagePath = "/Images/folder.png",
                IsChecked = true
            };

            var child1 = new TreeListNode(root1)
            {
                Name = "Module A",
                Description = "Core module",
                ImagePath = "/Images/file.png",
                IsChecked = false
            };

            var child2 = new TreeListNode(root1)
            {
                Name = "Module B",
                Description = "Secondary module",
                ImagePath = "/Images/file.png",
                IsChecked = true
            };

            var subChild1 = new TreeListNode(child2)
            {
                Name = "Component 1",
                Description = "Sub-component",
                ImagePath = "/Images/component.png",
                IsChecked = false
            };

            child2.Children.Add(subChild1);
            root1.Children.Add(child1);
            root1.Children.Add(child2);

            var root2 = new TreeListNode
            {
                Name = "Project 2",
                Description = "Secondary project",
                ImagePath = "/Images/folder.png",
                IsChecked = false
            };

            RootNodes.Add(root1);
            RootNodes.Add(root2);

            RebuildFlattenedList();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    } 
}