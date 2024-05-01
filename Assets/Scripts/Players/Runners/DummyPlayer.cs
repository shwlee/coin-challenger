using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class DummyPlayer : IPlayer
{
    private System.Random _random = new System.Random();
    private int _myNumber;

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
        => throw new NotImplementedException();

    public UniTask<int?> MoveNext(int[] map, int currentPosition)
        => UniTask.FromResult<int?>(_random.Next(0, 3));

    public UniTask CloseHost()
        => UniTask.CompletedTask;
}
