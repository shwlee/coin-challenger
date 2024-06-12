using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    private Camera _mainCamera;

    /// <summary>
    /// 미리 준비된 map 객체. 
    /// <para>map 은 0,0 을 기준으로 외벽을 제외한 ground 를 절반씩 생성한 값을 가진다. 
    /// 즉, 20x10 grid 일 때 0,0 을 기준으로 위로 10칸, 오른쪽으로 5칸 생성한 값을 가진다. </para>
    /// </summary>
    public GameObject _map;

    public int column;
    public int row;

    /// <summary>
    /// left top position
    /// </summary>    
    public Vector2 p1Position;

    /// <summary>
    /// right top position
    /// </summary>
    public Vector2 p2Position;

    /// <summary>
    /// left bottom position
    /// </summary>
    public Vector2 p3Position;

    /// <summary>
    /// right bottom position
    /// </summary>
    public Vector2 p4Position;

    private Tilemap _blocks;

    private int[] _mapBag;

    /// <summary>
    /// 생성된 map 을 읽어서 map 정보를 추출, 보관한다.
    /// </summary>
    public void InitMapContext()
    {
        if (_map is null)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name != "Game")
        {
            Debug.LogError("can't load or init map context. GameScene was not loaded.");
            return;
        }

        var mapStructure = _map.GetComponent<MapStructure>();
        Instantiate(_map); // TODO : map 선택해서 로딩 가능하도록.		

        var grid = _map.GetComponent<Grid>();
        var tilemaps = grid.gameObject.GetComponentsInChildren<Tilemap>();
        _blocks = tilemaps[1];

        column = mapStructure.column;
        row = mapStructure.row;
        _mapBag = new int[column * row];

        var blockers = _blocks.cellBounds.allPositionsWithin;
        foreach (var position in blockers)
        {
            if (_blocks.HasTile(position) is false)
            {
                continue;
            }

            // blocks 는 map 의 자식 좌표이므로 anchor 를 0으로 계산해야한다.
            var bagIndex = CoordinateService.ToIndex(column, row, position.x, position.y, 0);
            _mapBag[bagIndex] = -1;
        }

        InitPlayerPoistion();

        // map 초기화가 끝나면 코인 초기화 진행.
        InitCoins();

        // 초기화 과정이 끝나면 GameInfoService 초기화
        GameInfoService.Instance.Init(column, row, _mapBag);
        Debug.Log(JsonUtility.ToJson(GameInfoService.Instance.GetMapInfo()));

        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        Debug.Log($"current camera size:{_mainCamera.orthographicSize}");

        _mainCamera.orthographicSize = mapStructure.cameraDistance;
        Debug.Log($"set camera size:{_mainCamera.orthographicSize}");
    }

    /// <summary>
    /// 각 플레이어의 초기 위치를 지정한다. map 의 각 네 귀퉁이 위치를 기준으로 한다.
    /// <para>좌상단부터 p1, 우상단 p2, 좌하단 p3, 우하단 p4</para>
    /// </summary>
    private void InitPlayerPoistion()
    {
        p1Position = new Vector2((float)(0 - (column / 2) + 0.5), (float)(0 + (row / 2) - 0.5));
        p2Position = new Vector2((float)(0 + (column / 2) - 0.5), (float)(0 + (row / 2) - 0.5));
        p3Position = new Vector2((float)(0 - (column / 2) + 0.5), (float)(0 - (row / 2) + 0.5));
        p4Position = new Vector2((float)(0 + (column / 2) - 0.5), (float)(0 - (row / 2) + 0.5));
    }

    /// <summary>
    /// map 할당된 coin 정보를 추출한다.
    /// </summary>
    private void InitCoins()
    {
        // tag == "Coins" 하위에 각 prefab 들이 할당되어 있다.
        var coins = _map.transform.Find("Coins");
        if (coins is null)
        {
            Debug.Log("There is no coins.");
            return;
        }
        Debug.Log(coins.name);

        foreach (Transform coin in coins.transform)
        {
            var coinContext = coin.GetComponent<CoinContext>();
            var bagIndex = CoordinateService.ToIndex(column, row, coin.position.x, coin.position.y);
            _mapBag[bagIndex] = coinContext.CoinPoint;
        }
    }

    public CoinActionResult RemoveCoin(int coinIndex)
    {
        var coins = GameObject.FindGameObjectsWithTag("Items");
        var target = coins.FirstOrDefault(coin => CoordinateService.ToIndex(column, row, coin.transform.position) == coinIndex);
        if (target is null)
        {
            return CoinActionResult.NotExists;
        }

        Debug.Log($"remove coin:{coinIndex}");
        Destroy(target);

        return CoinActionResult.Deleted;
    }
}
