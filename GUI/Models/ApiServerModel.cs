using ReplayAPI;

namespace GUI.Models;

public class ApiServerModel
{
    public bool IsRunning { get; private set; }
    private readonly ReplayApiServer _apiServer = new ReplayApiServer();
    
    public void Run()
    {
        _apiServer.Run();
    }
}