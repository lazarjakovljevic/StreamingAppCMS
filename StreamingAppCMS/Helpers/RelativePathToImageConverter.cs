using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace StreamingAppCMS.Helpers
{
    public class RelativePathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                string absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                if (File.Exists(absolutePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(absolutePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }

                Console.WriteLine($"Image file not found: {absolutePath}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image from path '{path}': {ex.Message}");
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}