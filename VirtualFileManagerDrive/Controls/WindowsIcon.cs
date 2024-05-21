using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace VirtualFileManagerDrive.Controls;

public class WindowsIcon : Image
{
    public string File
    {
        get => (string)GetValue(FileProperty);
        set => SetValue(FileProperty, value);
    }

    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public double Rotation
    {
        get => (double)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public bool Large
    {
        get => (bool)GetValue(LargeProperty);
        set => SetValue(LargeProperty, value);
    }

    public bool IsCorneredIcon
    {
        get => (bool)GetValue(IsCorneredIconProperty);
        set => SetValue(IsCorneredIconProperty, value);
    }

    public static readonly DependencyProperty FileProperty =
        DependencyProperty.Register(nameof(File), typeof(string),
            typeof(WindowsIcon),
            new PropertyMetadata("shell32.dll")
        );
    
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

    public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(nameof(Index), typeof(int),
            typeof(WindowsIcon),
            new PropertyMetadata(default(int), (obj, args) =>
            {
                var control = (WindowsIcon)obj;
                control.Source = (BitmapSource?)Windows.GetIcon(control.File, (int)args.NewValue, control.Large,
                    control.Rotation);
            })
);

public static readonly DependencyProperty LargeProperty =
        DependencyProperty.Register(nameof(Large), typeof(bool), 
            typeof(WindowsIcon),
            new PropertyMetadata(default(bool))
        );
    
    public static readonly DependencyProperty RotationProperty =
        DependencyProperty.Register(nameof(Rotation), typeof(double), 
            typeof(WindowsIcon),
            new PropertyMetadata(default(double))
        );
}