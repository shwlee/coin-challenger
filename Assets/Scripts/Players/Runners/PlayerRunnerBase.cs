using Cysharp.Threading.Tasks;
using System;

public abstract class PlayerRunnerBase : IPlayer
{
    private int _port;
    private int _myNumber;
    private string _platform;
    private string _hostPath;

    protected PlayerRunnerBase(int port, string platform, string hostPath)
    {
        _port = port;
        _platform = platform;
        _hostPath = hostPath;
    }

    public UniTask CloseHost()
        => RunnerHostMediator.CloseRunner(_port, _platform);

    public UniTask<string> GetName()
        => RunnerHostMediator.GetPlayerName(_port, _platform, _myNumber);

    public UniTask Initialize(int column, int row)
        => RunnerHostMediator.InitializePlayer(_port, _platform, _myNumber, column, row);

    public UniTask<int?> MoveNext(int[] map, int currentPosition)
        => RunnerHostMediator.GetMoveNextDirection(_port, _platform, _myNumber, map, currentPosition);

    public async virtual UniTask Setup(int myNumber, string filePath)
    {
        _myNumber = myNumber;

        ThrowIfInitNotYet();

        await RunnerHostMediator.PrepareRunnerHost(_port, _platform, _hostPath);
        await RunnerHostMediator.LoadPlayerInstance(_port, _platform, myNumber, filePath);
    }

    private void ThrowIfInitNotYet()
    {
        if (_port is 0)
        {
            throw new Exception("_port not is set.");
        }

        if (string.IsNullOrWhiteSpace(_platform))
        {
            throw new Exception("_platform not is set.");
        }

        if (string.IsNullOrWhiteSpace(_hostPath))
        {
            throw new Exception("_hostPath not is set.");
        }
    }
}