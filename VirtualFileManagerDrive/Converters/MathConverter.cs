using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace VirtualFileManagerDrive.Converters;

public class MathConverter : MarkupExtension, IValueConverter
{
    public string? Operator { get; set; }
    public double? Argument { get; set; }
    public bool Opposite { get; set; } = false;
    public bool Round { get; set; } = false;
    public string CastTo { get; set; } = "double";
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var number = (double?)value;
        if (number == null)
            throw new ArgumentNullException(nameof(value), "Number cannot be null.");
        var res = Operator switch
        {
            "+" => number + Argument,
            "-" => Opposite ? Argument - number : number - Argument,
            "*" => number * Argument,
            "/" => Opposite ? Argument / number : number / Argument,
            _ => throw new ArgumentOutOfRangeException()
        };
        if (Round && res != null)
            res = Math.Round((double)res);
        return CastTo switch
        {
            "double" => res,
            "int" => (int?)res,
            "float" => (float?)res,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}