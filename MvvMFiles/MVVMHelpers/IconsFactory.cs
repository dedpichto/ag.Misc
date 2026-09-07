using System.Collections.Concurrent;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShayCommon.Mvvm.Helpers
{
    public static class IconsFactory
    {
        private static readonly ConcurrentDictionary<Drawing, Drawing> _drawingCache = new();

        public static Image CreateIcon(DrawingImage source, double width = 16, double height = 16)
        {
            if (source?.Drawing == null) return null;
            var cloneDrawing = _drawingCache.GetOrAdd(source.Drawing, drawing =>
            {
                var clone = drawing.Clone();
                return clone;
            });
            return new Image
            {
                Source = new DrawingImage(cloneDrawing),
                Height = height,
                Width = width
            };
        }

        public static void Clean() => _drawingCache.Clear();
    }
}
