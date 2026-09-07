using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace ShayCommon.Mvvm.Helpers
{
    public static class GridViewColumnHelper
    {
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(GridViewColumnHelper),
                new PropertyMetadata(true, onIsVisibleChanged));

        public static bool GetIsVisible(DependencyObject obj) => (bool)obj.GetValue(IsVisibleProperty);
        public static void SetIsVisible(DependencyObject obj, bool value) => obj.SetValue(IsVisibleProperty, value);

        private static readonly DependencyProperty OriginalWidthProperty =
            DependencyProperty.RegisterAttached("OriginalWidth", typeof(double), typeof(GridViewColumnHelper));

        private static void onIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridViewColumn column)
            {
                var isVisible = (bool)e.NewValue;

                if (!isVisible)
                {
                    if (column.Width > 0)
                        column.SetValue(OriginalWidthProperty, column.Width);
                    column.Width = 0;
                }
                else
                {
                    var originalWidth = column.GetValue(OriginalWidthProperty);
                    if (originalWidth != null)
                        column.Width = (double)originalWidth;
                }
            }
        }
    }
}
