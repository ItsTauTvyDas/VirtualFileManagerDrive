using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using UI.Controls;
using UI.ViewModels;
using VirtualFileManagerDrive.Common;
using VirtualFileManagerDrive.Core;

namespace UI;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ServerInstance.SavedServers.CollectionChanged += (_, _) =>
        {
            UnselectSelectedServer();
            ServerList.Items.Refresh();
        };
        ServerList.ItemsSource = ServerInstance.SavedServers;
    }

    public void UnselectSelectedServer()
    {
        ServerInstance.SelectedServer = -1;
        ServerList.UnselectAll();
    }
    
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var source = PresentationSource.FromVisual(this) as HwndSource;
        source?.AddHook(WndProc);
        Console.WriteLine("WndProc hook added");
    }
    
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const int DBT_DEVICEARRIVAL = 0x8000;
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const int WM_DEVICECHANGE = 0x0219;
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WM_DEVICECHANGE || wParam is not (DBT_DEVICEARRIVAL or DBT_DEVICEREMOVECOMPLETE)) return IntPtr.Zero;
        Console.WriteLine("Device added/removed");
        return IntPtr.Zero;
    }

    private void UpdateSelectedServerView()
    {
        ((ServerInstanceViewModel?)ServerInstance.SelectedServerObject?.View)?.Update();
        ServerList.Items.Refresh();
    }

    private void AddServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new AddServerWindow
        {
            Owner = this,
            Title = "Add New Server"
        };
        dialog.ShowDialog();
        ServerList.Focus();
        ServerList.SelectedIndex = ServerInstance.SelectedServer;
    }
    
    private void ServerList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ServerInstance.SelectedServer = ServerList.SelectedIndex;
    }

    private void RemoveServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ServerList.SelectedItem != null)
            ServerInstance.SavedServers.RemoveAt(ServerList.SelectedIndex);
    }

    private void InstallDokany_OnClick(object sender, RoutedEventArgs e)
    {
        var message = MessageBox.Show(this,
            "Do you want to automatically download Dokany and install it?",
            "Install automatically?",
            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.None);
        switch (message)
        {
            case MessageBoxResult.No:
                Process.Start(new ProcessStartInfo("https://github.com/dokan-dev/dokany/releases")
                {
                    UseShellExecute = true
                });
                break;
            case MessageBoxResult.Yes:
                
                break;
        }
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            UnselectSelectedServer();
    }

    private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e) => RootElement.Focus();

    private void ContextMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        if (ServerList.SelectedIndex == -1) return;
        var header = ((MenuItem)sender).Header.ToString();
        var server = ServerInstance.SelectedServerObject;
        if (server == null)
        {
            MessageBox.Show(this, "Server isntance is not set!", "An error occurred!", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }
        int newIndex;
        switch (header)
        {
            case "Mount":
                server.SetMounted(!server.IsMounted());
                break;
            case "Connect":
                if (server.IsConnected())
                    server.Disconnect();
                else
                    server.Connect();
                UpdateSelectedServerView();
                break;
            case "Open Folder":
                
                break;
            case "Open Terminal":
                break;
            case "Move Up":
                ServerInstance.SavedServers.Move(ServerInstance.SelectedServer,
                    newIndex = Math.Max(0, ServerInstance.SelectedServer - 1));
                ServerList.SelectedIndex = newIndex;
                break;
            case "Move Down":
                ServerInstance.SavedServers.Move(ServerInstance.SelectedServer,
                    newIndex = Math.Min(ServerInstance.SavedServers.Count - 1, ServerInstance.SelectedServer + 1));
                ServerList.SelectedIndex = newIndex;
                break;
            case "Remove":
                ServerInstance.SavedServers.Remove((ServerInstance)ServerList.SelectedItem);
                if (ServerInstance.SavedServers.Count == 0)
                {
                    ServerList.SelectedIndex = -1;
                    break;
                }

                ServerList.SelectedIndex +=
                    ServerInstance.SelectedServer + 1 < ServerInstance.SavedServers.Count ? 1 : -1;
                break;
        }
    }

    private void ContextMenu_OnLoaded(object sender, RoutedEventArgs e)
    {
        var server = ServerInstance.SelectedServerObject;
        if (sender is not ContextMenu cm) return;
        foreach (var cmItem in cm.Items)
        {
            if (cmItem is not MenuItem mi) continue;
            mi.IsEnabled = server != null;
            if (mi.IsEnabled)
                switch (mi.Tag)
                {
                    case "Mount":
                        if (server!.IsConnected())
                            mi.Header = "Unmount";
                        mi.Foreground = server.IsMounted() ? Brushes.Red : Brushes.Green;
                        break;
                    case "Connect":
                        if (server!.IsConnected())
                            mi.Header = "Disconnect";
                        mi.Foreground = server.IsConnected() ? Brushes.Red : Brushes.Green;
                        break;
                    case "Open Folder":
                    case "Open Terminal":
                        mi.IsEnabled = server!.IsMounted();
                        break;
                    case "Move Up":
                        mi.IsEnabled = ServerInstance.SelectedServer - 1 >= 0;
                        break;
                    case "Move Down":
                        mi.IsEnabled = ServerInstance.SelectedServer + 1 < ServerList.Items.Count;
                        break;
                }
            mi.Header ??= mi.Tag;
        }
    }

    private void ServerListItem_PreviewMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
        if (sender is not ListBoxItem draggedItem || mouseEventArgs.LeftButton != MouseButtonState.Pressed) return;
        DragDrop.DoDragDrop(draggedItem, new ObjectWrapper(draggedItem.DataContext), DragDropEffects.Move);
        draggedItem.IsSelected = true;
    }
    
    private void ServerListItem_OnDropEvent(object sender, DragEventArgs dragEventArgs)
    {
        var list = ServerInstance.SavedServers;
        if (dragEventArgs.Data.GetData(typeof(ObjectWrapper)) is not ObjectWrapper dropped ||
            ((ListBoxItem)sender).DataContext is not ServerInstance target) return;
        var oldIndex = list.IndexOf((ServerInstance)dropped.Object);
        var newIndex = list.IndexOf(target);
        list.Move(oldIndex, newIndex);
        ServerList.SelectedIndex = newIndex;
    }
    
    protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
    {
        Mouse.SetCursor(Cursors.Hand);
        e.Handled = true;
    }

    private void EditServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new AddServerWindow(true, ServerInstance.SelectedServerObject)
        {
            Owner = this,
            Title = "Edit Server"
        };
        ServerInstance.EditMode = true;
        dialog.ShowDialog();
        ServerInstance.EditMode = false;
        ServerList.Items.Refresh();
        // A workaround lol
        var old = ServerList.SelectedIndex;
        ServerList.SelectedIndex = -1;
        ServerList.SelectedIndex = old;
    }
    
    private void ExecuteButton_OnClick(object sender, RoutedEventArgs args)
    {
        var server = ServerInstance.SelectedServerObject;
        if (server == null || !server.IsExecutable() || !server.IsConnected()) return;
        // server.Execute("use products_orders_customers;select * from products", out var i, out var e);
        // while (e.MoveNext())
        // {
        //     var j = 0;
        //     while (j < i)
        //     {
        //         Console.Write(e.GetRecord()?.GetValue(j) + " | ");
        //         j++;
        //     }
        //
        //     Console.WriteLine();
        // }
    }

    private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
    {
        var server = ServerInstance.SelectedServerObject;
        if (server == null) return;
        if (server.IsConnected()) server.Disconnect();
        else server.Connect();
        UpdateSelectedServerView();
    }

    private void MountButton_OnClick(object sender, RoutedEventArgs e)
    {
        var server = ServerInstance.SelectedServerObject;
        if (server == null) return;
    }

    private void TerminalTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        var box = (TextBox)sender;
        var server = ServerInstance.SelectedServerObject;
        if (server == null) return;
        var view = (ServerInstanceViewModel)server.View!;
        var command = box.Text;
        box.Text = "";
        view.TerminalLogs.Add("> " + command);
        if (!server.Execute(command,
                out var fieldCount,
                out var affected,
                out var en,
                out var closeAction)) return;
        if (affected > 0)
            view.TerminalLogs.Add($"{affected} row(s) affected.");
        if (en == null) return;
        var reader = new FlowDocumentReader
        {
            Document = new FlowDocument(),
            HorizontalAlignment = HorizontalAlignment.Left,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            UseLayoutRounding = true,
            SnapsToDevicePixels = true,
            ViewingMode = FlowDocumentReaderViewingMode.Scroll,
            Zoom = 1
        };
        var table = new Table
        {
            CellSpacing = 0,
            RowGroups =
            {
                new TableRowGroup()
            }
        };
        for (var i = 0; i < fieldCount; i++)
            table.Columns.Add(new TableColumn());
        reader.Document.Blocks.Add(table);
        try
        {
            while (en.MoveNext())
            {
                var row = new TableRow();
                var j = 0;
                while (j < fieldCount)
                {
                    var cell = new TableCell
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(1)
                    };
                    cell.Blocks.Add(new Paragraph
                    {
                        Inlines =
                        {
                            new SelectableText
                            {
                                Text = en.GetRecord()?.GetValue(j).ToString() ?? "<null>",
                                TextWrapping = TextWrapping.NoWrap,
                                TextAlignment = TextAlignment.Center
                            }
                        }
                    });
                    j++;
                    row.Cells.Add(cell);
                }

                table.RowGroups[0].Rows.Add(row);
            }
            view.TerminalLogs.Add(new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) }
                },
                Children = { reader }
            });
            closeAction?.Invoke();
        }
        catch (Exception ex)
        {
            closeAction?.Invoke();
            ServerInstance.HandleException(ex);
        }
    }
}