using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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
}