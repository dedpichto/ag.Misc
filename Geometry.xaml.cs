using System.Windows;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Geometry.xaml
    /// </summary>
    public partial class Geometry : Window
    {
        public Geometry()
        {
            InitializeComponent();
        }

        private DrawingImage loadImage(GeometryGroup geometryGroup, Brush penBrush)
        {

            var geometryDrawing = new GeometryDrawing(Brushes.Transparent, new Pen(penBrush, 1), geometryGroup);
            var geometryImage = new DrawingImage(geometryDrawing);

            // Freeze the DrawingImage for performance benefits.
            geometryImage.Freeze();

            return geometryImage;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var gd = (GeometryGroup)TryFindResource("gmShay");
            if (gd != null)
            {
                imgShay.Source = loadImage(gd, Brushes.Red);
            }
            gd = (GeometryGroup)TryFindResource("gmCalc");
            if (gd != null)
            {
                imgCalc.Source = loadImage(gd, Brushes.Blue);
            }
            gd = (GeometryGroup)TryFindResource("gmUtils");
            if (gd != null)
            {
                imgUtils.Source = loadImage(gd, Brushes.Magenta);
            }
            //System.Windows.FontStyle fontStyle = FontStyles.Normal;
            //FontWeight fontWeight = FontWeights.Bold;

            //// Create the formatted text based on the properties set.
            //FormattedText formattedText = new FormattedText(
            //    "S",
            //    CultureInfo.GetCultureInfo("en-us"),
            //    FlowDirection.LeftToRight,
            //    new Typeface(
            //        new System.Windows.Media.FontFamily("Arial"),
            //        fontStyle,
            //        fontWeight,
            //        FontStretches.Normal),
            //    17,
            //    System.Windows.Media.Brushes.Black // This brush does not matter since we use the geometry of the text.
            //    );

            //// Build the geometry object that represents the text.
            //var textGeometry =(GeometryGroup) formattedText.BuildGeometry(new System.Windows.Point(4.2, 0.8));
        }
    }
}
