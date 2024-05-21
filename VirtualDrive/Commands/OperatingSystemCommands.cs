using DokanNet;

namespace VirtualDrive.Commands;

public abstract class OperatingSystemCommands
{
    public string ListFilesCommand;
    public string GoToPathCommand = "cd {0}";
    public string CreateDirectoryCommand = "mkdir {0}";
    public string CreateFileCommand = "touch {0}";
    public string ReadFromFileCommand = "cat {0}";
    public string WriteToFileCommand = "echo '{0}' >> {1}";
    public string DeleteFileCommand = "rm {0}";
    public string DeleteFolderCommand = "rm -r {0}";
    public string MoveFileCommand = "mv {0} {1}";

    public abstract List<FileInformation> HandleListFilesCommand(List<string> commandResults);
}

public class UbuntuCommands : OperatingSystemCommands
{
    public UbuntuCommands() => ListFilesCommand = "stat -c '%F/%s/%n/%W/%X/%Y' -- *";
    
    public override List<FileInformation> HandleListFilesCommand(List<string> commandResults)
    {
        List<FileInformation> list = [];
        commandResults.ForEach(r =>
        {
            var split = r.Split("/");
            FileInformation fi;
            list.Add(fi = new FileInformation()
            {
                FileName = split[2],
                Length = long.Parse(split[1]),
                CreationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(split[3])).DateTime,
                LastAccessTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(split[4])).DateTime,
                LastWriteTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(split[5])).DateTime
            });
            fi.Attributes = split[0] switch
            {
                "symbolic link" or "directory" => FileAttributes.Directory,
                _ => FileAttributes.Normal
            };
        });
        return list;
    }
}