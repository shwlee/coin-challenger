using Cysharp.Threading.Tasks;
using UnityEngine;

public class DummyPlayer : IPlayer
{
    private System.Random _random = new System.Random();
    private int _myNumber;

    public UniTask GameSetup(string gameId, int column, int row)
        => UniTask.CompletedTask;

    public UniTask Setup(int myPostion, string path)
    {
        _myNumber = myPostion;
        return UniTask.CompletedTask;
    }

    public UniTask<string> GetName()
        => UniTask.FromResult("CLO-Dummy0");

    public UniTask Initialize(int column, int row)
    {
        Debug.Log($"initialized myNumber:{_myNumber}, column:{column}, row:{row}");
        return UniTask.CompletedTask;
    }

    public UniTask LoadRunner()
        => UniTask.CompletedTask;

    public UniTask<int?> MoveNext(int turn, int[] map, int currentPosition)
        => UniTask.FromResult<int?>(_random.Next(0, 3));

    public UniTask CloseHost()
        => UniTask.CompletedTask;

    public UniTask CleanupHost()
        => UniTask.CompletedTask;
}
