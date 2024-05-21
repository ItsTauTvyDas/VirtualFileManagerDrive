using System.Globalization;
using System.Windows.Data;
using UI.ViewModels;
using VirtualFileManagerDrive.Core;

namespace UI.Converters;

public class ServerInstanceToViewConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ((ServerInstance?)value)?.View;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return ((ServerInstanceViewModel?)value)?.Instance;
    }
}