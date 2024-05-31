using System.Windows;
using System.Windows.Interop;
using VirtualFileManagerDrive.Common;

namespace UI;

public partial class CustomMessageBox
{
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public CustomMessageBox()
    {
        Loaded += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsApi.SetWindowLong(hwnd, WindowsApi.GWL_STYLE, WindowsApi.GetWindowLong(hwnd, WindowsApi.GWL_STYLE) & ~WindowsApi.WS_SYSMENU);
        };
        InitializeComponent(); 
    }
    
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), 
            typeof(CustomMessageBox), new PropertyMetadata(default(string))
        );
}