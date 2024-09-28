using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameInfoService
{
    #region singleton

    private static readonly Lazy<GameInfoService> _instance = new Lazy<GameInfoService>(() => new GameInfoService());
    public static GameInfoService Instance => _instance.Value;

    private GameInfoService()
    {
    }

    #endregion

    private System.Random _random = new System.Random();

    private int _column;
    private int _row;
    private int[] _mapBag;

    public void Init(int column, int row, int[] mapBag)
    {
        _column = column;
        _row = row;
        _mapBag = mapBag.ToArray();
    }

    public (int column, int row, int[] map) GetMapInfo()
        => (_column, _row, _mapBag.ToArray());

    public (int column, int row) GetMapRange()
        => (_column, _row);

    public void RemoveItem(int index)
        => RemoveItemByIndex(index);    

    private void RemoveItemByIndex(int index)
    {
        Debug.Log($"GameInfoService.RemoveItemByIndex. index:{index}");

        _mapBag[index] = 0;

        if (IsClearAllCoins())
        {
            if (GameManager.Instance.Mode is GameMode.Test) // test mode 일 때에는 자동 종료 하지 않는다.
            {
                return;
            }

            GameManager.Instance.GameStatus = GameStatus.GameSet; //  맵에 남은 코인이 없으면 게임 종료.
        }
    }

    public int GetRandomCoinIndex()
    {
        var coinIndexes = new List<int>();
        for (var i = 0; i < _mapBag.Length; i++)
        {
            if (_mapBag[i] > 0)
            {
                coinIndexes.Add(i);
            }
        }

        if (coinIndexes.Any() is false)
        {
            return -1;
        }

        var coinIndex = coinIndexes.OrderBy(x => _random.Next()).Take(1).FirstOrDefault();
        Debug.Log($"select remove coin index:{coinIndex}");

        return coinIndex;
    }

    /// <summary>
    /// 지정된 타입의 coin 을 랜덤하게 배치된 순서로 반환 받는다.
    /// </summary>
    /// <param name="coinType"></param>
    /// <returns></returns>
    public IEnumerable<int> GetRandomCoinIndexes(CoinType coinType)
    {
        var coinIndexes = new List<int>();
        var coinScore = (int)coinType;
        for (int i = 0; i < _mapBag.Length; i++)
        {
            if (_mapBag[i] != coinScore)
            {
                continue;
            }

            coinIndexes.Add(i);
        }

        return coinIndexes.OrderBy(x => _random.Next());
    }

    public bool IsClearAllCoins()
        => _mapBag.All(item => item <= 0);
}
