using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using VirtualFileManagerDrive.Common;
using VirtualFileManagerDrive.Core;

namespace UI.ViewModels;

public class ServerInstanceViewModel(ServerInstance instance) : ViewModelBase
{
    public ServerInstance Instance => instance;
    private Dictionary<string, object?>? _editedValues;

    public string ConnectionType =>
        ServerInstance.SupportedConnectionTypes.First(x => x.Value == instance.GetType()).Key;

    public void SetEditMode()
    {
        _editedValues = new Dictionary<string, object?>();
    }

    public void CancelEditMode()
    {
        _editedValues = null;
    }

    public void ApplyEdits()
    {
        if (_editedValues == null)
            return;
        //instance.Password = (string)_editedValues.GetValueOrDefault(nameof(Password), instance.Password)!;
        instance.MountButDontAutoConnect = (bool)_editedValues.GetValueOrDefault(nameof(MountButDontAutoConnect), instance.MountButDontAutoConnect)!;
        instance.Address = (string)_editedValues.GetValueOrDefault(nameof(Address), instance.Address)!;
        instance.ServerName = (string)_editedValues.GetValueOrDefault(nameof(ServerName), instance.ServerName)!;
        instance.DriveName = (string)_editedValues.GetValueOrDefault(nameof(DriveName), instance.DriveName)!;
        instance.Port = (int)_editedValues.GetValueOrDefault(nameof(Port), instance.Port)!;
        instance.User = (string)_editedValues.GetValueOrDefault(nameof(User), instance.User)!;
        instance.ReadOnly = (bool)_editedValues.GetValueOrDefault(nameof(ReadOnly), instance.ReadOnly)!;
        instance.DriveLetter = (char)_editedValues.GetValueOrDefault(nameof(DriveLetter), instance.DriveLetter)!;
        instance.AutoDisconnectAfter = (ulong?)_editedValues.GetValueOrDefault(nameof(AutoDisconnectAfter), instance.AutoDisconnectAfter);
        instance.FileInfoCaching = (bool)_editedValues.GetValueOrDefault(nameof(FileInfoCaching), instance.FileInfoCaching)!;
        instance.MountOnProgramLoad = (bool)_editedValues.GetValueOrDefault(nameof(MountOnProgramLoad), instance.MountOnProgramLoad)!;
        
        _editedValues = null;
    }

    private void SetValue(object? value, string member)
    {
        if (_editedValues != null)
        {
            _editedValues[member] = value;
        }
        else
        {
            var field = Instance.GetType().GetField(member);
            if (field == null)
                Instance.GetType().GetProperty(member)?.SetValue(Instance, value);
            field?.SetValue(Instance, value);
        }
        OnPropertyAutoChanged(member);
    }

    private void SetAutoValue(object? value, [CallerMemberName] string member = "") => SetValue(value, member);

    private object? GetValue([CallerMemberName] string member = "")
    {
        if (_editedValues != null)
        {
            if (_editedValues.TryGetValue(member, out var value))
                return value;
        }
        var field = Instance.GetType().GetField(member);
        return field == null ? Instance.GetType().GetProperty(member)?.GetValue(Instance) : field.GetValue(Instance);
    }

    public void NotifyPing()
    {
        OnPropertyChanged(nameof(Ping));
    }
    
    public void Update()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsMounted));
        OnPropertyChanged(nameof(CanExecute));
        OnPropertyChanged(nameof(Ping));
        OnPropertyChanged(nameof(ConnectionIconIndex));
        OnPropertyChanged(nameof(ConnectionToolTip));
        OnPropertyChanged(nameof(ConnectionStatus));
    }

    private void VerifyNotEmptyAndSetValue(string value, string name, [CallerMemberName] string member = "")
    {
        if (value == "" || string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{name} cannot be empty!");
        SetValue(value, member);
    }

    public ObservableCollection<Exception> Logs => Instance.Logs;
    public ObservableCollection<object> TerminalLogs => Instance.TerminalLogs;
    public List<AdditionalData> AdditionalData => Instance.AdditionalData;

    public string ServerName
    {
        get => (string)GetValue()!;
        set => VerifyNotEmptyAndSetValue(value, "Server Name");
    }

    public string ConnectionStatus => IsConnected ? "Connected" : "Disconnected";
    public Brush ConnectionStatusColor => IsConnected ? Brushes.ForestGreen : Brushes.IndianRed;

    public bool IsExecutable => Instance.IsExecutable();

    public bool CanExecute => Instance.IsExecutable() && Instance.IsConnected();
    
    public bool IsConnected => Instance.IsConnected();
    
    public bool IsMounted => Instance.IsMounted();

    public WindowsApi.ShellIcon Icon => Instance.Icon;

    public long Ping => Instance.Ping;

    public int ConnectionIconIndex => IsConnected
        ? (int)ApplicationSettings.GetConnectionStrengthIconIndex(Ping)
        : (int)ApplicationSettings.ConnectionStrength.None;

    public string ConnectionToolTip => IsConnected ? $"Ping: {Ping}" : "Not Connected!";
    
    public string Address
    {
        get => (string)GetValue()!;
        set => VerifyNotEmptyAndSetValue(value, "Server Address");
    }

    public string AddressAndPort => Address + ":" + Port;
    
    public string User
    {
        get => (string)GetValue()!;
        set => VerifyNotEmptyAndSetValue(value, "Username");
    }
    
    public int Port
    {
        get => (int)GetValue()!;
        set => SetAutoValue(value);
    }
    
    public string DriveName
    {
        get => (string)GetValue()!;
        set => VerifyNotEmptyAndSetValue(value, "Volume Name");
    }
    
    public char DriveLetter
    {
        get => (char)GetValue()!;
        set => SetAutoValue(value);
    }
    public string? Note => Instance.Note;
    
    public bool FileInfoCaching
    {
        get => (bool)GetValue()!;
        set => SetAutoValue(value);
    }
    
    public bool ReadOnly
    {
        get => (bool)GetValue()!;
        set => SetAutoValue(value);
    }
    
    public bool MountOnProgramLoad
    {
        get => (bool)GetValue()!;
        set
        {
            SetAutoValue(value);
            if (MountOnProgramLoad) return;
            SetAutoValue(false, nameof(MountButDontAutoConnect));
            OnPropertyAutoChanged(nameof(MountButDontAutoConnect));
        }
    }
    
    public ulong? AutoDisconnectAfter
    {
        get => (ulong?)GetValue();
        set
        {
            SetAutoValue(value);
            OnPropertyAutoChanged(nameof(AutoDisconnectAfterString));
        }
    }

    public string AutoDisconnectAfterString => AutoDisconnectAfter switch
    {
        null => "Never",
        0 => "Instant",
        _ => $"{AutoDisconnectAfter} sec."
    };
    
    public bool MountButDontAutoConnect
    {
        get => (bool)GetValue()!;
        set => SetAutoValue(value);
    }
}