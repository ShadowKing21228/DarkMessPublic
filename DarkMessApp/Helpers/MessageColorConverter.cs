using System.Globalization;

namespace DarkMessApp.Helpers;

public class MessageColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "#7B2CBF" : "Gray"; // Свои сообщения - фиолетовые, чужие - серые
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}