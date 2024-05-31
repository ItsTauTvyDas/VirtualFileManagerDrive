using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VirtualFileManagerDrive.Common;

namespace UI.Helper;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class ShellIcons
{
    public static object? GetIcon(WindowsApi.ShellIcon icon, bool largeIcon = false, uint size = 0, double rotate = 0)
    {
        var hIcon = WindowsApi.GetIcon(icon, largeIcon);
        if (hIcon == 0)
            return null;
        try
        {
            return new TransformedBitmap(
                Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty,
                    size > 0 ? BitmapSizeOptions.FromWidthAndHeight((int)size, (int)size) : BitmapSizeOptions.FromEmptyOptions()),
                new RotateTransform(rotate));
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            WindowsApi.DestroyIcon(hIcon);
        }
    }
}