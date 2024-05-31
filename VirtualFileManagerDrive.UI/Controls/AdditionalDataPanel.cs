using System.Windows;
using System.Windows.Controls;
using VirtualFileManagerDrive.Common;
using VirtualFileManagerDrive.Core;

namespace UI.Controls;

public class AdditionalDataPanel : StackPanel
{
    public List<AdditionalData> Data
    {
        get => (List<AdditionalData>)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public bool ReadOnly { get; set; }
    
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(List<AdditionalData>), 
            typeof(AdditionalDataPanel),
            new PropertyMetadata(new List<AdditionalData>(), PropertyChangedCallback)
        );

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AdditionalDataPanel panel) return;
        panel.Children.Clear();
        GroupBox? lastGroupBox = null;
        Panel? lastPanel = null;
        var row = 0;
        var added = false;
        foreach (var data in panel.Data)
        {
            if (lastGroupBox is { Header: string header } && !header.Equals(data.Header))
            {
                lastGroupBox = null;
                lastPanel = null;
                added = false;
            }
            
            lastGroupBox ??= new GroupBox { Header = data.Header };
            lastPanel ??= panel.ReadOnly ? new StackPanel() : new Grid();
            
            if (!added)
            {
                panel.Children.Add(lastGroupBox);
                lastGroupBox.Content = lastPanel;
                added = true;
            }

            if (lastPanel is Grid grid)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto});

            if (panel.ReadOnly)
            {
                var dock = new DockPanel();
                dock.Children.Add(new TextBlock { Text = data.Title });
                dock.Children.Add(new SelectableText { Text = (string)data.Value });
                lastPanel.Children.Add(dock);
            }
            else
            {
                var stack = new StackPanel();
                var input = new TextBox { Text = (string)data.Value };
                input.TextChanged += (_, _) =>
                {
                    if (ServerInstance.EditMode)
                        // input.Tag = input.Text;
                        data.Value = input.Text;
                    else
                        data.Value = input.Text;
                };
                stack.SetValue(Grid.RowProperty, row);
                stack.Children.Add(new TextBlock { Text = data.Title });
                stack.Children.Add(input);
                lastPanel.Children.Add(stack);
            }

            row++;
        }
    }
}