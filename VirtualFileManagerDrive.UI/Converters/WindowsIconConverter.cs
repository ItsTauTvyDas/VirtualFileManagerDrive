using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using UI.Helper;
using VirtualFileManagerDrive.Common;

namespace UI.Converters;

public class WindowsIconConverter : MarkupExtension, IValueConverter
{
    public WindowsApi.ShellIcon Icon { get; set; }
    public uint Size { get; set; }
    public bool Large { get; set; }
    public bool ConvertToImage { get; set; } = true;
    public double Rotate { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var icon = ShellIcons.GetIcon(value is uint v ? (WindowsApi.ShellIcon)v : Icon, Large, Size, Rotate);
        return ConvertToImage ? new Image { Source = (ImageSource)icon!} : icon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}