using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using UI.Helper;
using VirtualFileManagerDrive.Common;

namespace UI.Controls;

public class WindowsIcon : Image
{
    public WindowsApi.ShellIcon Icon
    {
        get => (WindowsApi.ShellIcon)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public uint Size
    {
        get => (uint)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
    
    public bool Large
    {
        get => (bool)GetValue(LargeProperty);
        set => SetValue(LargeProperty, value);
    }

    public double Rotation
    {
        get => (double)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public bool IsCorneredIcon
    {
        get => (bool)GetValue(IsCorneredIconProperty);
        set => SetValue(IsCorneredIconProperty, value);
    }

    private static void UpdateProperties(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var control = (WindowsIcon)obj;
        control.Source = (BitmapSource?)ShellIcons.GetIcon(
            control.Icon,
            control.Large,
            control.Size,
            control.Rotation);
    }
    
    public static readonly DependencyProperty IsCorneredIconProperty =
        DependencyProperty.Register(nameof(IsCorneredIcon), typeof(bool),
            typeof(WindowsIcon),
            new PropertyMetadata(default(bool), (obj, args) =>
            {
                var control = (WindowsIcon)obj;
                if (!(bool)args.NewValue) return;
                control.Width = 10;
                control.VerticalAlignment = VerticalAlignment.Bottom;
                control.HorizontalAlignment = HorizontalAlignment.Right;
            })
        );

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(WindowsApi.ShellIcon),
            typeof(WindowsIcon),
            new PropertyMetadata(default(WindowsApi.ShellIcon), UpdateProperties)
    );
    
    public static readonly DependencyProperty RotationProperty =
        DependencyProperty.Register(nameof(Rotation), typeof(double), 
            typeof(WindowsIcon),
            new PropertyMetadata(default(double), UpdateProperties)
        );
    
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(uint), 
            typeof(WindowsIcon),
            new PropertyMetadata(default(uint), UpdateProperties)
        );
    
    public static readonly DependencyProperty LargeProperty =
        DependencyProperty.Register(nameof(Large), typeof(bool), 
            typeof(WindowsIcon),
            new PropertyMetadata(default(bool), UpdateProperties)
        );
}