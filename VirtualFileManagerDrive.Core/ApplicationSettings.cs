namespace VirtualFileManagerDrive.Core;

public static class ApplicationSettings
{
    public enum ConnectionStrength
    {
        Excellent = 10,
        VeryGood = 9,
        Good = 8,
        Fair = 7,
        Poor = 6,
        None = 5
    }

    public static readonly Dictionary<ConnectionStrength, int> ConnectionStrengthTypes = new();

    public static ConnectionStrength GetConnectionStrengthIconIndex(uint ping)
    {
        for (var i = ConnectionStrength.Poor; i <= ConnectionStrength.Excellent; i++)
        {
            if (!ConnectionStrengthTypes.TryGetValue(i, out var minPing)) continue;
            if (minPing > ping) continue;
            return i;
        }
        return ConnectionStrength.Poor;
    }
    
    public static void LoadDefault()
    {
        ConnectionStrengthTypes.Clear();
        ConnectionStrengthTypes[ConnectionStrength.Excellent] = 0;
        ConnectionStrengthTypes[ConnectionStrength.VeryGood] = 30;
        ConnectionStrengthTypes[ConnectionStrength.Good] = 60;
        ConnectionStrengthTypes[ConnectionStrength.Fair] = 100;
        ConnectionStrengthTypes[ConnectionStrength.Poor] = 200;
    }
    
    public static void Load()
    {
        LoadDefault();
    }
    
    public static void Save()
    {
        
    }
}