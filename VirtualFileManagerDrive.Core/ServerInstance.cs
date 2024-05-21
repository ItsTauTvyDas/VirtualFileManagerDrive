using System.Collections.ObjectModel;
using DokanNet;

namespace VirtualFileManagerDrive.Core;

public abstract class ServerInstance : ICloneable
{
    public static int SelectedServer = -1;
    public static ServerInstance? SelectedServerObject => SelectedServer == -1 ? null : SavedServers[SelectedServer];
    public static readonly ObservableCollection<ServerInstance> SavedServers = [];
    public static readonly Dictionary<string, Type?> SupportedConnectionTypes = new()
    {
        {"SSH (Secure Shell)", null},
        {"SFTP (Secure File Transfer Protocol)", null},
        {"FTP (File Transfer Protocol)", null},
        {"MySQL Database", null},
        {"Microsoft SQL Server", null}
    };

    public static IEnumerable<char> AvailableVolumeLetters => Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i)
        .Except(DriveInfo.GetDrives().Select(s => s.Name[..1][0]));
    
    // public static IEnumerable<string> AvailableVolumeLetters => Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (Char)i + ":")
    //     .Except(DriveInfo.GetDrives().Select(s => s.Name.Replace("\\", "")));

    public object? View { get; set; }
    public string ServerName = "Localhost";
    public string Address = "localhost";
    public string User = "root";
    public string Password { protected get; set; } = "";
    public uint Ping = 0;
    public string? Note { get; internal init; }
    public int Port = 20;
    public string VolumeName = "Remote Server Drive";
    public char VolumeLetter;
    public bool FileInfoCaching = true;
    public bool ReadOnly = false;
    public bool MountOnProgramLoad = false;
    public bool MountButDontAutoConnect = false;
    public ulong? AutoDisconnectAfter = 200;
    
    public VirtualServerDisk Virtual { get; private set; }
    protected ServerInstance() => Virtual = new VirtualServerDisk(this);
    
    public virtual void SetMounted(bool mounted)
    {
        
    }

    public bool IsMounted()
    {
        return false;
    }
    
    public abstract bool TestConnection(out string errorReason);
    public abstract bool IsConnected();
    public abstract bool Connect();
    public abstract bool Disconnect();

    public abstract IList<FileInformation> GetFiles();
    public abstract FileInformation GetFileInformation(string filename);
    public abstract bool? GoToPath(string path);
    public abstract bool? CreateFile(bool isDirectory, string filename);
    public abstract string ReadFile(string filename);
    public abstract bool? WriteToFile(string filename, string content);
    public abstract bool? DeleteFile(bool isDirectory, string filename);
    public abstract bool? MoveFile(string oldName, string newName);
    
    public object Clone() => MemberwiseClone();

    public ServerInstance CloneInstance()
    {
        var clone = (ServerInstance)Clone();
        clone.View = null;
        clone.Virtual = new VirtualServerDisk(clone);
        return clone;
    }
}