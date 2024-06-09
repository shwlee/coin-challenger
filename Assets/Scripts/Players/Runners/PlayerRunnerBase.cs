using Cysharp.Threading.Tasks;
using System;

public abstract class PlayerRunnerBase<T> : IPlayer
{
    public static int Port { get; private set; }
    public static string Platform { get; private set; }
    public static string HostPath { get; private set; }

    private int _myNumber;

    protected PlayerRunnerBase(int port, string platform, string hostPath)
    {
        Port = port;
        Platform = platform;
        HostPath = hostPath;
    }

    public static UniTask CloseHost()
        => RunnerHostMediator.CloseRunner(Port, Platform);

    public static UniTask CleanupHost()
        => RunnerHostMediator.CleanupPlayerHost(Port, Platform);

    public UniTask<string> GetName()
        => RunnerHostMediator.GetPlayerName(Port, Platform, _myNumber);

    public UniTask Initialize(int column, int row)
        => RunnerHostMediator.InitializePlayer(Port, Platform, _myNumber, column, row);

    public UniTask<int?> MoveNext(int[] map, int currentPosition)
        => RunnerHostMediator.GetMoveNextDirection(Port, Platform, _myNumber, map, currentPosition);

    public async virtual UniTask Setup(int myNumber, string filePath)
    {
        _myNumber = myNumber;

        ThrowIfInitNotYet();

        await RunnerHostMediator.PrepareRunnerHost(Port, Platform, HostPath);
        await RunnerHostMediator.LoadPlayerInstance(Port, Platform, myNumber, filePath);
    }

    private void ThrowIfInitNotYet()
    {
        if (Port is 0)
        {
            throw new Exception("_port not is set.");
        }

        if (string.IsNullOrWhiteSpace(Platform))
        {
            throw new Exception("_platform not is set.");
        }

        if (string.IsNullOrWhiteSpace(Platform))
        {
            throw new Exception("_hostPath not is set.");
        }
    }
}