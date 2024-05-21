using System.Collections.Immutable;
using DokanNet;
using Renci.SshNet;
using VirtualDrive.Commands;

namespace VirtualDrive.Server;

public class SecureShellBasedServer : ServerInstance
{
    private enum OperatingSystemType
    {
        UbuntuOrDebian
    }

    public static readonly IImmutableList<string> OperatingSystemTypes =
        Enum.GetNames(typeof(OperatingSystemType)).Select(x => x.Replace("Or", "/")).ToImmutableList();

    public SecureShellBasedServer()
    {
        OperatingSystem = (int)OperatingSystemType.UbuntuOrDebian;
        Note = "SSH connection works by executing commands commands to read/write files - like ls, cat, rm and etc.";
    }
    
    private OperatingSystemType _operatingSystem;
    public int OperatingSystem
    {
        get => (int)_operatingSystem;
        set
        {
            Commands = _operatingSystem switch
            {
                OperatingSystemType.UbuntuOrDebian => new UbuntuCommands(),
                _ => null
            };
            _operatingSystem = (OperatingSystemType)value;
        }
    }

    public OperatingSystemCommands? Commands { get; private set; }
    
    public bool CommandsTested = false;
    public SshClient? Client;

    public void TestCommands()
    {

    }

    public override bool TestConnection(out string errorReason)
    {
        errorReason = "Not implemented!";
        return false;
    }

    public override void SetMounted(bool mounted)
    {
        
    }

    public override bool Connect()
    {
        if (IsConnected())
            return false;
        Client = new SshClient(Address, Port, User, Password);
        Client.Connect();
        return Client.IsConnected;
    }

    public override bool IsConnected()
    {
        return Client?.IsConnected ?? false;
    }

    public override bool Disconnect()
    {
        if (Client == null)
            return false;
        try
        {
            Client?.Disconnect();
            Client?.Dispose();
            Client = null;
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    public int? ExecuteCommand(out string? results, string? command, params object[] args)
    {
        if (command == null)
        {
            results = null;
            return null;
        }
        if (IsConnected())
        {
            var cmd = Client!.CreateCommand(string.Format(command, args));
            cmd.Execute();
            results = cmd.Result;
            return cmd.ExitStatus;
        }

        results = null;
        return null;
    }

    public override List<FileInformation> GetFiles()
    {
        return [];// ExecuteCommand(Commands.ListFilesCommand);
    }

    public override FileInformation GetFileInformation(string filename)
    {
        throw new NotImplementedException();
    }
    

    public override bool? GoToPath(string path)
    {
        throw new NotImplementedException();
    }

    public override bool? CreateFile(bool isDirectory, string filename)
    {
        throw new NotImplementedException();
    }

    public override string ReadFile(string filename)
    {
        throw new NotImplementedException();
    }

    public override bool? WriteToFile(string filename, string content)
    {
        throw new NotImplementedException();
    }

    public override bool? DeleteFile(bool isDirectory, string filename)
    {
        if (ExecuteCommand(out var results, isDirectory ? Commands?.DeleteFolderCommand : Commands?.DeleteFileCommand,
                filename) == 0)
            return true;
        return true;
    }

    public override bool? MoveFile(string oldName, string newName)
    {
        ExecuteCommand(out _, Commands?.MoveFileCommand, oldName, newName);
        return true;
    }
}