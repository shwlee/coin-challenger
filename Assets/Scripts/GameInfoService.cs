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

    public int GetRandomBlockIndex()
    {
        var blockIndexes = new List<int>();
        for (var i = 0; i < _mapBag.Length; i++)
        {
            if (_mapBag[i] is -1)
            {
                blockIndexes.Add(i);
            }
        }

        if (blockIndexes.Count is 0)
        {
            return -1;
        }

        var blockIndex = blockIndexes.OrderBy(x => _random.Next()).Take(1).FirstOrDefault();
        Debug.Log($"select block index to remove. block:{blockIndex}");

        return blockIndex;
    }

    public int[] GetRandomEmptyTiles(int count, IEnumerable<int> playerPositions)
    {
        var emptyTiles = new List<int>();
        for (var i = 0; i < _mapBag.Length; i++)
        {
            if (_mapBag[i] is 0)
            {
                emptyTiles.Add(i);
            }
        }

        var removeIndexes = new HashSet<int>();
        foreach (var playerPosition in playerPositions)
        {
            removeIndexes.Add(playerPosition); // 현재 player 위치부터 추가.
            FullIndexesWithNearDirections(playerPosition, removeIndexes);
        }

        foreach (var notAllow in removeIndexes)
        {
            emptyTiles.Remove(notAllow);
        }

        var randomTiles = emptyTiles.OrderBy(x => _random.Next()).Take(4);

        return randomTiles.ToArray();
    }

    /// <summary>
    /// 지정된 index 로부터 인접한 상하좌우 index 를 구한다. 기본 2칸 범위를 구한다.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="totalBag"></param>
    /// <returns></returns>
    private void FullIndexesWithNearDirections(int index, HashSet<int> totalBag, int distance = 2)
    {
        var totalLength = _column * _row;

        var currentColumn = index / _column;
        var columnMin = currentColumn * _column;
        var columnMax = currentColumn * _column + _column;

        // 상하좌우 빈 공간 인덱스를 구한다.
        // 상
        SetUpIndexesForEmpty(index, totalBag, distance);

        // 하 
        SetDownIndexesForEmpty(index, totalBag, totalLength, distance);

        // 좌
        SetLeftIndexesForEmpty(index, totalBag, columnMin, distance);

        // 우
        SetRightIndexesForEmpty(index, totalBag, columnMax, distance);
    }

    private void SetUpIndexesForEmpty(int index, HashSet<int> totalBag, int distance = 1)
    {
        for (int i = 0; i < distance; i++)
        {
            var up = index - _column * (i + 1);
            if (up < 0)
            {
                continue;
            }

            if (_mapBag[up] is 0)
            {
                totalBag.Add(up);
            }
        }
    }

    private void SetDownIndexesForEmpty(int index, HashSet<int> totalBag, int totalLength, int distance = 1)
    {
        for (int i = 0; i < distance; i++)
        {
            var down = index + _column * (i + 1);
            if (down >= totalLength)
            {
                continue;
            }

            if (_mapBag[down] is 0)
            {
                totalBag.Add(down);
            }
        }
    }

    private void SetLeftIndexesForEmpty(int index, HashSet<int> totalBag, int columnMin, int distance = 1)
    {
        for (int i = 0; i < distance; i++)
        {
            var left = index - (i + 1);
            if (left < columnMin)
            {
                continue;
            }

            if (_mapBag[left] is 0)
            {
                totalBag.Add(left);
            }
        }
    }

    private void SetRightIndexesForEmpty(int index, HashSet<int> totalBag, int columnMax, int distance = 1)
    {
        for (int i = 0; i < distance; i++)
        {
            var right = index + (i + 1);
            if (right > columnMax)
            {
                continue;
            }

            if (_mapBag[right] is 0)
            {
                totalBag.Add(right);
            }
        }
    }

    public void AddBlackMatter(int location)
    {
        _mapBag[location] = 500;
    }

    public bool IsClearAllCoins()
        => _mapBag.All(item => item <= 0);
}
