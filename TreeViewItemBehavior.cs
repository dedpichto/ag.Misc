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

    ////////////////////////////////////////////////////

    
}

public static class TreeViewBehavior
{
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static object GetSelectedItem(DependencyObject obj) => obj.GetValue(SelectedItemProperty);
    public static void SetSelectedItem(DependencyObject obj, object value) => obj.SetValue(SelectedItemProperty, value);

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TreeView treeView)
            return;

        // First time: subscribe to events
        treeView.Loaded -= TreeView_Loaded;
        treeView.Loaded += TreeView_Loaded;
        treeView.IsVisibleChanged -= TreeView_IsVisibleChanged;
        treeView.IsVisibleChanged += TreeView_IsVisibleChanged;
        treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
        treeView.SelectedItemChanged += TreeView_SelectedItemChanged;

        // Only sync if already loaded and visible
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
            SetSelectedItem(treeView, e.NewValue);
    }

    private static void SyncSelection(TreeView treeView, object item)
    {
        if (item == null || treeView.ItemsSource == null)
            return;

        treeView.Dispatcher.InvokeAsync(() =>
        {
            var container = treeView.ItemContainerGenerator
                .ContainerFromItem(item) as TreeViewItem;

            if (container != null)
            {
                container.IsSelected = true;
                container.BringIntoView();
            }
        }, DispatcherPriority.Loaded);
    }
}
