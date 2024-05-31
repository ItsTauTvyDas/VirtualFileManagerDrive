using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using UI.Helper;

namespace UI.Converters;

public class WindowsIconConverter : MarkupExtension, IValueConverter
{
    public string File { get; set; } = "shell32.dll";
    public int Index { get; set; }
    public bool Large { get; set; }
    public bool ConvertToImage { get; set; } = true;
    public double Rotate { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var icon = WindowsApi.GetIcon(File, value is int v ? v : Index, Large, Rotate);
        return ConvertToImage ? new Image { Source = (ImageSource)icon!} : icon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}