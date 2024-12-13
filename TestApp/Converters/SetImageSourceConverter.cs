using Scryfall.Models;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TestApp.Converters
{
    public class SetImageSourceConverter : IValueConverter
    {
        private static Dictionary<Uri, ImageSource> _imageSources = new ();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CardSet set && targetType == typeof(ImageSource))
            {
                if (!_imageSources.TryGetValue(set.IconSvgUri, out var bmp))
                {
                    var fileName = Path.GetFileName(set.IconSvgUri.LocalPath).Replace(".svg", ".png");
                    var uri = new Uri($"pack://application:,,,/Images/set/{fileName}");
                    bmp = new BitmapImage(uri);
                    _imageSources[set.IconSvgUri] = bmp;
                }

                return bmp;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
