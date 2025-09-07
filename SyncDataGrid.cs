// 1. Create an attached behavior for column width synchronization
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

public static class DataGridSyncBehavior
{
    private static readonly Dictionary<string, List<DataGrid>> _syncGroups = new();

    #region SyncGroup Attached Property
    public static readonly DependencyProperty SyncGroupProperty =
        DependencyProperty.RegisterAttached(
            "SyncGroup",
            typeof(string),
            typeof(DataGridSyncBehavior),
            new PropertyMetadata(null, OnSyncGroupChanged));

    public static void SetSyncGroup(DependencyObject obj, string value)
    {
        obj.SetValue(SyncGroupProperty, value);
    }

    public static string GetSyncGroup(DependencyObject obj)
    {
        return (string)obj.GetValue(SyncGroupProperty);
    }
    #endregion

    private static void OnSyncGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid dataGrid)
        {
            string oldGroup = e.OldValue as string;
            string newGroup = e.NewValue as string;

            // Remove from old group
            if (!string.IsNullOrEmpty(oldGroup) && _syncGroups.ContainsKey(oldGroup))
            {
                _syncGroups[oldGroup].Remove(dataGrid);
                if (_syncGroups[oldGroup].Count == 0)
                    _syncGroups.Remove(oldGroup);
            }

            // Add to new group
            if (!string.IsNullOrEmpty(newGroup))
            {
                if (!_syncGroups.ContainsKey(newGroup))
                    _syncGroups[newGroup] = new List<DataGrid>();

                _syncGroups[newGroup].Add(dataGrid);
                
                // Subscribe to column width changes
                dataGrid.Loaded += OnDataGridLoaded;
                dataGrid.Unloaded += OnDataGridUnloaded;
            }
        }
    }

    private static void OnDataGridLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            SubscribeToColumnWidthChanges(dataGrid);
        }
    }

    private static void OnDataGridUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            UnsubscribeFromColumnWidthChanges(dataGrid);
        }
    }

    private static void SubscribeToColumnWidthChanges(DataGrid dataGrid)
    {
        foreach (var column in dataGrid.Columns)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                DataGridColumn.WidthProperty, 
                typeof(DataGridColumn));
            descriptor?.AddValueChanged(column, OnColumnWidthChanged);
        }
    }

    private static void UnsubscribeFromColumnWidthChanges(DataGrid dataGrid)
    {
        foreach (var column in dataGrid.Columns)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(
                DataGridColumn.WidthProperty, 
                typeof(DataGridColumn));
            descriptor?.RemoveValueChanged(column, OnColumnWidthChanged);
        }
    }

    private static void OnColumnWidthChanged(object sender, EventArgs e)
    {
        if (sender is DataGridColumn changedColumn)
        {
            var dataGrid = FindParentDataGrid(changedColumn);
            if (dataGrid == null) return;

            string syncGroup = GetSyncGroup(dataGrid);
            if (string.IsNullOrEmpty(syncGroup) || !_syncGroups.ContainsKey(syncGroup))
                return;

            int columnIndex = dataGrid.Columns.IndexOf(changedColumn);
            if (columnIndex < 0) return;

            // Update other DataGrids in the same sync group
            foreach (var otherGrid in _syncGroups[syncGroup])
            {
                if (otherGrid != dataGrid && otherGrid.Columns.Count > columnIndex)
                {
                    // Temporarily unsubscribe to avoid infinite loop
                    var descriptor = DependencyPropertyDescriptor.FromProperty(
                        DataGridColumn.WidthProperty, 
                        typeof(DataGridColumn));
                    
                    var targetColumn = otherGrid.Columns[columnIndex];
                    descriptor?.RemoveValueChanged(targetColumn, OnColumnWidthChanged);
                    
                    // Sync the width
                    targetColumn.Width = changedColumn.Width;
                    
                    // Re-subscribe
                    descriptor?.AddValueChanged(targetColumn, OnColumnWidthChanged);
                }
            }
        }
    }

    private static DataGrid FindParentDataGrid(DataGridColumn column)
    {
        // Find the DataGrid that contains this column
        return Application.Current.Windows
            .OfType<Window>()
            .SelectMany(w => FindDataGrids(w))
            .FirstOrDefault(dg => dg.Columns.Contains(column));
    }

    private static IEnumerable<DataGrid> FindDataGrids(DependencyObject parent)
    {
        var result = new List<DataGrid>();
        
        if (parent is DataGrid dataGrid)
        {
            result.Add(dataGrid);
        }

        for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
            result.AddRange(FindDataGrids(child));
        }

        return result;
    }
}

// 2. Alternative approach using ViewModel with ObservableCollection of column widths
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ColumnWidthViewModel : INotifyPropertyChanged
{
    private DataGridLength _width;
    
    public DataGridLength Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ColumnWidthViewModel> ColumnWidths { get; }
    public ObservableCollection<DataItem> DataGrid1Items { get; }
    public ObservableCollection<DataItem> DataGrid2Items { get; }

    public MainViewModel()
    {
        // Initialize with default column widths
        ColumnWidths = new ObservableCollection<ColumnWidthViewModel>
        {
            new ColumnWidthViewModel { Width = new DataGridLength(100) },
            new ColumnWidthViewModel { Width = new DataGridLength(150) },
            new ColumnWidthViewModel { Width = new DataGridLength(200) }
        };

        // Sample data
        DataGrid1Items = new ObservableCollection<DataItem>
        {
            new DataItem { Column1 = "A1", Column2 = "B1", Column3 = "C1" },
            new DataItem { Column1 = "A2", Column2 = "B2", Column3 = "C2" }
        };

        DataGrid2Items = new ObservableCollection<DataItem>
        {
            new DataItem { Column1 = "X1", Column2 = "Y1", Column3 = "Z1" },
            new DataItem { Column1 = "X2", Column2 = "Y2", Column3 = "Z2" }
        };
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class DataItem
{
    public string Column1 { get; set; }
    public string Column2 { get; set; }
    public string Column3 { get; set; }
}
