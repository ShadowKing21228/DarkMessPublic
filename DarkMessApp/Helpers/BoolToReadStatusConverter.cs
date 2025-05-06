using System.Globalization;

namespace DarkMessApp.Helpers;

public class BoolToReadStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRead)
        {
            return isRead ? "Прочитано" : "Не прочитано";
        }

        return "Неизвестно";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException(); // Обратное преобразование не нужно
    }
}