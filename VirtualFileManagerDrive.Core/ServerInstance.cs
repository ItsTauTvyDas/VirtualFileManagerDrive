using System.Collections.ObjectModel;
using System.Diagnostics;
using DokanNet;
using DokanNet.Logging;
using VirtualFileManagerDrive.Common;
using VirtualFileManagerDrive.Core.Server;

namespace VirtualFileManagerDrive.Core;

public abstract class ServerInstance : ICloneable
{
    private class SupportsStatusDialogAttribute : Attribute;
    
    public static int SelectedServer = -1;
    public static bool EditMode = false;
    public static ServerInstance? SelectedServerObject => SelectedServer == -1 ? null : SavedServers[SelectedServer];
    public static readonly ObservableCollection<ServerInstance> SavedServers = [];
    public static readonly Dictionary<string, Type?> SupportedConnectionTypes = new()
    {
        {"MySQL Database", typeof(MySqlServer)},
        {"SSH (Secure Shell)", null},
        {"SFTP (Secure File Transfer Protocol)", null},
        {"FTP (File Transfer Protocol)", null},
        {"Microsoft SQL Server", null}
    };
    private static readonly Dokan Dokan = new(new ConsoleLogger("[Dokan]: "));

    public static EventHandler<UnhandledExceptionEventArgs> ExceptionHandler = delegate {};

    public void HandleException(Exception ex)
    {
        CloseStatusDialogHandler(null, EventArgs.Empty);
        ExceptionHandler(null, new UnhandledExceptionEventArgs(ex, false));
    }

    public static IEnumerable<char> AvailableDriveLetters => Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i)
        .Except(DriveInfo.GetDrives().Select(s => s.Name[..1][0]));
    
    // public static IEnumerable<string> AvailableDriveLetters => Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (Char)i + ":")
    //     .Except(DriveInfo.GetDrives().Select(s => s.Name.Replace("\\", "")));

    internal Exception? LastException;
    
    public object? View { get; set; }
    public string ServerName = "Localhost";
    public string Address = "localhost";
    public string User = "root";
    public string Password { protected get; set; } = "";
    public string? Note { get; internal init; }
    public int Port = 0;
    public string DriveName = "Remote Server Drive";
    public char DriveLetter;
    public string FileSystemName;
    public bool FileInfoCaching = true;
    public bool ReadOnly = false;
    public bool MountOnProgramLoad = false;
    public bool MountButDontAutoConnect = false;
    public ulong? AutoDisconnectAfter = 200;
    public WindowsApi.ShellIcon Icon = WindowsApi.ShellIcon.NetworkDrive;
    public Dictionary<string, IList<FileInformation>> FileCache { get; internal set; } = [];
    
    private long _ping;
    public long Ping
    {
        get => _ping;
        protected set
        {
            _ping = value;
            NewPingHandler(this, value);
        }
    }

    public bool Busy { get; internal set; }

    public readonly ObservableCollection<Exception> Logs = [];
    public readonly ObservableCollection<object> TerminalLogs = [];
    public readonly List<AdditionalData> AdditionalData = [];

    // public IEnumerable<string> AdditionalDataHeaders => AdditionalData.GroupBy(x => x.Header).Where(x => x.Count() == 1)
    //     .Select(x => (string)x.Key.Header);
    
    public VirtualServerDisk Virtual { get; private set; }
    private Task? _driveTask;
    private DokanInstance? _driveInstance;
    private readonly CancellationTokenSource _driveTaskCancellation = new();
    private bool _showDialogForNextStatus;
    
    protected readonly Stopwatch Stopwatch = new();
    
    public EventHandler<uint> DriveUnmountHandler = delegate {};
    public EventHandler DriveMountHandler = delegate {};
    public static EventHandler<string> ShowStatusDialogHandler = delegate {};
    public static EventHandler CloseStatusDialogHandler = delegate {};
    public EventHandler<long> NewPingHandler = delegate {};
    public EventHandler<object> NewLogReceivedHandler = delegate {};

    protected ServerInstance()
    {
        FileSystemName = "Remote Server (?)";
        Virtual = new VirtualServerDisk(this);
        ShowStatusDialogHandler += (_, _) => _showDialogForNextStatus = false;
    }

    public void ShowDialogForNextStatus() => _showDialogForNextStatus = true;

    private void ShowStatusDialog(bool show, string? status = null)
    {
        if (show)
        {
            if (_showDialogForNextStatus)
                ShowStatusDialogHandler(this, status!);
            return;
        } 
        CloseStatusDialogHandler(this, EventArgs.Empty);
    }
    
    [SupportsStatusDialog]
    public void Mount()
    {
        ShowStatusDialog(true, "Mounting...");
        _driveTask = Task.Run(() =>
        {
            try
            {
                var dokanBuilder = new DokanInstanceBuilder(Dokan)
                    .ConfigureOptions(options =>
                    {
                        //options.Options = DokanOptions.StderrOutput;
                        options.MountPoint = $"{DriveLetter}:\\";
                    });
                _driveInstance = dokanBuilder.Build(Virtual);
                ShowStatusDialog(false);
                DriveMountHandler(_driveInstance, EventArgs.Empty);
                var result = _driveInstance.WaitForFileSystemClosed(uint.MaxValue);
                DriveUnmountHandler(_driveInstance, result);
                Console.WriteLine("Drive Unmounted");
            }
            catch (Exception ex)
            {
                ShowStatusDialog(false);
                HandleException(ex);
                _driveTaskCancellation.Cancel();
            }
        }, _driveTaskCancellation.Token);
    }

    [SupportsStatusDialog]
    public bool Unmount()
    {
        ShowStatusDialog(true, "Unmounting...");
        var result = Dokan.Unmount(DriveLetter);
        ShowStatusDialog(false);
        return result;
    }

    [SupportsStatusDialog]
    public void SetMounted(bool mounted)
    {
        if (mounted) Mount(); else Unmount();
    }

    public bool IsMounted()
    {
        return _driveInstance != null && _driveInstance.IsFileSystemRunning();
    }

    public void TryToCancelExecutingTask() => Busy = false;

    [SupportsStatusDialog]
    public bool Connect()
    {
        try
        {
            ShowStatusDialogHandler(this, "Connecting...");
            var success = UnsafeConnect();
            CloseStatusDialogHandler(this, EventArgs.Empty);
            return success;
        }
        catch (Exception ex)
        {
            CloseStatusDialogHandler(this, EventArgs.Empty);
            HandleException(LastException = ex);
            return false;
        }
    }
    
    internal void ActionStart() => Stopwatch.Restart();

    internal void ActionEnd()
    {
        if (!Stopwatch.IsRunning) return;
        Ping = Stopwatch.ElapsedMilliseconds;
        Stopwatch.Stop();
    }
    
    public virtual bool IsExecutable() => false;
    
    public abstract bool Execute(string command, Action<object> action, bool async = false);
    public abstract void TestConnection(out string? errorReason);
    public abstract bool IsConnected();
    internal abstract bool UnsafeConnect();
    public abstract bool Disconnect();
    
    public abstract IList<FileInformation> GetFiles(string path = "\\");
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