public static class TreeViewBehavior
{
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(TreeViewBehavior),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged,
                CoerceSelectedItem));  // <-- add this

    public static object GetSelectedItem(DependencyObject obj) => obj.GetValue(SelectedItemProperty);
    public static void SetSelectedItem(DependencyObject obj, object value) => obj.SetValue(SelectedItemProperty, value);

    // Coerce always runs, even null -> null
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
        // Check if already subscribed using a private attached property
        if (treeView.GetValue(IsSubscribedProperty) is bool subscribed && subscribed)
            return;

        treeView.SetValue(IsSubscribedProperty, true);

        treeView.Loaded += TreeView_Loaded;
        treeView.IsVisibleChanged += TreeView_IsVisibleChanged;
        treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
    }

    // Private marker to avoid double subscription
    private static readonly DependencyProperty IsSubscribedProperty =
        DependencyProperty.RegisterAttached(
            "IsSubscribed",
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
            SetSelectedItem(treeView, e.NewValue);
        }
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
