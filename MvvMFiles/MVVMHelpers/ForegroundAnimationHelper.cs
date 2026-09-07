using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ShayCommon.Mvvm.Helpers
{
    public static class ForegroundAnimationHelper
    {
        public static readonly DependencyProperty AnimateFromColorKeyProperty =
            DependencyProperty.RegisterAttached(
                "AnimateFromColorKey", typeof(string), typeof(ForegroundAnimationHelper),
                new PropertyMetadata(null, onAnimationKeyChanged));
        public static readonly DependencyProperty AnimateToColorKeyProperty =
            DependencyProperty.RegisterAttached(
                "AnimateToColorKey", typeof(string), typeof(ForegroundAnimationHelper),
                new PropertyMetadata(null, onAnimationKeyChanged));
        public static string GetAnimateFromColorKey(DependencyObject obj) =>
            (string)obj.GetValue(AnimateFromColorKeyProperty);
        public static void SetAnimateFromColorKey(DependencyObject obj, string value) =>
            obj.SetValue(AnimateFromColorKeyProperty, value);
        public static string GetAnimateToColorKey(DependencyObject obj) =>
            (string)obj.GetValue(AnimateToColorKeyProperty);
        public static void SetAnimateToColorKey(DependencyObject obj, string value) =>
            obj.SetValue(AnimateToColorKeyProperty, value);
        private static readonly ConditionalWeakTable<TextBlock, AnimationClock> _animations = new();

        private static void onAnimationKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                if (e.OldValue != null)
                {
                    textBlock.Loaded -= onTextBlockLoaded;
                    if (_animations.TryGetValue(textBlock, out var clock))
                    {
                        clock.Controller?.Stop();
                        _animations.Remove(textBlock);
                    }
                }

                if (e.NewValue != null)
                {
                    textBlock.Loaded += onTextBlockLoaded;
                    textBlock.Unloaded += onTextBlockUnloaded;
                }
            }
        }

        private static void onTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
                startAnimation(textBlock);
        }

        private static void onTextBlockUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.Unloaded -= onTextBlockUnloaded;
                if (_animations.TryGetValue(textBlock, out var clock))
                {
                    clock.Controller?.Stop();
                    _animations.Remove(textBlock);
                }
            }
        }

        private static void startAnimation(TextBlock textBlock)
        {
            var fromKey = GetAnimateFromColorKey(textBlock);
            var toKey = GetAnimateToColorKey(textBlock);
            if (string.IsNullOrEmpty(fromKey) || string.IsNullOrEmpty(toKey)) return;

            var fromColor = textBlock.TryFindResource(fromKey) as Color?;
            var toColor = textBlock.TryFindResource(toKey) as Color?;
            if (!fromColor.HasValue || !toColor.HasValue) return;

            var brush = new SolidColorBrush(fromColor.Value);
            textBlock.Foreground = brush;
            var animation = new ColorAnimation
            {
                From = fromColor.Value,
                To = toColor.Value,
                Duration = TimeSpan.FromSeconds(1),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var clock = animation.CreateClock();
            brush.ApplyAnimationClock(SolidColorBrush.ColorProperty, clock);
            _animations.Remove(textBlock);
            _animations.Add(textBlock, clock);
        }

    }
}
