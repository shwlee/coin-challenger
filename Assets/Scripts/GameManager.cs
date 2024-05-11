using Cysharp.Threading.Tasks;
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
    public float hurryUpBlinkingDuration = 0.5F;
    public Color hurryUpBackground = Color.red;

    public GameStatus GameStatus;
    public GameMode Mode;

    public GameSettings Settings { get; set; }

    private MapGenerator _mapGenerator;
    private PlayerManager _playerManager;

    private bool _isHurryUpProcedureStarted;

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

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Game")
        {
            return;
        }

        CheckPlayTime();

        CheckMouseClickForCoinRemove();

        if (Input.GetKey(KeyCode.Return))
        {
            SceneManager.LoadScene("Result");
        }
        else if (Input.GetKey("escape"))
        {
            ExitGame(); // 즉시 종료.
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

    private void CheckMouseClickForCoinRemove()
    {
        if (Mode == GameMode.Contest) // 마우스 coin 제거는 contest 모드에서는 사용 불가.
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) is false)
        {
            return;
        }

        var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider == null)
        {
            return;
        }

        var collision = hit.collider;
        if (collision.CompareTag("Items"))
        {
            var coin = collision.gameObject;

            // remove coin
            Destroy(coin);
            RemoveCoin(coin);
        }
    }

    private void RemoveCoin(GameObject coin)
    {
        var coinIndex = CoordinateService.ToIndex(_mapGenerator.column, _mapGenerator.row, coin.transform.position);
        GameInfoService.Instance.RemoveCoinByIndex(coinIndex);
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

    private IEnumerator RemoveCoins(CoinType coinType, float delayTime)
    {
        var randomCoins = GameInfoService.Instance.GetRandomCoinIndexes(coinType);
        if (randomCoins.Any() is false)
        {
            yield break;
        }

        foreach (var blackMatter in randomCoins)
        {
            var removeResult = _mapGenerator.RemoveCoin(blackMatter);
            if (removeResult is CoinActionResult.NotExists) // 이미 지워졌으면 다음 코인 삭제 시도.
            {
                continue;
            }

            GameInfoService.Instance.RemoveCoinByIndex(blackMatter);
            yield return new WaitForSeconds(delayTime);
        }
    }


    public (int row, int column) GetGameGridRange()
        => (_mapGenerator.row, _mapGenerator.column);

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
}
