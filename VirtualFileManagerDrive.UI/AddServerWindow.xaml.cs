using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Mysqlx.Resultset;
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
    
    private void TestConnectionButton_OnClick(object sender, RoutedEventArgs e)
    {
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

        var success = instance.TestConnection(out var errorReason);
        MessageBox.Show(
            success ? "Connected successfully!" : $"Connection failed: {errorReason}",
            success ? "Success" : "Error occured",
            MessageBoxButton.OK,
            success ? MessageBoxImage.Information : MessageBoxImage.Error
        );
    }

    private DependencyObject? _oldFaultyObject;
    private void AddServerButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!DependencyExtension.CheckRequiredElements(this, ServerDataInput, out _oldFaultyObject, _oldFaultyObject,
                "Failed to add new server!")) return;
        _oldFaultyObject = null;
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