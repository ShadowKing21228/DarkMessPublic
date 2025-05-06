using System.Globalization;

namespace DarkMessApp.Helpers;

public class DateTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime date)
        {
            return date.Date == DateTime.Today
                ? date.ToString("HH:mm") 
                : date.ToString("dd.MM.yyyy");
        }
        return string.Empty;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}