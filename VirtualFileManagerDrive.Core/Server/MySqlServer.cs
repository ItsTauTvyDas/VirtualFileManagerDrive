using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using DokanNet;
using MySql.Data.MySqlClient;
using VirtualFileManagerDrive.Common;

namespace VirtualFileManagerDrive.Core.Server;

public class MySqlServer : ServerInstance
{
    private readonly MySqlConnection _connection = new();
    private readonly MySqlCommand _execute = new();

    public string DatabaseName = "products_orders_customers";
    public string AdditionalConnectionStringData = "";
    
    public MySqlServer()
    {
        _execute.Connection = _connection;
        Note = "If no database is entered, then all of the databases gonna be shown in volume's root.";
        Port = 3306;
        FileSystemName = "MySQL";
        DriveName = "MySQL Remote Server";
        AdditionalData.Add(new AdditionalData("MySQL", "Connect to database", DatabaseName,
            s => DatabaseName = (string)s));
        AdditionalData.Add(new AdditionalData("MySQL", "Additional connection string data",
            AdditionalConnectionStringData, s => AdditionalConnectionStringData = (string)s));
    }

    public string ConnectionString
    {
        get
        {
            var str = $"server={Address};port={Port};uid={User};pwd={Password}";
            if (!string.IsNullOrWhiteSpace(DatabaseName))
                str += $";database={DatabaseName}";
            return str;
        }
    }

    public override bool IsConnected() => _connection.State == ConnectionState.Open;

    internal override bool UnsafeConnect()
    {
        _connection.ConnectionString = ConnectionString + ";" + AdditionalConnectionStringData;
        ActionStart();
        _connection.Open();
        NewLogReceivedHandler(this, "Connection Opened");
        ActionEnd();
        return IsConnected();
    }
    
    public override void TestConnection(out string? errorReason)
    {
        Connect();
        errorReason = LastException?.Message;
        Disconnect();
    }

    public override bool Disconnect()
    {
        Stopwatch.Stop();
        Ping = 0;
        _connection.Close();
        NewLogReceivedHandler(this, "Disconnected");
        return false;
    }
    
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    [SuppressMessage("ReSharper", "GenericEnumeratorNotDisposed")]
    private bool Read(object obj, Action<object> action)
    {
        Busy = true;
        DbDataReader? reader = null;
        try
        {
            ActionStart();
            reader = obj is Task<DbDataReader> task ? task.Result : _execute.ExecuteReader();
            ActionEnd();
            action.Invoke(reader);
            return true;
        }
        catch (Exception ex)
        {
            ActionEnd();
            NewLogReceivedHandler(this, ex);
            HandleException(ex);
            return false;
        }
        finally
        {
            Busy = false;
            reader?.Close();
            reader?.Dispose();
            if (obj is Task<DbDataReader> task)
                task.Dispose();
        }
    }
    
    public override bool Execute(string command, Action<object> action, bool async = false)
    {
        _execute.CommandText = command;
        if (!async) return Read(_execute, action);
        _execute.ExecuteReaderAsync().ContinueWith(task => Read(task, action));
        return true;
    }

    public override bool IsExecutable() => true;

    public override IList<FileInformation> GetFiles(string path = "\\")
    {
        List<FileInformation> files = [];
        if (path == "\\")
        {
            if (FileCache.TryGetValue(path, out var list))
                return list;
            if (DatabaseName == "")
            {
                _execute.CommandText =
                    "SELECT " +
                    "    s.SCHEMA_NAME, " +
                    "    MAX(t.CREATE_TIME), " +
                    "    MAX(t.UPDATE_TIME)," +
                    "    COALESCE(SUM(t.DATA_LENGTH), 0)" +
                    "FROM " +
                    "    information_schema.SCHEMATA s " +
                    "LEFT JOIN " +
                    "    (SELECT TABLE_SCHEMA, CREATE_TIME, UPDATE_TIME, DATA_LENGTH FROM information_schema.TABLES) t " +
                    "ON " +
                    "    s.SCHEMA_NAME = t.TABLE_SCHEMA " +
                    "GROUP BY " +
                    "    s.SCHEMA_NAME " +
                    "ORDER BY " +
                    "    s.SCHEMA_NAME;";
                ActionStart();
                using (var reader = _execute.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var file = new FileInformation
                        {
                            Attributes = FileAttributes.Directory,
                            FileName = reader.GetString(0),
                            CreationTime = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                            LastWriteTime = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                            LastAccessTime = null,
                            Length = reader.GetInt64(3)
                        };
                        files.Add(file);
                        FileCache.AddToList(path, file);
                    }
                }
                ActionEnd();
            }
            else
            {
                
            }
        }
        Console.WriteLine(path + " - " + files.Count);
        return files;
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