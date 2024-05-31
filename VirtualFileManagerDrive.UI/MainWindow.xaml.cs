using System.Collections.Specialized;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using UI.Controls;
using UI.Extensions;
using VirtualFileManagerDrive.Common;
using VirtualFileManagerDrive.Core;

namespace UI;

public partial class MainWindow
{
    private CustomMessageBox? _messageBox;
    
    public MainWindow()
    {
        InitializeComponent();
        ServerInstance.ShowStatusDialogHandler += (_, str) =>
            Dispatcher.Invoke(() => 
                (_messageBox = new CustomMessageBox
                {
                    Owner = this,
                    Text = str
                }).Show());
        ServerInstance.CloseStatusDialogHandler += (_, _) => Dispatcher.Invoke(() => _messageBox?.Close());
        ServerInstance.SavedServers.CollectionChanged += (_, args) =>
        {
            UnselectSelectedServer();
            ServerList.Items.Refresh();
            if (args.Action != NotifyCollectionChangedAction.Add) return;
            var server = (ServerInstance)args.NewItems![0]!;
            server.NewPingHandler += (_, _) => Dispatcher.Invoke(() => server.GetView().NotifyPing());
            server.DriveMountHandler += (_, _) => Dispatcher.Invoke(() =>  server.UpdateView());
            server.NewLogReceivedHandler += (_, log) => Dispatcher.Invoke(() =>
            {
                if (log is Exception ex)
                    server.Logs.Add(ex);
                else
                    server.Logs.Add(new Exception(log.ToString()));
            });
        };
        ServerList.ItemsSource = ServerInstance.SavedServers;
    }

    private void UnselectSelectedServer()
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
        ServerInstance.SelectedServerObject!.UpdateView();
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
            case MessageBoxResult.None:
            case MessageBoxResult.OK:
            case MessageBoxResult.Cancel:
            default:
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
                MountButton_OnClick();
                break;
            case "Connect":
                ConnectButton_OnClick();
                break;
            case "Open Folder":
                Process.Start(new ProcessStartInfo($"{server.DriveLetter}:\\")
                {
                    UseShellExecute = true
                });
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

    private void EditServerButton_OnClick(object? sender = null, RoutedEventArgs? e = null)
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
        // A workaround lol, refreshes all the variables from server view
        var old = ServerList.SelectedIndex;
        ServerList.SelectedIndex = -1;
        ServerList.SelectedIndex = old;
    }
    
    private void ExecuteButton_OnClick(object sender, RoutedEventArgs args)
    {
        var server = ServerInstance.SelectedServerObject;
        if (server == null || !server.IsExecutable() || !server.IsConnected()) return;
    }

    private void ConnectButton_OnClick(object? sender = null, RoutedEventArgs? e = null)
    {
        var server = ServerInstance.SelectedServerObject;
        if (server == null) return;
        if (server.IsConnected())
        {
            SystemSounds.Hand.Play();
            server.Disconnect();
        }
        else
        {
            server.ShowDialogForNextStatus();
            server.Connect();
        }
        UpdateSelectedServerView();
    }

    private void MountButton_OnClick(object? sender = null, RoutedEventArgs? e = null)
    {
        var server = ServerInstance.SelectedServerObject;
        if (server == null) return;
        server.ShowDialogForNextStatus();
        server.SetMounted(true);
    }

    private void CreateSqlTableColumn(DbDataReader reader, int i, int j, Grid grid, GroupBox groupBox,
        TextBlock rowCountTextBlock, bool isData = true)
    {
        var columnSchema = isData ? null : reader.GetColumnSchema()[j];
        var value = isData ? reader.GetValue(j).ToString() : columnSchema!.ColumnName;
        var type = isData ? reader.GetFieldType(j).Name : columnSchema!.DataType?.Name ?? "<unknown>";
        object ns = "<Not Set>";
        var tooltip = isData
            ? $"Length: {value?.Length ?? 0}\n" +
              $"Is Null: {value == null}\n" +
              $"Type: {type}\n" +
              $"Row: {i + 1}, Column: {j + 1}"
            : $"Name: {value}\n" +
              $"Type: {type}\n" +
              $"Size: {columnSchema!.ColumnSize ?? ns}\n" +
              $"Table Name: {columnSchema.BaseTableName ?? ns}\n" +
              $"Is Identity: {columnSchema.IsIdentity ?? ns}\n" +
              $"Is Key: {columnSchema.IsKey ?? ns}\n" +
              $"Is Auto-Increment: {columnSchema.IsAutoIncrement ?? ns}\n" +
              $"Is Aliased: {columnSchema.IsAliased ?? ns}\n" +
              $"Is Expression: {columnSchema.IsExpression ?? ns}\n" +
              $"Is Hidden: {columnSchema.IsHidden ?? ns}\n" +
              $"Is Unique: {columnSchema.IsUnique ?? ns}\n" +
              $"Is Read-Only: {columnSchema.IsReadOnly ?? ns}";
        Dispatcher.Invoke(() =>
        {
            if (i == 0)
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            if (j == 0)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var cell = new SelectableText
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = groupBox.BorderBrush,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(2),
                Text = value ?? "<null>",
                FontWeight = isData ? FontWeights.Normal : FontWeights.Bold,
                ToolTip = tooltip,
                TextWrapping = TextWrapping.NoWrap
            };
            cell.SetValue(Grid.RowProperty, isData ? i + 1 : i);
            cell.SetValue(Grid.ColumnProperty, j);
            grid.Children.Add(cell);
            if (j == 0)
                rowCountTextBlock.Text = $"Showing {i + 1} row(s).";
        });
    }

    private void TerminalTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        var box = (TextBox)sender;
        var server = ServerInstance.SelectedServerObject;
        if (server == null) return;
        if (server.Busy && box.Text != "stop") return;
        if (box.Text == "stop")
        {
            server.TryToCancelExecutingTask();
            box.Text = "";
            return;
        }
        var view = server.GetView();
        var command = box.Text;
        box.Text = "";
        view.TerminalLogs.Add("> " + command);
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            }
        };
        var groupBox = new GroupBox
        {
            Header = "Table",
            Content = grid,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        var rowCountTextBlock = new TextBlock();
        server.Execute(command, response =>
        {
            switch (response)
            {
                case string str:
                    Dispatcher.Invoke(() => view.TerminalLogs.Add(str));
                    break;
                case DbDataReader reader:
                {
                    if (reader.RecordsAffected != -1)
                        Dispatcher.Invoke(() => view.TerminalLogs.Add($"{reader.RecordsAffected} row(s) affected."));
                    if (reader.IsClosed) return;
                    Dispatcher.Invoke(() =>
                    {
                        view.TerminalLogs.Add(groupBox);
                        view.TerminalLogs.Add(rowCountTextBlock);
                    });
                    try
                    {
                        for (var i = 0; reader.Read(); i++)
                        {
                            if (!server.Busy) break;
                            if (i > 200)
                            {
                                Dispatcher.Invoke(() => view.TerminalLogs.Add("Table exceeded 200 rows!"));
                                break;
                            }

                            for (var j = 0; j < reader.FieldCount; j++)
                            {
                                if (!server.Busy) break;
                                if (i == 0)
                                    CreateSqlTableColumn(reader, i, j, grid, groupBox, rowCountTextBlock, false);
                                CreateSqlTableColumn(reader, i, j, grid, groupBox, rowCountTextBlock);
                            }
                            if (i % 1 == 0)
                                Thread.Sleep(5);
                        }
                        if (!server.Busy)
                            Dispatcher.Invoke(() => view.TerminalLogs.Add("The process has been interrupted, not continuing anymore..."));
                    }
                    catch (Exception ex)
                    {
                        server.HandleException(ex);
                    }
                    break;
                }
            }
        }, true);
    }
}