using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const string ConfigPath = "config";
    private const string ConfigFile = "gameSettings.json";

    public static GameManager Instance { get; private set; }

    public float runningTimeBeforeHurryUp = 120f; // 기본 120초(2분)이 게임시간. 2분이 경과되면 hurryUp mode.
    public float hurryUpBlinkingDuration = 0.5f;
    public Color hurryUpBackground = Color.red;

    public GameStatus GameStatus;
    public GameMode Mode;

    public float startRandomCoin = 15f;
    public float destroyBlockInterval = 3f;

    public GameSettings Settings { get; set; }

    private MapGenerator _mapGenerator;
    private PlayerManager _playerManager;

    private bool _isHurryUpProcedureStarted;
    private bool _isClosing;
    private bool _isGimmickRunning;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //중복으로 존재할시에는 파괴!
            Destroy(this.gameObject);
        }

        Application.runInBackground = true;
        Screen.SetResolution(1920, 1080, true);

        _mapGenerator = GetComponent<MapGenerator>();
        _playerManager = GetComponent<PlayerManager>();
        LoadSettings();

        runningTimeBeforeHurryUp = Settings.RunningTime > 0 ? Settings.RunningTime : 120;

        ExitInputManager.OnExitPressed += ExitInputManager_OnExitPressed;
        ExitInputManager.OnCleanExitPressed += ExitInputManager_OnCleanExitPressed;
    }

    private async void ExitInputManager_OnCleanExitPressed()
    {
        await ExitGame(true);
        ExitInputManager.OnExitPressed -= ExitInputManager_OnExitPressed;
        ExitInputManager.OnCleanExitPressed -= ExitInputManager_OnCleanExitPressed;
    }

    private async void ExitInputManager_OnExitPressed()
    {
        await ExitGame();
        ExitInputManager.OnExitPressed -= ExitInputManager_OnExitPressed;
        ExitInputManager.OnCleanExitPressed -= ExitInputManager_OnCleanExitPressed;
    }

    private void LoadSettings()
    {
        var path = Path.Combine(Application.streamingAssetsPath, ConfigPath, ConfigFile);
        var folder = Path.GetDirectoryName(path);
        if (Directory.Exists(folder) is false)
        {
            Directory.CreateDirectory(folder);
        }

        if (File.Exists(path) is false)
        {
            Settings = new GameSettings();
            SaveDefaultGameSettings(path, Settings);
        }

        var json = File.ReadAllText(path);
        var settings = JsonUtility.FromJson<GameSettings>(json);
        Settings = settings;
    }

    private void SaveDefaultGameSettings(string settingFilePath, GameSettings settings)
    {
        using var file = File.Create(settingFilePath);
        var result = JsonUtility.ToJson(settings, true);
        var buffer = Encoding.UTF8.GetBytes(result);
        file.Write(buffer);
    }

    public void SetGameMode(GameMode mode)
        => Mode = mode;

    public UniTask<IEnumerable<PlayerContext>> LoadPlayers()
        => _playerManager.LoadPlayers();

    public void InitMap()
    {
        _mapGenerator.InitMapContext();
    }

    /// <summary>
    /// 실제 게임을 진행할 플레이어 인스턴스를 생성해 맵 내부에 위치시킨다.
    /// </summary>
    public async UniTask InitPlayers()
    {
        // PlayerManager, MapGenerator 생성과 로딩 후 사용 가능.
        if (SceneManager.GetActiveScene().name != "Game")
        {
            Debug.LogError("can't load or init players. Game scene was not loaded.");
            return;
        }

        var column = _mapGenerator.column;
        var row = _mapGenerator.row;

        var playerPositions = new Dictionary<int, Vector2>(4);
        switch (Mode)
        {
            case GameMode.Contest:
                playerPositions.Add(0, _mapGenerator.p1Position);
                playerPositions.Add(1, _mapGenerator.p2Position);
                playerPositions.Add(2, _mapGenerator.p3Position);
                playerPositions.Add(3, _mapGenerator.p4Position);
                break;
            case GameMode.Test: // test 는 무조건 1번자리에 배치.
            case GameMode.Single1:
                playerPositions.Add(0, _mapGenerator.p1Position);
                break;
            case GameMode.Single2:
                playerPositions.Add(1, _mapGenerator.p2Position);
                break;
            case GameMode.Single3:
                playerPositions.Add(2, _mapGenerator.p3Position);
                break;
            case GameMode.Single4:
                playerPositions.Add(3, _mapGenerator.p4Position);
                break;
            default:
                break;
        }

        await _playerManager.InitPlayer(column, row, playerPositions);
    }

    public IEnumerable<PlayerContext> GetAllPlayerContexts()
        => _playerManager.GetPlayerContexts();

    public void SetPlayerScore(GameObject coin, int playerIndex, int point)
    {
        RemoveCoin(coin);
        _playerManager.SetScore(playerIndex, point);
    }

    public int GetPlayerScore(int playerIndex)
        => _playerManager.GetScore(playerIndex);

    public IEnumerable<int> GetPlayerRanking()
        => _playerManager.GetPlayerRanking();

    public async UniTask CloseAllPlayerHost(bool isForcePlayerHostShutdown)
    {
        var playerContexts = _playerManager.GetPlayerContexts();
        // 종료 전 cleanup 호출.
        var playerForms = playerContexts.Select(player => player.Player).ToList();
        var playerGroups = playerForms.GroupBy(form => form.GetType());
        if (isForcePlayerHostShutdown)
        {
            Settings.CloseWithoutPlayerHostExit = false;
        }

        foreach (var group in playerGroups)
        {
            var platform = group.Key;
            switch (platform)
            {
                case Type _ when platform == typeof(CsPlayerRunner):
                    await CsPlayerRunner.CleanupHost();
                    await CsPlayerRunner.CloseHost();
                    break;
                case Type _ when platform == typeof(JsPlayerRunner):
                    await JsPlayerRunner.CleanupHost();
                    await JsPlayerRunner.CloseHost();
                    break;
                case Type _ when platform == typeof(DummyPlayer):
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Game")
        {
            return;
        }

        StartRandomGimmick();

        CheckPlayTime();

        CheckMouseClickToRemoveItem();

        if (Input.GetKey(KeyCode.Return))
        {
            SceneManager.LoadScene("Result");
            return;
        }
    }

    private void CheckPlayTime()
    {
        if (GameStatus is not GameStatus.Playing)
        {
            return;
        }

        if (runningTimeBeforeHurryUp > 0)
        {
            runningTimeBeforeHurryUp -= Time.deltaTime;
        }

        if (runningTimeBeforeHurryUp <= 0)
        {
            if (_isHurryUpProcedureStarted is true)
            {
                return;
            }

            _isHurryUpProcedureStarted = true;
            GameStatus = GameStatus.HurryUp;
            GoHurryUp();
        }
    }

    private void CheckMouseClickToRemoveItem()
    {
        if (Mode == GameMode.Contest) // 클릭 테스트는 contest 모드에서는 사용 불가.
        {
            return;
        }

        if (Input.GetMouseButtonDown(0)) // 좌클릭은 코인이나 블럭 제거.
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            var hit = Physics2D.Raycast(worldPoint, Vector2.zero);
            if (hit.collider is not null) // coin
            {
                var collision = hit.collider;
                if (collision.CompareTag("Items"))
                {
                    var coin = collision.gameObject;

                    // remove coin
                    Destroy(coin);
                    RemoveCoin(coin);
                }

                return;
            }

            // block
            var index = CoordinateService.ToIndex(_mapGenerator.column, _mapGenerator.row, worldPoint);

            _mapGenerator.RemoveBlock(index);
        }

        if (Input.GetMouseButtonDown(1)) // 우클릭은 blackMatter 생성.
        {
            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var index = CoordinateService.ToIndex(_mapGenerator.column, _mapGenerator.row, worldPoint);

            _mapGenerator.AddBlackMatter(index);
        }
    }

    private void RemoveCoin(GameObject coin)
    {
        var coinIndex = CoordinateService.ToIndex(_mapGenerator.column, _mapGenerator.row, coin.transform.position);
        GameInfoService.Instance.RemoveItem(coinIndex);
    }

    private void GoHurryUp()
    {
        StartCoroutine(ChangeCameraBackgroundLerp());
        StartCoroutine(ReduceCoinsInHurryUp());
    }

    /// <summary>
    /// hurryUp 이미지를 강조할 camera background blinking animation 구현.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeCameraBackgroundLerp()
    {
        var camera = Camera.main;
        var originColor = camera.backgroundColor;
        var moveColor = hurryUpBackground;
        var swap = moveColor;
        var elapsed = 0f;
        var transitionRate = 0f;
        while (true)
        {
            if (GameStatus is not GameStatus.HurryUp)
            {
                yield break;
            }

            while (elapsed < hurryUpBlinkingDuration)
            {
                transitionRate = elapsed / hurryUpBlinkingDuration;
                elapsed += Time.deltaTime;

                if (camera == null)
                {
                    yield break;
                }

                if (camera.gameObject == null)
                {
                    yield break;
                }

                if (camera.gameObject.activeSelf)
                {
                    camera.backgroundColor = Color.Lerp(originColor, moveColor, transitionRate);
                }

                yield return null;
            }

            // 색 반전을 위해 다시 되돌아감.
            elapsed = 0;
            swap = moveColor;
            moveColor = originColor;
            originColor = swap;

            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// HurryUp 상태에 도달 시 남은 코인을 제거한다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReduceCoinsInHurryUp()
    {
        while (GameInfoService.Instance.IsClearAllCoins() is false)
        {
            // 제일 싼 코인부터 제거.
            yield return StartCoroutine(RemoveCoins(CoinType.Copper, 0.3f));
            yield return StartCoroutine(RemoveCoins(CoinType.Silver, 0.7f));
            yield return StartCoroutine(RemoveCoins(CoinType.Gold, 1.0f));
            yield return StartCoroutine(RemoveCoins(CoinType.Diamond, 1.3f));
            yield return StartCoroutine(RemoveCoins(CoinType.BlackMatter, 1.3f));
        }

        GameStatus = GameStatus.GameSet;

        yield return null;
    }

    private void StartRandomGimmick()
    {
        if (GameStatus is not GameStatus.Playing)
        {
            return;
        }

        if (_isGimmickRunning)
        {
            return;
        }

        if (Settings.UseRandomGimmick is false)
        {
            return;
        }

        _isGimmickRunning = true;

        // gimmick 수행.
        StartCoroutine(RunDestroyBlocksGimmick());
        StartCoroutine(GenerateRandomBlackMatter());
    }

    /// <summary>
    /// 지정된 시간 간격으로 랜덤하게 block 을 파괴한다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RunDestroyBlocksGimmick()
    {
        yield return new WaitForSeconds(destroyBlockInterval);

        while (GameInfoService.Instance.IsClearAllCoins() is false)
        {
            if (GameStatus is GameStatus.GameSet)
            {
                yield return null;
            }

            // random 으로 block 파괴.
            var removeBlock = GameInfoService.Instance.GetRandomBlockIndex();
            if (removeBlock is -1) // 선택 가능한 block 이 없음.
            {
                yield break;
            }

            if (GameStatus is GameStatus.GameSet)
            {
                yield break;
            }

            _mapGenerator.RemoveBlock(removeBlock);

            yield return new WaitForSeconds(destroyBlockInterval);
        }

        yield return null;
    }

    /// <summary>
    /// 지정된 시간 간격이 되면 blackMatter 4개를 빈 자리에 생성한다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GenerateRandomBlackMatter()
    {
        while (GameStatus is GameStatus.Playing)
        {
            yield return new WaitForSeconds(startRandomCoin);

            if (GameStatus is not GameStatus.Playing)
            {
                yield return null;
            }

            if (GameInfoService.Instance.IsClearAllCoins()) // 방어처리
            {
                yield return null;
            }

            var playerPositions = _playerManager.GetAllPlayersPositions();
            var emptyTiles = GameInfoService.Instance.GetRandomEmptyTiles(4, playerPositions); // 4 개만 생성한다.
            if (emptyTiles.Length is 0)
            {
                continue;
            }

            foreach (var tilePosition in emptyTiles)
            {
                _mapGenerator.AddBlackMatter(tilePosition);
            }
        }

        yield return null;
    }

    private IEnumerator RemoveCoins(CoinType coinType, float delayTime)
    {
        var randomCoins = GameInfoService.Instance.GetRandomCoinIndexes(coinType);
        if (randomCoins.Any() is false)
        {
            yield break;
        }

        foreach (var coin in randomCoins)
        {
            var removeResult = _mapGenerator.RemoveCoin(coin);
            if (removeResult is CoinActionResult.NotExists) // 이미 지워졌으면 다음 코인 삭제 시도.
            {
                continue;
            }

            GameInfoService.Instance.RemoveItem(coin);
            yield return new WaitForSeconds(delayTime);
        }
    }


    public (int row, int column) GetGameGridRange()
        => (_mapGenerator.row, _mapGenerator.column);

    public async UniTask ExitGame(bool isForcePlayerHostShutdown = false)
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;

        await CloseAllPlayerHost(isForcePlayerHostShutdown);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
}
