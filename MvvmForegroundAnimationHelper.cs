public static class MvvmForegroundAnimationHelper
{
    public static readonly DependencyProperty AnimateFromColorKeyProperty =
        DependencyProperty.RegisterAttached(
            "AnimateFromColorKey", typeof(string), typeof(MvvmForegroundAnimationHelper),
            new PropertyMetadata(null, OnAnimationKeyChanged));

    public static readonly DependencyProperty AnimateToColorKeyProperty =
        DependencyProperty.RegisterAttached(
            "AnimateToColorKey", typeof(string), typeof(MvvmForegroundAnimationHelper),
            new PropertyMetadata(null, OnAnimationKeyChanged));

    public static string GetAnimateFromColorKey(DependencyObject obj) =>
        (string)obj.GetValue(AnimateFromColorKeyProperty);

    public static void SetAnimateFromColorKey(DependencyObject obj, string value) =>
        obj.SetValue(AnimateFromColorKeyProperty, value);

    public static string GetAnimateToColorKey(DependencyObject obj) =>
        (string)obj.GetValue(AnimateToColorKeyProperty);

    public static void SetAnimateToColorKey(DependencyObject obj, string value) =>
        obj.SetValue(AnimateToColorKeyProperty, value);

    private static void OnAnimationKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock textBlock)
        {
            textBlock.Loaded += (s, args) => StartAnimation(textBlock);
        }
    }

    private static void StartAnimation(TextBlock textBlock)
    {
        var fromKey = GetAnimateFromColorKey(textBlock);
        var toKey = GetAnimateToColorKey(textBlock);

        if (fromKey == null || toKey == null) return;

        var fromColor = textBlock.TryFindResource(fromKey) as Color?;
        var toColor = textBlock.TryFindResource(toKey) as Color?;

        if (!fromColor.HasValue || !toColor.HasValue) return;

        var brush = new SolidColorBrush(fromColor.Value);
        textBlock.Foreground = brush;

        var animation = new ColorAnimation
        {
            From = fromColor.Value,
            To = toColor.Value,
            Duration = TimeSpan.FromSeconds(0.5),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever
        };

        brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
    }
}
