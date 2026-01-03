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
