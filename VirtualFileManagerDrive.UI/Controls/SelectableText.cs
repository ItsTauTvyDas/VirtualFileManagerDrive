using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace UI.Controls;

public class SelectableText : TextBox
{
    public SelectableText()
    {
        TextWrapping = TextWrapping.Wrap;
        IsReadOnly = true;
        BorderThickness = new Thickness(0);
        Background = Brushes.Transparent;
    }
}