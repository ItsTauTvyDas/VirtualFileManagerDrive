using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UI;

public static class Windows
{
    public static int Count(string file) => ExtractIconEx(file, -1, out _, out _, 0);
    
    public static object? GetIcon(string file, int number, bool largeIcon, double rotate = 0)
    {
        ExtractIconEx(file, number, out var large, out var small, 1);
        try
        {
            return new TransformedBitmap(
                Imaging.CreateBitmapSourceFromHIcon(largeIcon ? large : small, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()),
                new RotateTransform(rotate));
        }
        catch
        {
            return null;
        }
    }
    
    public static string? GetString(string file, uint resourceId, string? defaultValue = null)
    {
        var buffer = new StringBuilder(1024);
        var indirectString = $"@{file},-{resourceId}";
        var result = SHLoadIndirectString(indirectString, buffer, buffer.Capacity, IntPtr.Zero);;
        return result == 0 ? buffer.ToString() : defaultValue;
    }

    [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);
    
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);
}