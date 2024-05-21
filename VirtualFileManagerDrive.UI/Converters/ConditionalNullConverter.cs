using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace UI.Converters;

public class ConditionalNullConverter : MarkupExtension, IValueConverter
{
    public object? NullValue { get; set; }
    public object? NotNullValue { get; set; }
    public bool ConvertNullToEmpty { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var res = value == null ? NullValue : (NotNullValue is string ? NotNullValue?.ToString()?.Replace("%", value.ToString()) : NotNullValue);
        if (ConvertNullToEmpty && res == null)
            return "";
        return res;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}