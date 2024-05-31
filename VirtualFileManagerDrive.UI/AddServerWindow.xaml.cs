using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UI.Extensions;
using UI.ViewModels;
using VirtualFileManagerDrive.Core;

namespace UI;

public partial class AddServerWindow
{
    public ServerInstance? Instance => ((ServerInstanceViewModel)GetValue(InstanceViewProperty))?.Instance;
    public ServerInstanceViewModel InstanceView => (ServerInstanceViewModel)GetValue(InstanceViewProperty);
    public static readonly DependencyProperty InstanceViewProperty =
        DependencyProperty.Register(nameof(InstanceView), typeof(ServerInstanceViewModel), 
            typeof(AddServerWindow)
        );
    public bool IsEditMode { get; }

    public AddServerWindow(bool editMode = false, ServerInstance? instance = null)
    {
        IsEditMode = editMode;
        InitializeComponent();
        SelectServerType.ItemsSource = ServerInstance.SupportedConnectionTypes.Keys;
        switch (editMode)
        {
            case true when instance == null:
                throw new ArgumentNullException(nameof(instance), "Edit mode is enabled, but passed instance was null.");
            case true:
                var oldView = (ServerInstanceViewModel)instance.View!;
                var newView = new ServerInstanceViewModel(instance);
                newView.SetEditMode();
                SetValue(InstanceViewProperty, newView);
                SelectServerType.SelectedItem = oldView.ConnectionType;
                SelectServerType.IsReadOnly = true;
                SelectServerType.IsEnabled = false;
                return;
        }
        SelectServerType.SelectedIndex = 0;
    }
    
    private void SelectServerType_OnSelected(object sender, RoutedEventArgs e)
    {
        if (IsEditMode) return;
        if (!ServerInstance.SupportedConnectionTypes.TryGetValue((SelectServerType.SelectedItem as string)!, out var type)) return;
        if (type == null)
        {
            SelectServerType.SelectedIndex = 0;
            MessageBox.Show(
                "This connection type is not implemented yet!",
                "Error occured",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
            return;
        }
        var instance = (ServerInstance?)Activator.CreateInstance(type);
        if (instance == null) return;
        SetValue(InstanceViewProperty, new ServerInstanceViewModel(instance));
        Console.WriteLine("New instance");
        InstanceView.OnPropertyChanged("AdditionalData");
    }

    private bool IsValid(DependencyObject parent)
    {
        foreach (var obj in parent.GetAllChildren())
        {
            if (!Validation.GetHasError(obj)) continue;
            var error = Validation.GetErrors(obj)[0];
            MessageBox.Show(this, error.ErrorContent.ToString(), "Validation Error!", MessageBoxButton.OK,
                MessageBoxImage.Error);
            ((IInputElement)obj).Focus();
            return false;
        }
        return true;
    }
    
    private void TestConnectionButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsValid(ServerDataInput))
            return;
        var instance = Instance;
        if (instance == null)
        {
            MessageBox.Show(
                "No instance was found, please (re)select server type and input the information again",
                "Error occured",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
            return;
        }
        
        instance.TestConnection(out var errorReason);
        if (errorReason == null)
            MessageBox.Show(
                "Connected successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
    }
    
    private void AddServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!IsValid(ServerDataInput))
            return;
        if (IsEditMode)
        {
            InstanceView.ApplyEdits();
            ServerInstance.EditMode = false;
            if (Instance != null) Instance.View = InstanceView;
            Close();
            return;
        }
        if (Instance == null) return;
        Instance.View = InstanceView;
        InstanceView.Logs.CollectionChanged += (_, _) => Dispatcher.Invoke(() => InstanceView.OnPropertyChanged(nameof(InstanceView.Logs)));
        InstanceView.TerminalLogs.CollectionChanged +=
            (_, _) => InstanceView.OnPropertyChanged(nameof(InstanceView.TerminalLogs));
        Instance.DriveMountHandler += (_, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                InstanceView.Update();
                InstanceView.Logs.Add(new Exception("Drive mounted"));
            });
        };
        Instance.DriveUnmountHandler += (_, result) =>
        {
            Dispatcher.Invoke(() => {
                InstanceView.Update();
                Instance.Logs.Add(new Exception($"Drive unmounted ({result})"));
            });
        };
        ServerInstance.SavedServers.Add(Instance);
        ServerInstance.SelectedServer = ServerInstance.SavedServers.Count - 1;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e) => Close();

    private void PasswordInput_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (Instance != null && sender is PasswordBox pb)
            Instance.Password = pb.Password;
    }

    private void Window_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Keyboard.ClearFocus();
    }

    private void AddServerWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        InstanceView.CancelEditMode();
    }

    private void ServerNameInput_OnLoaded(object sender, RoutedEventArgs e)
    {
        var box = (TextBox)sender;
        box.Focus();
        box.SelectionStart = 0;
        box.SelectionLength = box.Text.Length;
    }
}