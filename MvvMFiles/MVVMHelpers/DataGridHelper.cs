using BLL.WPF.StandardStyles;
using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    public static class DataGridHelper
    {
        #region Sync
        public static readonly DependencyProperty ParentDataGridProperty =
            DependencyProperty.RegisterAttached("ParentDataGrid",
                typeof(DataGrid),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onParentDataGridChanged));

        private static List<Type> _columTypes = new();
        private static readonly ConditionalWeakTable<DataGrid, DataGrid> _childToParent = new();

        private class GridPair
        {
            public DataGrid Parent { get; set; }
            public DataGrid Child { get; set; }
        }

        private static readonly ConditionalWeakTable<object, GridPair> _parentColumnOwners = new();

        public static object GetParentDataGrid(DependencyObject obj) => (DataGrid)obj.GetValue(ParentDataGridProperty);

        public static void SetParentDataGrid(DependencyObject obj, DataGrid value) => obj.SetValue(ParentDataGridProperty, value);

        private static void onParentDataGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid childGrid)
                return;

            if (e.OldValue is DataGrid oldParent)
            {
                childGrid.AutoGeneratingColumn -= onChildAutoGeneratingColumn;
                childGrid.Loaded -= onChildGridLoaded;
                var oldColumns = (INotifyCollectionChanged)oldParent.Columns;
                oldColumns.CollectionChanged -= onParentColumnsCollectionChanged;
                _parentColumnOwners.Remove(oldColumns);
                _childToParent.Remove(childGrid);
            }

            if (e.NewValue is not DataGrid parentGrid)
                return;

            _childToParent.Remove(childGrid);
            _childToParent.Add(childGrid, parentGrid);
            _columTypes.Clear();
            childGrid.AutoGeneratingColumn += onChildAutoGeneratingColumn;
            childGrid.Loaded += onChildGridLoaded;
            subscribeToParentColumnsChanged(parentGrid, childGrid);
        }

        private static void onChildAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs args)
        {
            if (sender is DataGrid childGrid && _childToParent.TryGetValue(childGrid, out var parentGrid))
            {
                _columTypes.Add(args.PropertyType);
                syncColumnWidth(parentGrid, args.Column, args.Column.DisplayIndex);
            }
        }

        private static void onChildGridLoaded(object sender, RoutedEventArgs args)
        {
            if (sender is DataGrid childGrid && _childToParent.TryGetValue(childGrid, out var parentGrid))
                syncAllColumnWidth(parentGrid, childGrid);
        }

        private static void syncColumnWidth(DataGrid parentGrid, DataGridColumn childColumn, int columnIndex)
        {
            if (columnIndex < 0)
                return;
            if (parentGrid.Columns.Count > columnIndex)
            {
                var parentColumn = parentGrid.Columns[columnIndex];
                var binding = new Binding("ActualWidth")
                {
                    Source = parentColumn,
                    Mode = BindingMode.OneWay
                };

                BindingOperations.SetBinding(childColumn, DataGridColumn.WidthProperty, binding);
            }
        }

        private static void syncAllColumnWidth(DataGrid parentGrid, DataGrid childGrid)
        {
            var minColumns = Math.Min(parentGrid.Columns.Count, childGrid.Columns.Count);
            if (_columTypes.Count == 0)
                return;
            for (var i = 0; i < minColumns; i++)
            {
                var style = new Style(typeof(TextBlock));
                if (_columTypes[i] == typeof(decimal))
                {
                    style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right));
                }
                style.Setters.Add(new Setter(TextBlock.ForegroundProperty, Application.Current.TryFindResource("ResultBrush")));

                var binding = new Binding("ActualWidth")
                {
                    Source = parentGrid.Columns[i],
                    Mode = BindingMode.OneWay
                };

                BindingOperations.SetBinding(childGrid.Columns[i], DataGridColumn.WidthProperty, binding);

                if (childGrid.Columns[i] is DataGridTextColumn textColumn)
                    textColumn.ElementStyle = style;
                else if (childGrid.Columns[i] is DataGridBoundColumn boundColumn)
                    boundColumn.ElementStyle = style;
            }
        }

        private static void subscribeToParentColumnsChanged(DataGrid parentGrid, DataGrid childGrid)
        {
            var columns = (INotifyCollectionChanged)parentGrid.Columns;
            _parentColumnOwners.Remove(columns);
            _parentColumnOwners.Add(columns, new GridPair { Parent = parentGrid, Child = childGrid });
            columns.CollectionChanged += onParentColumnsCollectionChanged;
        }

        private static void onParentColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_parentColumnOwners.TryGetValue(sender, out var pair))
            {
                pair.Child.Dispatcher.BeginInvoke(new Action(() =>
                {
                    syncAllColumnWidth(pair.Parent, pair.Child);
                }), DispatcherPriority.Loaded);
            }
        }
        #endregion

        #region ScrollIntoView
        public static readonly DependencyProperty ScrollIntoViewProperty =
           DependencyProperty.RegisterAttached("ScrollIntoView",
               typeof(object),
               typeof(DataGridHelper),
               new PropertyMetadata(null, onScrollIntoViewChanged));

        public static object GetScrollIntoView(DependencyObject obj) => obj.GetValue(ScrollIntoViewProperty);

        public static void SetScrollIntoView(DependencyObject obj, object value) => obj.SetValue(ScrollIntoViewProperty, value);

        private static void onScrollIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && e.NewValue != null)
            {
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dataGrid.UpdateLayout();
                    dataGrid.ScrollIntoView(e.NewValue);
                }));
            }
        }

        public static readonly DependencyProperty ScrollIntoViewByColumnProperty =
           DependencyProperty.RegisterAttached("ScrollIntoViewByColumn",
               typeof(Tuple<object, string>),
               typeof(DataGridHelper),
               new PropertyMetadata(null, onScrollIntoViewByColumnChanged));

        public static Tuple<object, string> GetScrollIntoViewByColumn(DependencyObject obj) => (Tuple<object, string>)obj.GetValue(ScrollIntoViewByColumnProperty);

        public static void SetScrollIntoViewByColumn(DependencyObject obj, object value) => obj.SetValue(ScrollIntoViewByColumnProperty, value);

        private static void onScrollIntoViewByColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid && e.NewValue != null)
            {
                if (e.NewValue is not Tuple<object, string> tuple) return;
                var cols = dataGrid.Columns.Where(c => c.GetValue(WPFGridViewHelper.ColumnTagProperty) != null);
                if (!cols.Any()) return;
                var col = cols.FirstOrDefault(c => c.GetValue(WPFGridViewHelper.ColumnTagProperty).ToString() == tuple.Item2);
                if (col == null) return;
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    dataGrid.UpdateLayout();
                    dataGrid.ScrollIntoView(tuple.Item1, col);
                }));
            }
        }
        #endregion

        #region Loading
        public static readonly DependencyProperty DataGridLoadedCallbackProperty =
            DependencyProperty.RegisterAttached("DataGridLoadedCallback",
                typeof(Action<int, Guid>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onDataGridLoaded));

        public static Action<int, Guid> GetDataGridLoadedCallback(DependencyObject obj) => (Action<int, Guid>)obj.GetValue(DataGridLoadedCallbackProperty);

        public static void SetDataGridLoadedCallback(DependencyObject obj, Action<int, Guid> value) => obj.SetValue(DataGridLoadedCallbackProperty, value);

        private static void onDataGridLoaded(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue != null)
                    dataGrid.Loaded -= onDataGridLoadedHandler;
                if (e.NewValue != null)
                    dataGrid.Loaded += onDataGridLoadedHandler;
            }
        }

        private static void onDataGridLoadedHandler(object s, RoutedEventArgs args)
        {
            if (s is DataGrid dataGrid)
                reportLoaded(dataGrid);
        }


        private static void reportLoaded(DataGrid dataGrid)
        {
            var callback = GetDataGridLoadedCallback(dataGrid);
            if (callback != null && dataGrid.Tag is Guid guid)
            {
                callback(dataGrid.Columns.Count, guid);
            }
        }
        #endregion

        #region Mouse weel
        public static readonly DependencyProperty DataGridMouseWheelCallbackProperty =
            DependencyProperty.RegisterAttached("DataGridMouseWheelCallback",
                typeof(Action<Tuple<DataGrid, MouseWheelEventArgs>>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onMouseWeelCommandChanged));
        public static Action<Tuple<DataGrid, MouseWheelEventArgs>> GetDataGridMouseWheelCallback(DependencyObject obj) => (Action<Tuple<DataGrid, MouseWheelEventArgs>>)obj.GetValue(DataGridMouseWheelCallbackProperty);

        public static void SetDataGridMouseWheelCallback(DependencyObject obj, Action<Tuple<DataGrid, MouseWheelEventArgs>> value) => obj.SetValue(DataGridMouseWheelCallbackProperty, value);

        private static void onMouseWeelCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.MouseWheel -= onMouseWheelHandler;
                if (e.NewValue != null)
                    dataGrid.MouseWheel += onMouseWheelHandler;
            }
        }

        private static void onMouseWheelHandler(object s, MouseWheelEventArgs e)
        {
            if (s is DataGrid dataGrid)
                reportMouseWheel(dataGrid, e);
        }

        private static void reportMouseWheel(DataGrid dataGrid, MouseWheelEventArgs e)
        {
            var callback = GetDataGridMouseWheelCallback(dataGrid);
            if (callback != null)
            {
                callback(new(dataGrid, e));
            }
        }
        #endregion

        #region ColumnsHeadersAndWidth
        public static readonly DependencyProperty ColumnsHeadersWidthAndVisibilityCallbackProperty =
            DependencyProperty.RegisterAttached("ColumnsHeadersWidthAndVisibilityCallback",
                typeof(Action<IEnumerable<(string, double, Visibility)>>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onColumnsHeadersWidthAndVisibilityCallbackChanged));

        public static Action<IEnumerable<(string, double, Visibility)>> GetColumnsHeadersWidthAndVisibilityCallback(DependencyObject obj) => (Action<IEnumerable<(string, double, Visibility)>>)obj.GetValue(ColumnsHeadersWidthAndVisibilityCallbackProperty);

        public static void SetColumnsHeadersWidthAndVisibilityCallback(DependencyObject obj, Action<IEnumerable<(string, double, Visibility)>> value) => obj.SetValue(ColumnsHeadersWidthAndVisibilityCallbackProperty, value);

        private static readonly ConditionalWeakTable<object, DataGrid> _columnCollectionOwners = new();

        private static void onColumnsHeadersWidthAndVisibilityCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue != null)
                {
                    dataGrid.Loaded -= OnColumnsCallbackLoaded;
                    ((INotifyCollectionChanged)dataGrid.Columns).CollectionChanged -= OnColumnsCallbackCollectionChanged;
                }

                if (e.NewValue != null)
                {
                    _columnCollectionOwners.Remove(dataGrid.Columns);
                    _columnCollectionOwners.Add(dataGrid.Columns, dataGrid);
                    dataGrid.Loaded += OnColumnsCallbackLoaded;
                    ((INotifyCollectionChanged)dataGrid.Columns).CollectionChanged += OnColumnsCallbackCollectionChanged;
                    subscribeToColumnWidthAndVisibilityChanges(dataGrid);
                }
            }
        }

        private static void OnColumnsCallbackLoaded(object s, RoutedEventArgs args)
        {
            if (s is DataGrid dataGrid)
                reportColumnHeadersWidthAndVisibility(dataGrid);
        }

        private static void OnColumnsCallbackCollectionChanged(object s, NotifyCollectionChangedEventArgs args)
        {
            if (_columnCollectionOwners.TryGetValue(s, out var dataGrid))
                reportColumnHeadersWidthAndVisibility(dataGrid);
        }


        private static readonly ConditionalWeakTable<DataGrid, List<(DataGridColumn, DependencyPropertyDescriptor)>> _visibilityWidthDescriptors = new();

        private static void subscribeToColumnWidthAndVisibilityChanges(DataGrid dataGrid)
        {
            unsubscribeFromColumnWidthAndVisibilityChanges(dataGrid);

            var descriptors = new List<(DataGridColumn, DependencyPropertyDescriptor)>();
            foreach (var column in dataGrid.Columns)
            {
                var visibilityDescriptor = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.VisibilityProperty,
                    typeof(DataGridColumn));
                if (visibilityDescriptor != null)
                {
                    visibilityDescriptor.AddValueChanged(column, onColumnWidthOrVisibilityChanged);
                    descriptors.Add((column, visibilityDescriptor));
                }
                var widthDescriptor = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.WidthProperty,
                    typeof(DataGridColumn));
                if (widthDescriptor != null)
                {
                    widthDescriptor.AddValueChanged(column, onColumnWidthOrVisibilityChanged);
                    descriptors.Add((column, widthDescriptor));
                }
            }
            _visibilityWidthDescriptors.Remove(dataGrid);
            _visibilityWidthDescriptors.Add(dataGrid, descriptors);

            dataGrid.Unloaded -= onVisibilityWidthDataGridUnloaded;
            dataGrid.Unloaded += onVisibilityWidthDataGridUnloaded;
        }

        private static void unsubscribeFromColumnWidthAndVisibilityChanges(DataGrid dataGrid)
        {
            if (_visibilityWidthDescriptors.TryGetValue(dataGrid, out var list))
            {
                foreach (var (col, desc) in list)
                    desc.RemoveValueChanged(col, onColumnWidthOrVisibilityChanged);
                _visibilityWidthDescriptors.Remove(dataGrid);
            }
        }

        private static void onVisibilityWidthDataGridUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dg) unsubscribeFromColumnWidthAndVisibilityChanges(dg);
        }

        private static void reportColumnHeadersWidthAndVisibility(DataGrid dataGrid)
        {
            var callback = GetColumnsHeadersWidthAndVisibilityCallback(dataGrid);
            if (callback != null)
            {
                var headersWidthAndVisibility = dataGrid.Columns.Select(c => (c.Header?.ToString() ?? string.Empty, c.ActualWidth, c.Visibility));
                callback(headersWidthAndVisibility);
            }
        }

        private static void onColumnWidthOrVisibilityChanged(object sender, EventArgs e)
        {
            if (sender is DataGridColumn changedColumn)
            {
                var dataGrid = findParentDataGrid(changedColumn);
                if (dataGrid == null) return;
                reportColumnHeadersWidthAndVisibility(dataGrid);
            }
        }

        #endregion

        #region ColumnsHeadersAndWidth
        public static readonly DependencyProperty ColumnsHeadersAndWidthCallbackProperty =
            DependencyProperty.RegisterAttached("ColumnsHeadersAndWidthCallback",
                typeof(Action<IEnumerable<(string, double)>>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onColumnsHeadersAndWidthCallbackChanged));

        public static Action<IEnumerable<(string, double)>> GetColumnsHeadersAndWidthCallback(DependencyObject obj) => (Action<IEnumerable<(string, double)>>)obj.GetValue(ColumnsHeadersAndWidthCallbackProperty);

        public static void SetColumnsHeadersAndWidthCallback(DependencyObject obj, Action<IEnumerable<(string, double)>> value) => obj.SetValue(ColumnsHeadersAndWidthCallbackProperty, value);

        private static readonly ConditionalWeakTable<object, DataGrid> _widthCollectionOwners = new();
        private static readonly ConditionalWeakTable<DataGrid, List<(DataGridColumn, DependencyPropertyDescriptor)>> _widthDescriptors = new();

        private static void onColumnsHeadersAndWidthCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue != null)
                {
                    dataGrid.Loaded -= onColumnsHeadersAndWidthLoaded;
                    dataGrid.Columns.CollectionChanged -= onColumnsHeadersAndWidthCollectionChanged;
                    _widthCollectionOwners.Remove(dataGrid.Columns);
                    unsubscribeFromColumnWidthChanges(dataGrid);
                }
                if (e.NewValue != null)
                {
                    _widthCollectionOwners.Remove(dataGrid.Columns);
                    _widthCollectionOwners.Add(dataGrid.Columns, dataGrid);
                    dataGrid.Loaded += onColumnsHeadersAndWidthLoaded;
                    dataGrid.Columns.CollectionChanged += onColumnsHeadersAndWidthCollectionChanged;
                    subscribeToColumnWidthChanges(dataGrid);
                }
            }
        }

        private static void onColumnsHeadersAndWidthLoaded(object s, RoutedEventArgs args)
        {
            if (s is DataGrid dg) reportColumnHeadersAndWidth(dg);
        }

        private static void onColumnsHeadersAndWidthCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_widthCollectionOwners.TryGetValue(sender, out var dg))
                reportColumnHeadersAndWidth(dg);
        }

        private static void subscribeToColumnWidthChanges(DataGrid dataGrid)
        {
            unsubscribeFromColumnWidthChanges(dataGrid);

            var descriptors = new List<(DataGridColumn, DependencyPropertyDescriptor)>();
            foreach (var column in dataGrid.Columns)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.WidthProperty,
                    typeof(DataGridColumn));
                if (descriptor != null)
                {
                    descriptor.AddValueChanged(column, onColumnWidthChanged);
                    descriptors.Add((column, descriptor));
                }
            }
            _widthDescriptors.Remove(dataGrid);
            _widthDescriptors.Add(dataGrid, descriptors);

            dataGrid.Unloaded -= onWidthDataGridUnloaded;
            dataGrid.Unloaded += onWidthDataGridUnloaded;
        }

        private static void unsubscribeFromColumnWidthChanges(DataGrid dataGrid)
        {
            if (_widthDescriptors.TryGetValue(dataGrid, out var list))
            {
                foreach (var (col, desc) in list)
                    desc.RemoveValueChanged(col, onColumnWidthChanged);
                _widthDescriptors.Remove(dataGrid);
            }
        }

        private static void onWidthDataGridUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dg) unsubscribeFromColumnWidthChanges(dg);
        }

        private static void reportColumnHeadersAndWidth(DataGrid dataGrid)
        {
            var callback = GetColumnsHeadersAndWidthCallback(dataGrid);
            if (callback != null)
            {
                var headersAndWidth = dataGrid.Columns.Select(c => (c.Header?.ToString() ?? string.Empty, c.ActualWidth));
                callback(headersAndWidth);
            }
        }

        private static void onColumnWidthChanged(object sender, EventArgs e)
        {
            if (sender is DataGridColumn changedColumn)
            {
                var dataGrid = findParentDataGrid(changedColumn);
                if (dataGrid == null) return;
                reportColumnHeadersAndWidth(dataGrid);
            }
        }
        private static DataGrid findParentDataGrid(DataGridColumn column)
        {
            //find the DataGrid that contains this column
            return Application.Current.Windows
                .OfType<Window>()
                .SelectMany(w => findDataGrids(w))
                .FirstOrDefault(dg => dg.Columns.Contains(column));
        }
        private static IEnumerable<DataGrid> findDataGrids(DependencyObject parent)
        {
            var result = new List<DataGrid>();

            if (parent is DataGrid dataGrid)
            {
                result.Add(dataGrid);
            }
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                result.AddRange(findDataGrids(child));
            }
            return result;
        }
        #endregion

        #region ColumnsHeaders
        public static readonly DependencyProperty ColumnsHeadersCallbackProperty =
            DependencyProperty.RegisterAttached("ColumnsHeadersCallback",
                typeof(Action<IEnumerable<string>>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onColumnsHeadersCallbackChanged));

        public static Action<IEnumerable<string>> GetColumnsHeadersCallback(DependencyObject obj) => (Action<IEnumerable<string>>)obj.GetValue(ColumnsHeadersCallbackProperty);

        public static void SetColumnsHeadersCallback(DependencyObject obj, Action<IEnumerable<string>> value) => obj.SetValue(ColumnsHeadersCallbackProperty, value);

        private static readonly ConditionalWeakTable<object, DataGrid> _headersCollectionOwners = new();

        private static void onColumnsHeadersCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue != null)
                {
                    dataGrid.Loaded -= onColumnsHeadersLoaded;
                    dataGrid.Columns.CollectionChanged -= onColumnsHeadersCollectionChanged;
                    _headersCollectionOwners.Remove(dataGrid.Columns);
                }
                if (e.NewValue != null)
                {
                    _headersCollectionOwners.Remove(dataGrid.Columns);
                    _headersCollectionOwners.Add(dataGrid.Columns, dataGrid);
                    dataGrid.Loaded += onColumnsHeadersLoaded;
                    dataGrid.Columns.CollectionChanged += onColumnsHeadersCollectionChanged;
                }
            }
        }

        private static void onColumnsHeadersLoaded(object s, RoutedEventArgs args)
        {
            if (s is DataGrid dg) reportColumnHeaders(dg);
        }

        private static void onColumnsHeadersCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_headersCollectionOwners.TryGetValue(sender, out var dg))
                reportColumnHeaders(dg);
        }
        private static void reportColumnHeaders(DataGrid dataGrid)
        {
            var callback = GetColumnsHeadersCallback(dataGrid);
            if (callback != null)
            {
                var headers = dataGrid.Columns.Select(c => c.Header?.ToString() ?? string.Empty);
                callback(headers);
            }
        }
        #endregion

        #region ColumnsHeadersAsElements
        public static readonly DependencyProperty ColumnsHeadersAsElementsCallbackProperty =
            DependencyProperty.RegisterAttached("ColumnsHeadersAsElementsCallback",
                typeof(Action<IEnumerable<UIElement>>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onColumnsHeadersAsElementsCallbackChanged));

        public static Action<IEnumerable<UIElement>> GeColumnsHeadersAsElementsCallback(DependencyObject obj) => (Action<IEnumerable<UIElement>>)obj.GetValue(ColumnsHeadersAsElementsCallbackProperty);

        public static void SetColumnsHeadersAsElementsCallback(DependencyObject obj, Action<IEnumerable<UIElement>> value) => obj.SetValue(ColumnsHeadersAsElementsCallbackProperty, value);

        private static readonly ConditionalWeakTable<object, DataGrid> _headersAsElementsCollectionOwners = new();

        private static void onColumnsHeadersAsElementsCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue != null)
                {
                    dataGrid.Loaded -= onColumnsHeadersAsElementsLoaded;
                    dataGrid.Columns.CollectionChanged -= onColumnsHeadersAsElementsCollectionChanged;
                    _headersAsElementsCollectionOwners.Remove(dataGrid.Columns);
                }
                if (e.NewValue != null)
                {
                    _headersAsElementsCollectionOwners.Remove(dataGrid.Columns);
                    _headersAsElementsCollectionOwners.Add(dataGrid.Columns, dataGrid);
                    dataGrid.Loaded += onColumnsHeadersAsElementsLoaded;
                    dataGrid.Columns.CollectionChanged += onColumnsHeadersAsElementsCollectionChanged;
                }
            }
        }

        private static void onColumnsHeadersAsElementsLoaded(object s, RoutedEventArgs args)
        {
            if (s is DataGrid dg) reportColumnHeadersAsElements(dg);
        }

        private static void onColumnsHeadersAsElementsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_headersAsElementsCollectionOwners.TryGetValue(sender, out var dg))
                reportColumnHeadersAsElements(dg);
        }
        private static void reportColumnHeadersAsElements(DataGrid dataGrid)
        {
            var callback = GeColumnsHeadersAsElementsCallback(dataGrid);
            if (callback != null)
            {
                var headers = dataGrid.Columns.Select(c => c.Header as UIElement);
                callback(headers);
            }
        }
        #endregion

        #region Get tag
        public static readonly DependencyProperty TagCallbackProperty =
            DependencyProperty.RegisterAttached("TagCallback",
                typeof(Action<object>),
                typeof(DataGridHelper),
                new PropertyMetadata(null, onTagCallbackChanged));

        public static Action<object> GetTagCallback(DependencyObject obj) => (Action<object>)obj.GetValue(TagCallbackProperty);

        public static void SetTagCallback(DependencyObject obj, Action<object> value) => obj.SetValue(TagCallbackProperty, value);

        private static void onTagCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue != null)
                    dataGrid.Loaded -= onTagLoaded;
                if (e.NewValue != null)
                    dataGrid.Loaded += onTagLoaded;
            }
        }

        private static void onTagLoaded(object s, RoutedEventArgs args)
        {
            if (s is DataGrid dg) reportTag(dg);
        }

        private static void reportTag(DataGrid dataGrid)
        {
            var callback = GetTagCallback(dataGrid);
            if (callback != null)
            {
                callback(dataGrid.Tag);
            }
        }
        #endregion
    }

    #region Checked items behavior
    public class DataGridWithCheckedItemsBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonUp += onDataGridPreviewMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonUp -= onDataGridPreviewMouseLeftButtonUp;
        }

        private void onDataGridPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(AssociatedObject);
            var hitTestResult = VisualTreeHelper.HitTest(AssociatedObject, mousePosition);
            var cell = findAncestor<DataGridCell>(hitTestResult.VisualHit);
            if (cell != null && cell.DataContext is CheckedItem checkedItem)
                checkedItem.IsChecked = !checkedItem.IsChecked;
        }

        private static T findAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
    #endregion

    #region Syncronize columns
    public static class DataGridSyncBehavior
    {
        private static readonly Dictionary<string, List<WeakReference>> _syncGroups = new();

        #region SyncGroup Attached Property
        public static readonly DependencyProperty SyncGroupProperty =
            DependencyProperty.RegisterAttached(
                "SyncGroup",
                typeof(string),
                typeof(DataGridSyncBehavior),
                new PropertyMetadata(null, onSyncGroupChanged));
        public static void SetSyncGroup(DependencyObject obj, string value) => obj.SetValue(SyncGroupProperty, value);
        public static string GetSyncGroup(DependencyObject obj) => (string)obj.GetValue(SyncGroupProperty);
        #endregion

        private static void onSyncGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                var oldGroup = e.OldValue as string;
                var newGroup = e.NewValue as string;

                if (!string.IsNullOrEmpty(oldGroup) && _syncGroups.ContainsKey(oldGroup))
                {
                    _syncGroups[oldGroup].RemoveAll(wr => !wr.IsAlive || wr.Target == dataGrid);
                    if (_syncGroups[oldGroup].Count == 0)
                        _syncGroups.Remove(oldGroup);
                }

                if (!string.IsNullOrEmpty(newGroup))
                {
                    if (!_syncGroups.ContainsKey(newGroup))
                        _syncGroups[newGroup] = new List<WeakReference>();
                    _syncGroups[newGroup].Add(new WeakReference(dataGrid));

                    dataGrid.Loaded += onDataGridLoaded;
                    dataGrid.Unloaded += onDataGridUnloaded;
                }
            }
        }
        private static void onDataGridLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                subscribeToColumnWidthChanges(dataGrid);
            }
        }
        private static void onDataGridUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                unsubscribeFromColumnWidthChanges(dataGrid);
            }
        }
        private static readonly ConditionalWeakTable<DataGrid, List<(DataGridColumn, DependencyPropertyDescriptor)>> _columnDescriptors = new();

        private static void subscribeToColumnWidthChanges(DataGrid dataGrid)
        {
            // Remove existing subscriptions first
            if (_columnDescriptors.TryGetValue(dataGrid, out var existingDescriptors))
            {
                foreach (var (column, descriptor) in existingDescriptors)
                    descriptor.RemoveValueChanged(column, onColumnWidthChanged);
                _columnDescriptors.Remove(dataGrid);
            }

            var descriptors = new List<(DataGridColumn, DependencyPropertyDescriptor)>();
            foreach (var column in dataGrid.Columns)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.WidthProperty,
                    typeof(DataGridColumn));
                if (descriptor != null)
                {
                    descriptor.AddValueChanged(column, onColumnWidthChanged);
                    descriptors.Add((column, descriptor));
                }
            }
            _columnDescriptors.Add(dataGrid, descriptors);

            dataGrid.Unloaded -= onDataGridUnloadedForCleanup;
            dataGrid.Unloaded += onDataGridUnloadedForCleanup;
        }

        private static void onDataGridUnloadedForCleanup(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                dataGrid.Unloaded -= onDataGridUnloadedForCleanup;
                if (_columnDescriptors.TryGetValue(dataGrid, out var descriptors))
                {
                    foreach (var (column, descriptor) in descriptors)
                        descriptor.RemoveValueChanged(column, onColumnWidthChanged);
                    _columnDescriptors.Remove(dataGrid);
                }
            }
        }

        private static void unsubscribeFromColumnWidthChanges(DataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(
                    DataGridColumn.WidthProperty,
                    typeof(DataGridColumn));
                descriptor?.RemoveValueChanged(column, onColumnWidthChanged);
            }
        }
        private static void onColumnWidthChanged(object sender, EventArgs e)
        {
            if (sender is DataGridColumn changedColumn)
            {
                var dataGrid = findParentDataGrid(changedColumn);
                if (dataGrid == null) return;
                var syncGroup = GetSyncGroup(dataGrid);
                if (string.IsNullOrEmpty(syncGroup) || !_syncGroups.ContainsKey(syncGroup))
                    return;
                var columnIndex = dataGrid.Columns.IndexOf(changedColumn);
                if (columnIndex < 0) return;

                foreach (var weakRef in _syncGroups[syncGroup].ToList())
                {
                    if (!weakRef.IsAlive)
                    {
                        _syncGroups[syncGroup].Remove(weakRef);
                        continue;
                    }

                    if (weakRef.Target is DataGrid otherGrid && otherGrid != dataGrid && otherGrid.Columns.Count > columnIndex)
                    {
                        var descriptor = DependencyPropertyDescriptor.FromProperty(
                            DataGridColumn.WidthProperty,
                            typeof(DataGridColumn));
                        var targetColumn = otherGrid.Columns[columnIndex];
                        descriptor?.RemoveValueChanged(targetColumn, onColumnWidthChanged);
                        targetColumn.Width = changedColumn.Width;
                        descriptor?.AddValueChanged(targetColumn, onColumnWidthChanged);
                    }
                }
            }
        }
        private static DataGrid findParentDataGrid(DataGridColumn column)
        {
            //find the DataGrid that contains this column
            return Application.Current.Windows
                .OfType<Window>()
                .SelectMany(w => findDataGrids(w))
                .FirstOrDefault(dg => dg.Columns.Contains(column));
        }
        private static IEnumerable<DataGrid> findDataGrids(DependencyObject parent)
        {
            var result = new List<DataGrid>();

            if (parent is DataGrid dataGrid)
            {
                result.Add(dataGrid);
            }
            for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                result.AddRange(findDataGrids(child));
            }
            return result;
        }
    }

    #endregion

    #region Scroll changed
    public class DataGridScrollChangedBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DataGridScrollChangedBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += onDataGridLoaded;
            AssociatedObject.Unloaded += onDataGridUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= onDataGridLoaded;
            AssociatedObject.Unloaded -= onDataGridUnloaded;
        }

        private void onDataGridLoaded(object sender, EventArgs e)
        {
            var scrollViewer = findChild<ScrollViewer>(AssociatedObject);
            if (scrollViewer == null)
                return;
            scrollViewer.ScrollChanged += onDataGridScrollChanged;
        }

        private void onDataGridUnloaded(object sender, EventArgs e)
        {
            var scrollViewer = findChild<ScrollViewer>(AssociatedObject);
            if (scrollViewer == null)
                return;
            scrollViewer.ScrollChanged -= onDataGridScrollChanged;
        }

        private void onDataGridScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (Command == null) return;
            if (e.HorizontalChange != 0)
            {
                Command.Execute(e);
            }
        }

        private T findChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T childT)
                    return childT;

                var result = findChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
    #endregion

    #region Column header click
    public class DataGridColumnHeaderClickBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DataGridColumnHeaderClickBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += onDataGricPreviewLeftMouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= onDataGricPreviewLeftMouseDown;
        }

        private void onDataGricPreviewLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element)
            {
                var hitTestResult = VisualTreeHelper.HitTest(element, e.GetPosition(element));
                var ancestor = findAncestor<Thumb>(hitTestResult?.VisualHit);
                if (ancestor != null)
                {
                    return;
                }
            }
            var header = findAncestor<DataGridColumnHeader>(e.OriginalSource as DependencyObject);
            if (header == null || Command == null) return;
            if (header.Column != null && Command.CanExecute(header.Column))
            {
                Command.Execute(header.Column);
            }
        }

        private static T findAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
    #endregion

    #region Cell changed and column
    public class DataGridCellChangedBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DataGridCellChangedBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CurrentCellChanged += onCurrentCellChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.CurrentCellChanged -= onCurrentCellChanged;
        }

        private void onCurrentCellChanged(object sender, EventArgs e)
        {
            if (AssociatedObject is DataGrid dataGrid)
            {
                if (dataGrid.CurrentCell != null)
                {
                    var column = dataGrid.CurrentCell.Column;
                    var selectedItem = dataGrid.SelectedItem;

                    var parameter = new DataGridCellChangedParameters
                    {
                        SelectedItem = selectedItem,
                        Column = column,
                        Cell = dataGrid.CurrentCell,
                        ControlName = dataGrid.Name
                    };

                    if (Command != null && Command.CanExecute(parameter))
                    {
                        Command.Execute(parameter);
                    }
                }
            }
        }
    }
    #endregion

    #region Double click and column
    public class DataGridDoubleClickBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DataGridDoubleClickBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += onMouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDoubleClick -= onMouseDoubleClick;
        }

        private void onMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject is DataGrid dataGrid)
            {
                var mousePosition = e.GetPosition(dataGrid);
                var hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePosition);
                var cell = findAncestor<DataGridCell>(hitTestResult.VisualHit);
                var row = findAncestor<DataGridRow>(hitTestResult.VisualHit);
                if (cell != null)
                {
                    var column = cell.Column;
                    var selectedItem = dataGrid.SelectedItem;

                    var parameter = new DataGridDoubleClickParameters
                    {
                        SelectedItem = selectedItem,
                        Column = column,
                        Cell = cell,
                        Row = row,
                        ControlName = dataGrid.Name
                    };

                    if (Command != null && Command.CanExecute(parameter))
                    {
                        Command.Execute(parameter);
                    }
                }
            }
        }

        private static T findAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }

    public class DataGridDoubleClickWithToggleButtonBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DataGridDoubleClickWithToggleButtonBehavior));

        public static readonly DependencyProperty ToggleButtonProperty =
            DependencyProperty.Register(nameof(ToggleButton),
                typeof(ToggleButton),
                typeof(DataGridDoubleClickWithToggleButtonBehavior));

        public ToggleButton ToggleButton
        {
            get => (ToggleButton)GetValue(ToggleButtonProperty);
            set => SetValue(ToggleButtonProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += onMouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDoubleClick -= onMouseDoubleClick;
        }

        private void onMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject is DataGrid dataGrid)
            {
                var mousePosition = e.GetPosition(dataGrid);
                var hitTestResult = VisualTreeHelper.HitTest(dataGrid, mousePosition);
                var cell = findAncestor<DataGridCell>(hitTestResult.VisualHit);

                if (cell != null)
                {
                    var column = cell.Column;
                    var selectedItem = dataGrid.SelectedItem;

                    var parameter = new DataGridDoubleClickParameters
                    {
                        SelectedItem = selectedItem,
                        Column = column,
                        Cell = cell,
                        ControlName = dataGrid.Name
                    };

                    if (Command != null && Command.CanExecute(parameter))
                    {
                        Command.Execute(parameter);
                    }
                }
                closePopup();
            }
        }

        private void closePopup()
        {
            if (ToggleButton != null)
            {
                ToggleButton.IsChecked = false;
                return;
            }

            var toggleButton = findToggleButtonForPopup();
            if (toggleButton != null)
            {
                toggleButton.IsChecked = false;
            }
        }

        private ToggleButton findToggleButtonForPopup()
        {
            var popup = findParent<Popup>(AssociatedObject);
            if (popup?.PlacementTarget is ToggleButton toggle)
                return toggle;

            var parentGrid = findParent<Grid>(popup);
            return findChild<ToggleButton>(parentGrid);
        }

        private T findParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            return (T)(parent is T ? parent : findParent<T>(parent));
        }

        private T findChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T childT)
                    return childT;

                var result = findChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private static T findAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }

    public class DataGridDoubleClickParameters
    {
        public object SelectedItem { get; set; }
        public DataGridColumn Column { get; set; }
        public DataGridCell Cell { get; set; }
        public DataGridRow Row { get; set; }
        public string ControlName { get; set; }
    }

    public class DataGridCellChangedParameters
    {
        public object SelectedItem { get; set; }
        public DataGridColumn Column { get; set; }
        public DataGridCellInfo Cell { get; set; }
        public string ControlName { get; set; }
    }

    #endregion

    #region Auto-sized columns
    public static class DataGridAutoSizeBehavior
    {
        public static readonly DependencyProperty AutoSizeColumnsProperty =
            DependencyProperty.RegisterAttached("AutoSizeColumns", typeof(bool), typeof(DataGridAutoSizeBehavior),
                new PropertyMetadata(false, onAutoSizeColumnsChanged));

        public static bool GetAutoSizeColumns(DependencyObject obj) => (bool)obj.GetValue(AutoSizeColumnsProperty);
        public static void SetAutoSizeColumns(DependencyObject obj, bool value) => obj.SetValue(AutoSizeColumnsProperty, value);

        private static void onAutoSizeColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue is bool oldVal && oldVal)
                    dataGrid.TargetUpdated -= onTargetUpdated;
                if (e.NewValue is bool newVal && newVal)
                    dataGrid.TargetUpdated += onTargetUpdated;
            }
        }

        private static void onTargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is DataGrid dg) refreshColumnWidths(dg);
        }

        private static void refreshColumnWidths(DataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns.Where(c => c.Width.IsAuto))
            {
                column.Width = 0;
                column.Width = DataGridLength.Auto;
            }
        }
    } 
    #endregion

    #region Common datagrid

    public class DataGridBevavior : Behavior<DataGrid>
    {
        private RoutedEventHandler _loadedHandler;

        protected override void OnAttached()
        {
            base.OnAttached();
            _loadedHandler = async (sender, e) => await Dispatcher.InvokeAsync(() =>
            {
                var viewModel = AssociatedObject.DataContext as BaseViewModelWithActions;
                if (viewModel == null)
                {
                    var rootView = findRootView(AssociatedObject);
                    if (rootView != null)
                        viewModel = rootView.DataContext as BaseViewModelWithActions;
                }
                if (viewModel != null && AssociatedObject.Tag is Enum extTag)
                {
                    viewModel.RegisterReplaceColumnsAction(extTag, (columns) =>
                    {
                        AssociatedObject.Columns.Clear();
                        foreach (var col in columns)
                            AssociatedObject.Columns.Add(col);
                    });
                    viewModel.RegisterClearColumnsAction(extTag, () =>
                    {
                        AssociatedObject.Columns.Clear();
                    });
                    viewModel.RegisterClearColumnsTillAction(extTag, (columnsToPreserve) =>
                    {
                        for (var i = AssociatedObject.Columns.Count - 1; i > columnsToPreserve - 1; i--)
                        {
                            AssociatedObject.Columns.RemoveAt(i);
                        }
                    });
                    viewModel.RegisterAddColumnsAction(extTag, (columns) =>
                    {
                        foreach (var col in columns)
                            AssociatedObject.Columns.Add(col);
                    });
                    viewModel.RegisterUnhandleColumnsCheckboxesAction(extTag, (checkedHandle, uncheckedHandle) =>
                    {
                        var checkBoxes = AssociatedObject.Columns.Select(c => c.Header).OfType<Grid>().SelectMany(g => g.Children.OfType<CheckBox>());
                        foreach (var chk in checkBoxes)
                        {
                            chk.Checked -= checkedHandle;
                            chk.Unchecked -= uncheckedHandle;
                        }
                    });
                    viewModel.RegisterGetRowItemFunc(extTag, (obj) =>
                    {
                        var row = findParent<DataGridRow>(obj);
                        return row?.Item;
                    });
                    viewModel.RegisterSetColumnHeaderAction(extTag, (index, header) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns[index].Header = header;
                    });
                    viewModel.RegisterGetColumnIndexFunc(extTag, (obj) =>
                    {
                        var cell = findParent<DataGridCell>(obj);
                        return cell != null ? cell.Column.DisplayIndex : -1;
                    });
                    viewModel.RegisterSetColumnHeaderTemplateAction(extTag, (index, headerTemplate) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns[index].HeaderTemplate = headerTemplate;
                    });
                    viewModel.RegisterSetCellTemplateAction(extTag, (index, cellTemplate) =>
                    {
                        if (AssociatedObject.Columns.Count > index && AssociatedObject.Columns[index] is DataGridTemplateColumn column)
                            column.CellTemplate = cellTemplate;
                    });
                    viewModel.RegisterRemoveColumnAction(extTag, (index) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns.RemoveAt(index);
                    });
                    viewModel.RegisterInsertColumnAction(extTag, (index, column) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns.Insert(index, column);
                        else
                            AssociatedObject.Columns.Add(column);
                    });
                    viewModel.RegisterSetSortColumnByTagAction(extTag, (tag, direction) =>
                    {
                        foreach (var c in AssociatedObject.Columns)
                            c.SortDirection = null;
                        var column = AssociatedObject.Columns.FirstOrDefault(c => c.GetValue(WPFGridViewHelper.ColumnTagProperty) == tag);
                        if (column != null)
                            column.SortDirection = direction;
                    });
                    viewModel.RegisterSetSortColumnByIndexAction(extTag, (index, direction) =>
                    {
                        foreach (var c in AssociatedObject.Columns)
                            c.SortDirection = null;
                        AssociatedObject.Columns[index].SortDirection = direction;
                    });
                    viewModel.RegisterClearDataGridSortDirectionsAction(extTag, () =>
                    {
                        foreach (var c in AssociatedObject.Columns)
                            c.SortDirection = null;
                    });
                    viewModel.RegisterScrollHorizontallyAction(extTag, (e) =>
                    {
                        var scrollViewer = findChild<ScrollViewer>(AssociatedObject);
                        if (scrollViewer == null)
                            return;
                        scrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                    });
                    viewModel.RegisterColumnsVisibilityAction(extTag, (filter) =>
                    {
                        if (string.IsNullOrEmpty(filter))
                        {
                            foreach (var col in AssociatedObject.Columns)
                            {
                                col.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            foreach (var col in AssociatedObject.Columns)
                            {
                                col.Visibility = col.Header.ToString() == filter ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }
                    });
                    viewModel.RegisterGetVisibleColumnsFunction(extTag, () =>
                    {
                        return AssociatedObject.Columns.Where(c => c.Visibility == Visibility.Visible).Select(c => (c.DisplayIndex, c.Header?.ToString()));
                    });
                    viewModel.RegisterGetCurrentColumnFunction(extTag, () =>
                    {
                        if (AssociatedObject.CurrentCell == null)
                            return null;
                        return AssociatedObject.CurrentCell.Column;
                    });
                    viewModel.RegisterSetDataGridColumnsWidthAction(extTag, (columnsWidth) =>
                    {
                        var arr = columnsWidth.ToArray();
                        for (var i = 0; i < arr.Length; i++)
                        {
                            AssociatedObject.Columns[i].Width = arr[i];
                        }
                    });
                    viewModel.RegisterGetDataGridContainerFromFunction(extTag, (arg) =>
                    {
                        if (arg is int index)
                            return AssociatedObject.ItemContainerGenerator.ContainerFromIndex(index);
                        else if (arg != null)
                            return AssociatedObject.ItemContainerGenerator.ContainerFromItem(arg);
                        return null;
                    });
                    viewModel.RegisterSetDataGridHeightAction(extTag, (arg) =>
                    {
                        AssociatedObject.Height = AssociatedObject.ColumnHeaderHeight + arg;
                    });
                    viewModel.RegisterSetDataGridContextMenuAction(extTag, (contextMenu) =>
                    {
                        AssociatedObject.ContextMenu = contextMenu;
                    });
                }
                else if (viewModel != null && AssociatedObject.Tag is Guid guid)
                {
                    viewModel.RegisterAddColumnsAction(guid, (columns) =>
                    {
                        foreach (var col in columns)
                            AssociatedObject.Columns.Add(col);
                    });
                    viewModel.RegisterReplaceColumnsAction(guid, (columns) =>
                    {
                        AssociatedObject.Columns.Clear();
                        foreach (var col in columns)
                            AssociatedObject.Columns.Add(col);
                    });
                    viewModel.RegisterClearColumnsAction(guid, () =>
                    {
                        AssociatedObject.Columns.Clear();
                    });
                    viewModel.RegisterClearColumnsTillAction(guid, (columnsToPreserve) =>
                    {
                        for (var i = AssociatedObject.Columns.Count - 1; i > columnsToPreserve - 1; i--)
                        {
                            AssociatedObject.Columns.RemoveAt(i);
                        }
                    });
                    viewModel.RegisterUnhandleColumnsCheckboxesAction(guid, (checkedHandle, uncheckedHandle) =>
                    {
                        var checkBoxes = AssociatedObject.Columns.Select(c => c.Header).OfType<Grid>().SelectMany(g => g.Children.OfType<CheckBox>());
                        foreach (var chk in checkBoxes)
                        {
                            chk.Checked -= checkedHandle;
                            chk.Unchecked -= uncheckedHandle;
                        }
                    });
                    viewModel.RegisterGetRowItemFunc(guid, (obj) =>
                    {
                        var row = findParent<DataGridRow>(obj);
                        return row?.Item;
                    });
                    viewModel.RegisterSetColumnHeaderAction(guid, (index, header) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns[index].Header = header;
                    });
                    viewModel.RegisterGetColumnIndexFunc(guid, (obj) =>
                    {
                        var cell = findParent<DataGridCell>(obj);
                        return cell != null ? cell.Column.DisplayIndex : -1;
                    });
                    viewModel.RegisterSetColumnHeaderTemplateAction(guid, (index, headerTemplate) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns[index].HeaderTemplate = headerTemplate;
                    });
                    viewModel.RegisterSetCellTemplateAction(guid, (index, cellTemplate) =>
                    {
                        if (AssociatedObject.Columns.Count > index && AssociatedObject.Columns[index] is DataGridTemplateColumn column)
                            column.CellTemplate = cellTemplate;
                    });
                    viewModel.RegisterRemoveColumnAction(guid, (index) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns.RemoveAt(index);
                    });
                    viewModel.RegisterInsertColumnAction(guid, (index, column) =>
                    {
                        if (AssociatedObject.Columns.Count > index)
                            AssociatedObject.Columns.Insert(index, column);
                        else
                            AssociatedObject.Columns.Add(column);
                    });
                    viewModel.RegisterSetSortColumnByTagAction(guid, (tag, direction) =>
                    {
                        foreach (var c in AssociatedObject.Columns)
                            c.SortDirection = null;
                        var column = AssociatedObject.Columns.FirstOrDefault((c => c.GetValue(WPFGridViewHelper.ColumnTagProperty) == tag));
                        if (column != null)
                            column.SortDirection = direction;
                    });
                    viewModel.RegisterSetSortColumnByIndexAction(guid, (index, direction) =>
                    {
                        foreach (var c in AssociatedObject.Columns)
                            c.SortDirection = null;
                        AssociatedObject.Columns[index].SortDirection = direction;
                    });
                    viewModel.RegisterClearDataGridSortDirectionsAction(guid, () =>
                    {
                        foreach (var c in AssociatedObject.Columns)
                            c.SortDirection = null;
                    });
                    viewModel.RegisterScrollHorizontallyAction(guid, (e) =>
                    {
                        var scrollViewer = findChild<ScrollViewer>(AssociatedObject);
                        if (scrollViewer == null)
                            return;
                        scrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                    });
                    viewModel.RegisterColumnsVisibilityAction(guid, (filter) =>
                    {
                        if (string.IsNullOrEmpty(filter))
                        {
                            foreach (var col in AssociatedObject.Columns)
                            {
                                col.Visibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            foreach (var col in AssociatedObject.Columns)
                            {
                                col.Visibility = col.Header.ToString() == filter ? Visibility.Visible : Visibility.Collapsed;
                            }
                        }
                    });
                    viewModel.RegisterGetVisibleColumnsFunction(guid, () =>
                    {
                        return AssociatedObject.Columns.Where(c => c.Visibility == Visibility.Visible).Select(c => (c.DisplayIndex, c.Header?.ToString()));
                    });
                    viewModel.RegisterGetCurrentColumnFunction(guid, () =>
                    {
                        if (AssociatedObject.CurrentCell == null)
                            return null;
                        return AssociatedObject.CurrentCell.Column;
                    });
                    viewModel.RegisterSetDataGridColumnsWidthAction(guid, (columnsWidth) =>
                    {
                        var arr = columnsWidth.ToArray();
                        for (var i = 0; i < arr.Length; i++)
                        {
                            AssociatedObject.Columns[i].Width = arr[i];
                        }
                    });
                    viewModel.RegisterGetDataGridContainerFromFunction(guid, (arg) =>
                    {
                        if (arg is int index)
                            return AssociatedObject.ItemContainerGenerator.ContainerFromIndex(index);
                        else if (arg != null)
                            return AssociatedObject.ItemContainerGenerator.ContainerFromItem(arg);
                        return null;
                    });
                    viewModel.RegisterSetDataGridHeightAction(guid, (arg) =>
                    {
                        AssociatedObject.Height = AssociatedObject.ColumnHeaderHeight + arg;
                    });
                    viewModel.RegisterSetDataGridContextMenuAction(guid, (contextMenu) =>
                    {
                        AssociatedObject.ContextMenu = contextMenu;
                    });
                }
            }, DispatcherPriority.DataBind);
            AssociatedObject.Loaded += _loadedHandler;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (_loadedHandler != null)
                AssociatedObject.Loaded -= _loadedHandler;
        }

        private static FrameworkElement findRootView(DependencyObject element)
        {
            while (element != null)
            {
                if (element is Window || element is UserControl)
                    return element as FrameworkElement;

                if (element is FrameworkElement frameworkElement && frameworkElement.TemplatedParent != null)
                {
                    element = frameworkElement.TemplatedParent;
                }
                else
                {
                    element = LogicalTreeHelper.GetParent(element) ?? VisualTreeHelper.GetParent(element);
                }
            }
            return null;
        }

        private static T findParent<T>(object child) where T : DependencyObject
        {
            if (child is not DependencyObject) return null;

            var parent = VisualTreeHelper.GetParent((DependencyObject)child);
            while (parent is not null and not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private T findChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T childT)
                    return childT;

                var result = findChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
    #endregion
}
