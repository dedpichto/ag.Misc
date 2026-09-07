using Microsoft.Xaml.Behaviors;
using ShayCommon.Mvvm.ViewModels;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ShayCommon.Mvvm.Helpers
{
#nullable enable
    public static class TreeViewHelper
    {

        private static readonly ConditionalWeakTable<TreeViewItem, object> _itemsCash = new();

        #region TreeViewItem IsExpanded property
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.RegisterAttached(
                "IsExpanded",
                typeof(bool),
                typeof(TreeViewHelper),
                new PropertyMetadata(false, OnIsExpandedChanged));
        public static bool GetIsExpanded(DependencyObject obj) =>
            (bool)obj.GetValue(IsExpandedProperty);
        public static void SetIsExpanded(DependencyObject obj, bool value) =>
            obj.SetValue(IsExpandedProperty, value);
        #endregion

        #region TreeView SelectedItem property
        public static readonly DependencyProperty SelectedItemProperty =
           DependencyProperty.RegisterAttached(
               "SelectedItem",
               typeof(object),
               typeof(TreeViewHelper),
               new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static object GetSelectedItem(DependencyObject obj) =>
            obj.GetValue(SelectedItemProperty);
        public static void SetSelectedItem(DependencyObject obj, object value) =>
            obj.SetValue(SelectedItemProperty, value);

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeViewItem item && e.NewValue is bool newValue)
            {
                item.IsExpanded = newValue;
            }
        }
        #endregion

        #region TreeView MonitorSelection property
        public static readonly DependencyProperty MonitorSelectionProperty =
            DependencyProperty.RegisterAttached(
                "MonitorSelection",
                typeof(bool),
                typeof(TreeViewHelper),
                new PropertyMetadata(false, OnMonitorSelectionChanged));
        public static bool GetMonitorSelection(DependencyObject obj) =>
            (bool)obj.GetValue(MonitorSelectionProperty);
        public static void SetMonitorSelection(DependencyObject obj, bool value) =>
            obj.SetValue(MonitorSelectionProperty, value);

        private static void OnMonitorSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeView treeView)
            {
                if (e.OldValue is bool oldVal && oldVal)
                    treeView.SelectedItemChanged -= onTreeViewSelectedItemChanged;
                if (e.NewValue is bool newVal && newVal)
                    treeView.SelectedItemChanged += onTreeViewSelectedItemChanged;
            }
        }

        private static void onTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
        {
            if (sender is TreeView tv)
                SetSelectedItem(tv, args.NewValue);
        }
        #endregion

        #region TreeView ExpandedCommand property
        public static readonly DependencyProperty ExpandedCommandProperty =
            DependencyProperty.RegisterAttached(
                "ExpandedCommand",
                typeof(ICommand),
                typeof(TreeViewHelper),
                new PropertyMetadata(null, OnChanged)
                );

        public static ICommand? GetExpandedCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(ExpandedCommandProperty);
        public static void SetExpandedCommand(DependencyObject obj, ICommand? value) =>
            obj.SetValue(ExpandedCommandProperty, value);

        private static void OnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TreeViewItem item)
            {
                if (e.OldValue != null)
                {
                    item.Expanded -= OnExpanded;
                    _itemsCash.Remove(item);
                }

                if (e.NewValue != null && !_itemsCash.TryGetValue(item, out _))
                {
                    item.Expanded += OnExpanded;
                    _itemsCash.Add(item, new object());
                }
            }
        }

        private static void OnExpanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem item && GetExpandedCommand(item) is ICommand command)
            {
                if (command == null) return;
                var dataContext = item.DataContext;
                if (command.CanExecute(dataContext))
                    command.Execute(dataContext);
            }
        }
        #endregion

        #region TreeView ScrollIntoView property
        public static readonly DependencyProperty ScrollIntoViewProperty =
            DependencyProperty.RegisterAttached("ScrollIntoView", typeof(object), typeof(TreeViewHelper), new PropertyMetadata(null, OnScrollIntoViewChanged));

        public static object GetScrollIntoView(DependencyObject obj) => obj.GetValue(ScrollIntoViewProperty);

        public static void SetScrollIntoView(DependencyObject obj, object value) => obj.SetValue(ScrollIntoViewProperty, value);

        private static void OnScrollIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeView treeView && e.NewValue != null)
            {
                treeView.Dispatcher.BeginInvoke(new Action(() =>
                {
                    treeView.UpdateLayout();
                    var item = getTreeViewItem(treeView, e.NewValue);
                    if (item != null)
                        item.BringIntoView();
                }));
            }
        }

        private static TreeViewItem? getTreeViewItem(ItemsControl container, object item)
        {
            if (container == null)
                return null;
            if (container.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return null;

            var directContainer = container.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (directContainer != null)
                return directContainer;

            foreach (var childItem in container.Items)
            {
                var childContainer = container.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                if (childContainer == null)
                    continue;
                var result = getTreeViewItem(childContainer, item);
                if (result != null)
                    return result;
            }

            return null;
        }
        #endregion
    }
#nullable disable

    public class TreeViewDoubleClickBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TreeViewDoubleClickBehavior));

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
            if (AssociatedObject is TreeView treeView)
            {
                var selectedItem = treeView.SelectedItem;
                var parameter = new TreeViewDoubleClickParameters
                {
                    SelectedItem = selectedItem,
                    ControlName = treeView.Name
                };

                if (Command != null && Command.CanExecute(parameter))
                {
                    Command.Execute(parameter);
                }
            }
        }
    }

    public class TreeViewBehavior : Behavior<TreeView>
    {
        private RoutedEventHandler _loadedHandler;

        public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewBehavior),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged,
                CoerceSelectedItem));
        public static object GetSelectedItem(DependencyObject obj) => obj.GetValue(SelectedItemProperty);
        public static void SetSelectedItem(DependencyObject obj, object value) => obj.SetValue(SelectedItemProperty, value);
        private static object CoerceSelectedItem(DependencyObject d, object baseValue)
        {
            if (d is TreeView treeView)
            {
                EnsureSubscribed(treeView);
            }
            return baseValue;
        }
        private static void EnsureSubscribed(TreeView treeView)
        {
            if (treeView.GetValue(IsSubscribedProperty) is bool subscribed && subscribed)
                return;
            treeView.SetValue(IsSubscribedProperty, true);
            treeView.Loaded += TreeView_Loaded;
            treeView.IsVisibleChanged += TreeView_IsVisibleChanged;
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
            treeView.Unloaded += TreeView_Unloaded;
        }

        private static void TreeView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                treeView.Loaded -= TreeView_Loaded;
                treeView.IsVisibleChanged -= TreeView_IsVisibleChanged;
                treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
                treeView.Unloaded -= TreeView_Unloaded;
                treeView.SetValue(IsSubscribedProperty, false);
            }
        }

        private static readonly DependencyProperty IsSubscribedProperty =
            DependencyProperty.RegisterAttached(
                "IsSubscribed",
                typeof(bool),
                typeof(TreeViewBehavior),
                new PropertyMetadata(false));
        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(TreeViewBehavior),
                new PropertyMetadata(false));
        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TreeView treeView)
                return;
            if (treeView.IsLoaded && treeView.IsVisible)
            {
                SyncSelection(treeView, e.NewValue);
            }
        }
        private static void TreeView_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeView treeView && treeView.IsVisible)
            {
                SyncSelection(treeView, GetSelectedItem(treeView));
            }
        }
        private static void TreeView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeView treeView && (bool)e.NewValue && treeView.IsLoaded)
            {
                SyncSelection(treeView, GetSelectedItem(treeView));
            }
        }
        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                if (treeView.GetValue(IsUpdatingProperty) is bool updating && updating)
                    return;
                treeView.SetValue(IsUpdatingProperty, true);
                SetSelectedItem(treeView, e.NewValue);
                treeView.SetValue(IsUpdatingProperty, false);
            }
        }
        private static void SyncSelection(TreeView treeView, object item)
        {
            if (item == null || treeView.ItemsSource == null)
                return;
            treeView.Dispatcher.InvokeAsync(() =>
            {
                if (treeView.GetValue(IsUpdatingProperty) is bool updating && updating)
                    return;
                treeView.SetValue(IsUpdatingProperty, true);
                var container = FindTreeViewItem(treeView, item);
                if (container != null)
                {
                    container.IsSelected = true;
                    container.BringIntoView();
                }
                treeView.SetValue(IsUpdatingProperty, false);
            }, DispatcherPriority.Loaded);
        }

        private static TreeViewItem FindTreeViewItem(ItemsControl container, object item)
        {
            if (container == null)
                return null;
            var directContainer = container.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (directContainer != null)
                return directContainer;
            foreach (var childItem in container.Items)
            {
                var childContainer = container.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                if (childContainer != null)
                {
                    var result = FindTreeViewItem(childContainer, item);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            _loadedHandler=async(sender,e) => await Dispatcher.InvokeAsync(() =>
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
                    viewModel.RegisterSetItemsSourceAction(extTag, (source) =>
                    {
                        AssociatedObject.ItemsSource = source;
                    });
                    viewModel.RegisterGetControlFunction(extTag, () =>
                    {
                        return AssociatedObject;
                    });
                }
                else if (viewModel != null && AssociatedObject.Tag is Guid guid)
                {
                    viewModel.RegisterSetItemsSourceAction(guid, (source) =>
                    {
                        AssociatedObject.ItemsSource = source;
                    });
                    viewModel.RegisterGetControlFunction(guid, () =>
                    {
                        return AssociatedObject;
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

        private static TreeViewItem getTreeViewItem(ItemsControl container, object item)
        {
            if (container == null)
                return null;
            if (container.DataContext == item)
                return container as TreeViewItem;
            if (container is TreeViewItem tvi && tvi.IsExpanded)
            {
                container.ApplyTemplate();
                container.UpdateLayout();
            }
            for (var i = 0; i < container.Items.Count; i++)
            {
                var subContainer = (TreeViewItem)container.ItemContainerGenerator.ContainerFromIndex(i);
                if (subContainer != null)
                {
                    var targetItem = getTreeViewItem(subContainer, item);
                    if (targetItem != null)
                        return targetItem;
                }
            }
            return null;
        }
    }

    public static class TreeViewItemBehavior
    {
        public static readonly DependencyProperty SelectParentOnClickProperty =
            DependencyProperty.RegisterAttached(
                "SelectParentOnClick",
                typeof(bool),
                typeof(TreeViewItemBehavior),
                new PropertyMetadata(false, OnSelectParentOnClickChanged));
        public static bool GetSelectParentOnClick(DependencyObject obj) => (bool)obj.GetValue(SelectParentOnClickProperty);
        public static void SetSelectParentOnClick(DependencyObject obj, bool value) => obj.SetValue(SelectParentOnClickProperty, value);
        private static void OnSelectParentOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeViewItem item)
            {
                if ((bool)e.NewValue)
                {
                    item.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
                    item.PreviewKeyDown += Item_PreviewKeyDown;
                }
                else
                {
                    item.PreviewMouseLeftButtonDown -= Item_PreviewMouseLeftButtonDown;
                    item.PreviewKeyDown -= Item_PreviewKeyDown;
                }
            }
        }
        private static void Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectParentAndHandle(sender, () => e.Handled = true);
        }
        private static void Item_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                SelectParentAndHandle(sender, () => e.Handled = true);
            }
        }
        private static void SelectParentAndHandle(object sender, Action markHandled)
        {
            if (sender is not TreeViewItem childItem)
                return;
            // Find parent TreeViewItem
            var parent = VisualTreeHelper.GetParent(childItem);
            while (parent != null && parent is not TreeViewItem)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent is TreeViewItem parentItem)
            {
                parentItem.IsSelected = true;
                parentItem.BringIntoView();
                markHandled();
            }
        }
    }

    public class TreeViewDoubleClickParameters
    {
        public object SelectedItem { get; set; }
        public string ControlName { get; set; }
    }

}
