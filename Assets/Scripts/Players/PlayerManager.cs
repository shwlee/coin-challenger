using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] PlayerObjects;

    private Dictionary<int, PlayerContext> _players = new(4); // 최대 4명까지만 플레이한다.

    public async UniTask<IEnumerable<PlayerContext>> LoadPlayers()
    {
        switch (GameManager.Instance.Mode)
        {
            case GameMode.Contest:
                await LoadAllPlayers();
                break;
            case GameMode.Test:
                LoadTestDummy();
                break;
            case GameMode.Single1:
            case GameMode.Single2:
            case GameMode.Single3:
            case GameMode.Single4:
                await LoadSinglePlayer();
                break;
            default:
                break;
        }

        return _players.Values.ToList();
    }

    private async void LoadTestDummy()
    {
        var dummyPlayer = new DummyPlayer();
        var testPlayer = new PlayerContext
        {
            LoadSucceed = true,
            Position = 0,
            Player = dummyPlayer,
            Name = await dummyPlayer.GetName(),
        };
        _players.Add(0, testPlayer);
    }

    private async UniTask LoadAllPlayers()
    {
        try
        {
            // IPlayer 로딩.	
            // TODO : 4명 제한. 4명 초과 시 게임 중단 기능 필요.
            var files = LoadTargetFiles().OrderBy(_ => Random.Range(0, 3)).ToList(); // random 배치.        
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                await LoadPlayerContext(file, i);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex);
        }

        await SetDummy();

        // player max 방지
        if (_players.Count > Consts.MaxPlayerCount)
        {
            for (int i = _players.Count; i > Consts.MaxPlayerCount; i--)
            {
                _players.Remove(i - 1); // index == count
            }
        }
    }

    private UniTask LoadSinglePlayer()
    {
        var file = LoadTargetFiles().FirstOrDefault();
        var index = (int)GameManager.Instance.Mode;
        return LoadPlayerContext(file, index);
    }

    private string[] LoadTargetFiles()
        => Directory.EnumerateFiles(Consts.PlayerModulePath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => Consts.PlatformFiles.Contains(Path.GetExtension(file)))
            .ToArray();

    private async UniTask LoadPlayerContext(string file, int index)
    {
        var playerContext = new PlayerContext
        {
            Position = index,
            FileName = Path.GetFileName(file)
        };

        try
        {
            var fullPath = Path.GetFullPath(file);
            var player = await PlayerLoader.Load(index, fullPath);
            if (player is null)
            {
                return;
            }

            playerContext.Player = player;
            playerContext.Name = await player.GetName();
            playerContext.LoadSucceed = true;
        }
        catch (System.Exception ex)
        {
            playerContext.LoadSucceed = false;
            Debug.LogError(ex);
        }

        _players.Add(index, playerContext);
    }

    public async UniTask InitPlayer(int column, int row, Dictionary<int, Vector2> players)
    {
        foreach (var position in players)
        {
            await SetPlayer(position.Key, position.Value, column, row);
        }
    }

    public IEnumerable<PlayerContext> GetPlayerContexts()
        => _players.Values.ToList();

    public void SetScore(int playerIndex, int point)
        => _players[playerIndex].Score += point;

    public int GetScore(int playerIndex)
        => _players[playerIndex].Score;

    public IEnumerable<int> GetPlayerRanking()
        => _players.Values.OrderByDescending(playerContext => playerContext.Score).Select(playerContext => playerContext.Position);

    private async UniTask SetDummy()
    {
        // 최소 player 채움.
        if (_players.Count < Consts.MaxPlayerCount)
        {
            for (int i = _players.Count; i < Consts.MaxPlayerCount; i++)
            {
                var dummyPlayer = new DummyPlayer();
                var dummyContext = new PlayerContext
                {
                    LoadSucceed = true,
                    Position = i,
                    Player = dummyPlayer,
                    Name = $"{await dummyPlayer.GetName()}_{i}",
                };

                _players.Add(i, dummyContext);
            }
        }
    }

    private async UniTask SetPlayer(int index, Vector3 location, int column, int row)
    {
        var playerContext = _players[index];
        var prefab = PlayerObjects[index];
        var player = Instantiate(prefab); // TODO : player prefab 을 바꾸는 걸로 전환.
        DontDestroyOnLoad(player);

        player.name = await playerContext.Player.GetName();
        player.transform.position = location;

        var sprite = player.GetComponent<SpriteRenderer>();
        playerContext.DefaultSprite = sprite.sprite;
        playerContext.Color = IsDummy(playerContext.Player) ? Color.red : ColorSet.GetColorBySeq(index); // Dummy 는 red
        sprite.color = playerContext.Color;

        var controller = player.GetComponent<PlayerController>();
        controller.Position = playerContext.Position;

        playerContext.PlayerObject = player;
        playerContext.Controller = controller;
        playerContext.IntiPosition = location;
        await playerContext.Player.Initialize(column, row);

        controller.SetPlayerModule(playerContext.Player);
    }

    private bool IsDummy(IPlayer player) => player is DummyPlayer;
}
