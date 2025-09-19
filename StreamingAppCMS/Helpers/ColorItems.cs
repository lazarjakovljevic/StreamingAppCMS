using System.Windows.Media;

namespace StreamingAppCMS.Helpers
{
    public class ColorItems
    {
        public string ColorName { get; set; }
        public SolidColorBrush Brush { get; set; }
        public Color Color
        {
            get { return Brush.Color; }
        }
    }
}
