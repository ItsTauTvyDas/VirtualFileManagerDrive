using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace VirtualFileManagerDrive.Common;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class WindowsApi
{
    public static nint GetIcon(ShellIcon icon, bool largeIcon = false)
    {
        var sii = new ShellIconInfo
        {
            cbSize = (uint)Marshal.SizeOf(typeof(ShellIconInfo))
        };

        SHGetStockIconInfo(icon, largeIcon ? ShellIconFlags.Icon : ShellIconFlags.Icon | ShellIconFlags.SmallIcon, ref sii);
        return sii.hIcon;
    }
    
    public const int GWL_STYLE = -16;
    public const int WS_SYSMENU = 0x80000;
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    public enum ShellIcon : uint
    {
        Application = 2, //SIID_APPLICATION
        Folder = 3, //SIID_FOLDER
        Drive35 = 6, //SIID_DRIVE35
        FixedDrive = 8, //SIID_DRIVEFIXED
        NetworkDrive = 9, //SIID_DRIVENET
        DisabledNetworkDrive = 10, //SIID_DRIVENETDISABLED
        FolderWithComputer = 42, //???
        NetworkFolder = 51, //SIID_SERVERSHARE
        Warning = 78, //SIID_WARNING
        Info = 79, //SIID_INFO
        Error = 80, //SIID_ERROR
        Rename = 83, //SIID_RENAME
        Delete = 84, //SIID_DELETE
        NetworkConnect = 103 //SIID_NETWORKCONNECT
    }

    [Flags]
    private enum ShellIconFlags : uint
    {
        SmallIcon = 0x000000001,
        Icon = 0x000000100
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShellIconInfo
    {
        public UInt32 cbSize;
        public IntPtr hIcon;
        public Int32 iSysIconIndex;
        public Int32 iIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szPath;
    }

    [DllImport("Shell32.dll", SetLastError = false)]
    private static extern uint SHGetStockIconInfo(ShellIcon siid, ShellIconFlags uFlags, ref ShellIconInfo psii);
    
    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool DestroyIcon(IntPtr hIcon);
}