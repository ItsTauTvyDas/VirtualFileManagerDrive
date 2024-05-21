using System.Runtime.CompilerServices;
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
        instance.VolumeName = (string)_editedValues.GetValueOrDefault(nameof(VolumeName), instance.VolumeName)!;
        instance.Port = (int)_editedValues.GetValueOrDefault(nameof(Port), instance.Port)!;
        instance.User = (string)_editedValues.GetValueOrDefault(nameof(User), instance.User)!;
        instance.ReadOnly = (bool)_editedValues.GetValueOrDefault(nameof(ReadOnly), instance.ReadOnly)!;
        instance.VolumeLetter = (char)_editedValues.GetValueOrDefault(nameof(VolumeLetter), instance.VolumeLetter)!;
        instance.AutoDisconnectAfter = (ulong?)_editedValues.GetValueOrDefault(nameof(AutoDisconnectAfter), instance.AutoDisconnectAfter);
        instance.FileInfoCaching = (bool)_editedValues.GetValueOrDefault(nameof(FileInfoCaching), instance.FileInfoCaching)!;
        instance.MountOnProgramLoad = (bool)_editedValues.GetValueOrDefault(nameof(MountOnProgramLoad), instance.MountOnProgramLoad)!;

        _editedValues = null;
    }

    private void SetValue(object? value, [CallerMemberName] string member = "")
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
        OnPropertyChanged(member);
    }

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
    
    public string ServerName
    {
        get => (string)GetValue()!;
        set => SetValue(value);
    }

    public bool IsConnected => Instance.IsConnected();
    
    public bool IsMounted => Instance.IsMounted();

    public uint Ping => Instance.Ping;

    public int ConnectionIconIndex => IsConnected
        ? (int)ApplicationSettings.GetConnectionStrengthIconIndex(Ping)
        : (int)ApplicationSettings.ConnectionStrength.None;

    public string ConnectionToolTip => IsConnected ? $"Ping: {Ping}" : "Not Connected!";

    public void Update()
    {
        OnPropertyChanged(nameof(IsConnected));
        OnPropertyChanged(nameof(IsMounted));
        OnPropertyChanged(nameof(Ping));
        OnPropertyChanged(nameof(ConnectionIconIndex));
        OnPropertyChanged(nameof(ConnectionToolTip));
    }
    
    public string Address
    {
        get => (string)GetValue()!;
        set => SetValue(value);
    }

    public string AddressAndPort => Address + ":" + Port;
    
    public string User
    {
        get => (string)GetValue()!;
        set => SetValue(value);
    }
    
    public string Password
    {
        get => (string)GetValue()!;
        set => SetValue(value);
    }
    
    public int Port
    {
        get => (int)GetValue()!;
        set => SetValue(value);
    }
    
    public string VolumeName
    {
        get => (string)GetValue()!;
        set => SetValue(value);
    }
    
    public char VolumeLetter
    {
        get => (char)GetValue()!;
        set => SetValue(value);
    }
    public string? Note => Instance.Note;
    
    public bool FileInfoCaching
    {
        get => (bool)GetValue()!;
        set => SetValue(value);
    }
    
    public bool ReadOnly
    {
        get => (bool)GetValue()!;
        set => SetValue(value);
    }
    
    public bool MountOnProgramLoad
    {
        get => (bool)GetValue()!;
        set
        {
            SetValue(value);
            if (MountOnProgramLoad) return;
            SetValue(false, nameof(MountButDontAutoConnect));
            OnPropertyChanged(nameof(MountButDontAutoConnect));
        }
    }
    
    public ulong? AutoDisconnectAfter
    {
        get => (ulong?)GetValue();
        set
        {
            SetValue(value);
            OnPropertyChanged(nameof(AutoDisconnectAfterString));
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
        set => SetValue(value);
    }
}