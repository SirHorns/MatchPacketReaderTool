namespace SpectatorAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var api = new APIServer();
        api.Run("F:\\ReplayArchive\\LRF\\Replays2\\found\\Map1\\00e196923dfca246d44cbf98716ac4a3.lrf");
    }
}