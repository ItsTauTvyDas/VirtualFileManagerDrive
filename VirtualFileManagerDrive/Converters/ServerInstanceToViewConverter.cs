using System.Globalization;
using System.Windows.Data;
using VirtualDrive;
using VirtualFileManagerDrive.ViewModels;

namespace VirtualFileManagerDrive.Converters;

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