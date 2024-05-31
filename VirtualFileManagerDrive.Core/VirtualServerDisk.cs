using System.Security.AccessControl;
using DokanNet;
using FileAccess = DokanNet.FileAccess;

namespace VirtualFileManagerDrive.Core;

public class VirtualServerDisk(ServerInstance server) : IDokanOperations
{
    public ServerInstance Server => server;

    public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options,
        FileAttributes attributes, IDokanFileInfo info)
    {
        return NtStatus.Success;
    }

    public void Cleanup(string fileName, IDokanFileInfo info) { }

    public void CloseFile(string fileName, IDokanFileInfo info) { }

    public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
    {
        bytesRead = 0;
        return NtStatus.Success;
    }

    public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
    {
        bytesWritten = 0;
        return NtStatus.Error;
    }

    public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
    {
        return NtStatus.Error;
    }

    public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
    {
        fileInfo = new FileInformation { FileName = fileName };
        if (fileName == "\\")
        {
            fileInfo.Attributes = FileAttributes.Directory;
            fileInfo.LastAccessTime = DateTime.Now;
            fileInfo.LastWriteTime = null;
            fileInfo.CreationTime = null;
            return DokanResult.Success;
        }
        return NtStatus.Success;
    }

    public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
    {
        files = server.GetFiles(fileName);
        return NtStatus.Success;
    }

    public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
    {
        files = [];
        return NtStatus.NotImplemented;
    }

    public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
    {
        return NtStatus.NotImplemented;
    }

    public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime,
        IDokanFileInfo info)
    {
        return NtStatus.Error;
    }

    public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
    {
        return Server.DeleteFile(info.IsDirectory, fileName) switch
        {
            true => NtStatus.Success,
            false => NtStatus.ObjectNameCollision,
            null => NtStatus.AccessDenied
        };
    }

    public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info) => DeleteFile(fileName, info);

    public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
    {
        return NtStatus.NotImplemented;
    }

    public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
    {
        return NtStatus.Error;
    }

    public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
    {
        return NtStatus.Error;
    }

    public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
    {
        return NtStatus.Success;
    }

    public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
    {
        return NtStatus.Success;;
    }

    public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
        IDokanFileInfo info)
    {
        freeBytesAvailable = 0;
        totalNumberOfBytes = 0;
        totalNumberOfFreeBytes = 0;
        return NtStatus.Success;
    }

    public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName,
        out uint maximumComponentLength, IDokanFileInfo info)
    {
        volumeLabel = server.DriveName;
        fileSystemName = server.FileSystemName;
        maximumComponentLength = 256;
        features = FileSystemFeatures.UnicodeOnDisk;
        return NtStatus.Success;
    }

    public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity? security, AccessControlSections sections,
        IDokanFileInfo info)
    {
        security = null;
        return NtStatus.Error;
    }

    public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
        IDokanFileInfo info)
    {
        return NtStatus.Error;
    }

    public NtStatus Mounted(string mountPoint, IDokanFileInfo info)
    {
        server.DriveMountHandler(server, EventArgs.Empty);
        return NtStatus.Success;
    }

    public NtStatus Unmounted(IDokanFileInfo info)
    {
        return NtStatus.Success;
    }

    public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
    {
        streams = [];
        return NtStatus.NotImplemented;
    }
}
