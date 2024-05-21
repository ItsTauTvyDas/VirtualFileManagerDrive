using DokanNet;

namespace VirtualFileManagerDrive.Core.Server;

// ReSharper disable once InconsistentNaming
public class MySQLServer : ServerInstance
{
    
    
    public override bool TestConnection(out string errorReason)
    {
        throw new NotImplementedException();
    }

    public override bool IsConnected()
    {
        throw new NotImplementedException();
    }

    public override bool Connect()
    {
        throw new NotImplementedException();
    }

    public override bool Disconnect()
    {
        throw new NotImplementedException();
    }

    public override IList<FileInformation> GetFiles()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public override bool? MoveFile(string oldName, string newName)
    {
        throw new NotImplementedException();
    }
}