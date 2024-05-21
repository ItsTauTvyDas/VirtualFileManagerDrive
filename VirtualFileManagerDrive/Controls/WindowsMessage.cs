using System.Windows;
using System.Windows.Controls;

namespace VirtualFileManagerDrive.Controls;

public class WindowsMessage : TextBlock
{
    public int ResourceId
    {
        get => (int)GetValue(ResourceIdProperty);
        set => SetValue(ResourceIdProperty, value);
    }

    public string FallbackText
    {
        get => (string)GetValue(FallbackTextProperty);
        set => SetValue(FallbackTextProperty, value);
    }
    
    public static readonly DependencyProperty ResourceIdProperty =
        DependencyProperty.Register(nameof(ResourceId), typeof(int),
            typeof(WindowsMessage),
            new PropertyMetadata(default(int))
        );
    
    public static readonly DependencyProperty FallbackTextProperty =
        DependencyProperty.Register(nameof(FallbackText), typeof(string),
            typeof(WindowsMessage),
            new PropertyMetadata(default(string))
        );
}