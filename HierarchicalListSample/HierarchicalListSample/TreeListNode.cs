using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

public class TreeListNode : INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool? _isChecked = false;
    private bool _isUpdating; // Prevents infinite recursion

    public TreeListNode(TreeListNode parent = null)
    {
        Parent = parent;
        Children = new ObservableCollection<TreeListNode>();
        Level = parent?.Level + 1 ?? 0;
    }

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

    /// <summary>
    /// Three-state checkbox: true, false, or null (indeterminate)
    /// </summary>
    public bool? IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked != value)
            {
                _isChecked = value;
                OnPropertyChanged();

                if (!_isUpdating)
                {
                    _isUpdating = true;
                    try
                    {
                        // Propagate down to children (only for definite states)
                        if (value.HasValue)
                        {
                            PropagateToChildren(value.Value);
                        }

                        // Propagate up to parent
                        UpdateParentState();
                    }
                    finally
                    {
                        _isUpdating = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets all children to the same checked state
    /// </summary>
    private void PropagateToChildren(bool isChecked)
    {
        foreach (var child in Children)
        {
            child._isUpdating = true;
            try
            {
                child.IsChecked = isChecked;
                // Recursively propagate to grandchildren
                child.PropagateToChildren(isChecked);
            }
            finally
            {
                child._isUpdating = false;
            }
        }
    }

    /// <summary>
    /// Updates parent's state based on children's states
    /// </summary>
    private void UpdateParentState()
    {
        var parent = Parent;
        while (parent != null)
        {
            parent._isUpdating = true;
            try
            {
                parent._isChecked = parent.DetermineStateFromChildren();
                parent.OnPropertyChanged(nameof(IsChecked));
            }
            finally
            {
                parent._isUpdating = false;
            }
            parent = parent.Parent;
        }
    }

    /// <summary>
    /// Determines the checkbox state based on children's states
    /// </summary>
    private bool? DetermineStateFromChildren()
    {
        if (!HasChildren)
            return _isChecked;

        var childStates = GetAllDescendantStates().ToList();

        bool allChecked = childStates.All(s => s == true);
        bool allUnchecked = childStates.All(s => s == false);

        if (allChecked)
            return true;
        if (allUnchecked)
            return false;

        // Mixed state - some checked, some unchecked, or some indeterminate
        return null;
    }

    /// <summary>
    /// Gets the leaf-level states (or node states if no children)
    /// </summary>
    private IEnumerable<bool?> GetAllDescendantStates()
    {
        foreach (var child in Children)
        {
            if (child.HasChildren)
            {
                // For parent nodes, get their calculated state
                foreach (var state in child.GetAllDescendantStates())
                {
                    yield return state;
                }
            }
            else
            {
                // For leaf nodes, return their state
                yield return child.IsChecked;
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