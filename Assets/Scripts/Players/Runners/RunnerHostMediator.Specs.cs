public partial class RunnerHostMediator
{
    private const string HostRoot = "http://localhost:{0}/coinchallenger/{1}"; // {0}:port, {1}:platform-csharp, js, cpp, py
    private const string PartGame = "game";
    private const string PartPlayer = "player";

    private const string Healthy = "healthy";
    private const string Set = "set";
    private const string Shutdown = "shutdown";

    private const string LoadPlayer = "load";
    private const string InitPlayer = "init";
    private const string Movenext = "movenext";
    private const string GetName = "name";
}
