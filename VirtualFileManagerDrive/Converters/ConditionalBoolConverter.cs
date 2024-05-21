using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace VirtualFileManagerDrive.Converters;

public class ConditionalBoolConverter : MarkupExtension, IValueConverter
{
    public object? TrueValue { get; set; }
    public object? FalseValue { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return FalseValue;
        return (bool)value ? TrueValue : FalseValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value == TrueValue;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}