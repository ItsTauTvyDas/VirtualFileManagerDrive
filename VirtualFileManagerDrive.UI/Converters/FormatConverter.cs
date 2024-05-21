using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace UI.Converters;

public class FormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return value;
        return parameter.ToString()!.Replace("[]", value.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return value;
        var param = parameter.ToString()!;
        var split = param.Split("[]");
        return value.ToString()!.Substring(split[0].Length,
            value.ToString()!.Length - split[1].Length - split[0].Length);
    }
}