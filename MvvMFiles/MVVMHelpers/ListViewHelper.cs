using BLL.WPF.LinkLabel;
using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.Commands;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
    public static class ListViewHelper
    {
        public static readonly DependencyProperty ScrollIntoViewProperty =
            DependencyProperty.RegisterAttached("ScrollIntoView", typeof(object), typeof(ListViewHelper), new PropertyMetadata(null, OnScrollIntoViewChanged));

        public static object GetScrollIntoView(DependencyObject obj) => obj.GetValue(ScrollIntoViewProperty);

        public static void SetScrollIntoView(DependencyObject obj, object value) => obj.SetValue(ScrollIntoViewProperty, value);

        private static void OnScrollIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView && e.NewValue != null)
            {
                listView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    listView.UpdateLayout();
                    listView.ScrollIntoView(e.NewValue);
                }));
            }
        }

        #region ColumnsHeaders
        public static readonly DependencyProperty ColumnsHeadersCallbackProperty =
            DependencyProperty.RegisterAttached("ColumnsHeadersCallback",
                typeof(Action<IEnumerable<string>>),
                typeof(ListViewHelper),
                new PropertyMetadata(null, onColumnsHeadersCallbackChanged));

        public static Action<IEnumerable<string>> GetColumnsHeadersCallback(DependencyObject obj) => (Action<IEnumerable<string>>)obj.GetValue(ColumnsHeadersCallbackProperty);

        public static void SetColumnsHeadersCallback(DependencyObject obj, Action<IEnumerable<string>> value) => obj.SetValue(ColumnsHeadersCallbackProperty, value);

        private static readonly ConditionalWeakTable<ListView, EventHandlers> _listViewHandlers = new();

        private class EventHandlers
        {
            public RoutedEventHandler LoadedHandler;
            public NotifyCollectionChangedEventHandler CollectionChangedHandler;
        }

        private static void onColumnsHeadersCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView)
            {
                if (e.OldValue != null && _listViewHandlers.TryGetValue(listView, out var oldHandlers))
                {
                    listView.Loaded -= oldHandlers.LoadedHandler;
                    if (listView.View is GridView oldGridView)
                        oldGridView.Columns.CollectionChanged -= oldHandlers.CollectionChangedHandler;
                    _listViewHandlers.Remove(listView);
                }

                if (e.NewValue != null)
                {
                    var handlers = new EventHandlers
                    {
                        LoadedHandler = (s, args) => reportColumnHeaders(listView),
                        CollectionChangedHandler = (s, args) => reportColumnHeaders(listView)
                    };

                    listView.Loaded += handlers.LoadedHandler;
                    if (listView.View is GridView gridView)
                        gridView.Columns.CollectionChanged += handlers.CollectionChangedHandler;

                    _listViewHandlers.Add(listView, handlers);
                }
            }
        }

        private static void reportColumnHeaders(ListView listView)
        {
            var callback = GetColumnsHeadersCallback(listView);
            if (callback != null && listView.View is GridView gridView)
            {
                var headers = gridView.Columns.Select(c => c.Header?.ToString() ?? string.Empty);
                callback(headers);
            }
        }
        #endregion

        #region ColumnsHeadersAndWidth
        public static readonly DependencyProperty ColumnsHeadersAndWidthCallbackProperty =
            DependencyProperty.RegisterAttached("ColumnsHeadersAndWidthCallback",
                typeof(Action<IEnumerable<(string, double)>>),
                typeof(ListViewHelper),
                new PropertyMetadata(null, onColumnsHeadersAndWidthCallbackChanged));

        public static Action<IEnumerable<(string, double)>> GetColumnsHeadersAndWidthCallback(DependencyObject obj) => (Action<IEnumerable<(string, double)>>)obj.GetValue(ColumnsHeadersAndWidthCallbackProperty);

        public static void SetColumnsHeadersAndWidthCallback(DependencyObject obj, Action<IEnumerable<(string, double)>> value) => obj.SetValue(ColumnsHeadersAndWidthCallbackProperty, value);

        private static readonly ConditionalWeakTable<ListView, EventHandlers> _listViewWidthHandlers = new();

        private static void onColumnsHeadersAndWidthCallbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView)
            {
                if (e.OldValue != null && _listViewWidthHandlers.TryGetValue(listView, out var oldHandlers))
                {
                    listView.Loaded -= oldHandlers.LoadedHandler;
                    if (listView.View is GridView oldGridView)
                        oldGridView.Columns.CollectionChanged -= oldHandlers.CollectionChangedHandler;
                    _listViewWidthHandlers.Remove(listView);
                }

                if (e.NewValue != null)
                {
                    var handlers = new EventHandlers
                    {
                        LoadedHandler = (s, args) => reportColumnHeadersAndWidth(listView),
                        CollectionChangedHandler = (s, args) => reportColumnHeadersAndWidth(listView)
                    };

                    listView.Loaded += handlers.LoadedHandler;
                    if (listView.View is GridView gridView)
                        gridView.Columns.CollectionChanged += handlers.CollectionChangedHandler;

                    _listViewWidthHandlers.Add(listView, handlers);
                }
            }
        }

        private static void reportColumnHeadersAndWidth(ListView listView)
        {
            var callback = GetColumnsHeadersAndWidthCallback(listView);
            if (callback != null && listView.View is GridView gridView)
            {
                var headersAndWidth = gridView.Columns.Select(c => (c.Header?.ToString() ?? string.Empty, c.ActualWidth));
                callback(headersAndWidth);
            }
        }
        #endregion
    }

    #region Double click and column
    public class ListViewDoubleClickBehavior : Behavior<ListView>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ListViewDoubleClickBehavior));

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
            if (AssociatedObject is ListView listView)
            {
                var element = e.OriginalSource as DependencyObject;
                while (element != null && !(element is ListViewItem))
                    element = VisualTreeHelper.GetParent(element);
                if (element == null) return;

                var selectedItem = listView.SelectedItem;
                var parameter = new ListViewDoubleClickParameters
                {
                    SelectedItem = selectedItem,
                    ControlName = listView.Name
                };
                if (listView.View is GridView gridView)
                {
                    var mousePosition = e.GetPosition(listView);
                    parameter.Column = getColumnFromPosition(gridView, mousePosition.X);
                }

                if (Command != null && Command.CanExecute(parameter))
                {
                    Command.Execute(parameter);
                }
            }
        }

        private GridViewColumn getColumnFromPosition(GridView gridView, double x)
        {
            double currentPosition = 0;

            foreach (var column in gridView.Columns)
            {
                var columnWidth = column.ActualWidth;
                if (x >= currentPosition && x < currentPosition + columnWidth)
                {
                    return column;
                }
                currentPosition += columnWidth;
            }

            return null;
        }
    }

    public class ListViewMouseUpBehavior : Behavior<ListView>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ListViewMouseUpBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonUp += onMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonUp -= onMouseLeftButtonUp;
        }

        private void onMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject is ListView listView)
            {
                var selectedItem = listView.SelectedItem;
                var parameter = new ListViewMouseActionParameters
                {
                    SelectedItem = selectedItem,
                    ControlName = listView.Name
                };

                if (Command != null && Command.CanExecute(parameter))
                {
                    Command.Execute(parameter);
                }
            }
        }

    }

    public class ListViewDoubleClickWithToggleButtonBehavior : Behavior<ListView>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ListViewDoubleClickWithToggleButtonBehavior));

        public static readonly DependencyProperty ToggleButtonProperty =
            DependencyProperty.Register(nameof(ToggleButton),
                typeof(ToggleButton),
                typeof(ListViewDoubleClickWithToggleButtonBehavior));

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
            if (AssociatedObject is ListView listView)
            {
                var selectedItem = listView.SelectedItem;
                var parameter = new ListViewDoubleClickParameters
                {
                    SelectedItem = selectedItem,
                    ControlName = listView.Name
                };
                if (listView.View is GridView gridView)
                {
                    var mousePosition = e.GetPosition(listView);
                    parameter.Column = getColumnFromPosition(gridView, mousePosition.X);
                }

                if (Command != null && Command.CanExecute(parameter))
                {
                    Command.Execute(parameter);
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

        private GridViewColumn getColumnFromPosition(GridView gridView, double x)
        {
            double currentPosition = 0;

            foreach (var column in gridView.Columns)
            {
                var columnWidth = column.ActualWidth;
                if (x >= currentPosition && x < currentPosition + columnWidth)
                {
                    return column;
                }
                currentPosition += columnWidth;
            }

            return null;
        }
    }

    public class ListViewBehavior : Behavior<ListView>
    {
        private RoutedEventHandler _loadedHandler;

        protected override void OnAttached()
        {
            base.OnAttached();
            _loadedHandler=async(sener,e) => await Dispatcher.InvokeAsync(() =>
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
                    viewModel.RegisterClearColumnsAction(extTag, () =>
                    {
                        if (AssociatedObject.View is GridView gridView)
                        {
                            foreach (var column in gridView.Columns)
                            {
                                if (column.Header is LinkLabel linkLabel)
                                {
                                    if (linkLabel.Command is IUICommand command)
                                    {
                                        command.Dispose();
                                    }
                                }
                            }
                            gridView.Columns.Clear();
                        }
                    });
                    viewModel.RegisterAddListColumnsAction(extTag, (columns) =>
                    {
                        if (AssociatedObject.View is GridView gridView)
                        {
                            gridView.Columns.AddRange(columns);
                        }
                    });
                    viewModel.RegisterSetListViewSelectedIndexAction(extTag, (index) =>
                    {
                        AssociatedObject.SelectedIndex = index;
                    });
                    viewModel.RegisterSetItemsSourceAction(extTag, (source) =>
                    {
                        AssociatedObject.ItemsSource = source;
                    });
                    viewModel.RegisterSetListViewColumnsHeadersAction(extTag, (headers) =>
                    {
                        if (AssociatedObject.View is GridView gridView)
                        {
                            var arr = headers.ToArray();
                            for (var i = 0; i < arr.Length; i++)
                            {
                                gridView.Columns[i].Header = arr[i];
                            }
                        }
                    });
                    viewModel.RegisterGetControlFunction(extTag, () =>
                    {
                        return AssociatedObject;
                    });
                    viewModel.RegisterScrollIntoAction(extTag, (item) =>
                    {
                        AssociatedObject.UpdateLayout();
                        AssociatedObject.ScrollIntoView(item);
                    });
                }
                else if (viewModel != null && AssociatedObject.Tag is Guid guid)
                {
                    viewModel.RegisterClearColumnsAction(guid, () =>
                    {
                        if (AssociatedObject.View is GridView gridView)
                        {
                            foreach (var column in gridView.Columns)
                            {
                                if (column.Header is LinkLabel linkLabel)
                                {
                                    if (linkLabel.Command is IUICommand command)
                                    {
                                        command.Dispose();
                                    }
                                }
                            }
                            gridView.Columns.Clear();
                        }
                    });
                    viewModel.RegisterAddListColumnsAction(guid, (columns) =>
                    {
                        if (AssociatedObject.View is GridView gridView)
                        {
                            gridView.Columns.AddRange(columns);
                        }
                    });
                    viewModel.RegisterSetListViewSelectedIndexAction(guid, (index) =>
                    {
                        AssociatedObject.SelectedIndex = index;
                    });
                    viewModel.RegisterSetItemsSourceAction(guid, (source) =>
                    {
                        AssociatedObject.ItemsSource = source;
                    });
                    viewModel.RegisterSetListViewColumnsHeadersAction(guid, (headers) =>
                    {
                        if (AssociatedObject.View is GridView gridView)
                        {
                            var arr = headers.ToArray();
                            for (var i = 0; i < arr.Length; i++)
                            {
                                gridView.Columns[i].Header = arr[i];
                            }
                        }
                    });
                    viewModel.RegisterGetControlFunction(guid, () =>
                    {
                        return AssociatedObject;
                    });
                    viewModel.RegisterScrollIntoAction(guid, (item) =>
                    {
                        AssociatedObject.UpdateLayout();
                        AssociatedObject.ScrollIntoView(item);
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
    }

    public class ListViewDoubleClickParameters
    {
        public object SelectedItem { get; set; }
        public GridViewColumn Column { get; set; }
        public string ControlName { get; set; }
    }

    public class ListViewMouseActionParameters
    {
        public object SelectedItem { get; set; }
        public string ControlName { get; set; }
    }
    #endregion
}
