using System.Security.AccessControl;
using DokanNet;
using FileAccess = DokanNet.FileAccess;

namespace VirtualDrive;

public class VirtualServerDisk(ServerInstance server) : IDokanOperations
{
    public ServerInstance Server => server;

    public NtStatus CreateFile(string fileName, FileAccess access, FileShare share, FileMode mode, FileOptions options,
        FileAttributes attributes, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus HandleExitCode(int code)
    {
        return code switch
        {
            0 => NtStatus.Success,
            1 => NtStatus.Error,
            2 => NtStatus.AccessDenied,
            126 => NtStatus.AccessDenied,
            127 => NtStatus.ObjectNameNotFound,
            _ => NtStatus.Unsuccessful
        };
    }

    public void Cleanup(string fileName, IDokanFileInfo info)
    {
    }

    public void CloseFile(string fileName, IDokanFileInfo info)
    {
    }

    public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus FindFiles(string fileName, out IList<FileInformation> files, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime,
        IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
    {
        return Server.DeleteFile(info.IsDirectory, fileName) switch
        {
            true => NtStatus.Success,
            false => NtStatus.ObjectNoLongerExists,
            null => NtStatus.AccessDenied
        };
    }

    public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info) => DeleteFile(fileName, info);

    public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes,
        IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName,
        out uint maximumComponentLength, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
        IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
        IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus Mounted(string mountPoint, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus Unmounted(IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }

    public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
    {
        throw new NotImplementedException();
    }
}
