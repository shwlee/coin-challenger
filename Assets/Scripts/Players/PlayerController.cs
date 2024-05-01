using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private LayerMask _hitBlockMask;
    private int _wallsLayer;
    private IPlayer _player;

    private float _layDistance = 1.0f;
    private float _moveTime = 0.3f;
    private bool _isMoving = false;
    private Animator _animator;

    public int Position;

    public void SetPlayerModule(IPlayer module) => _player = module;

    // Start is called before the first frame update
    void Start()
    {
        _wallsLayer = LayerMask.NameToLayer("Walls");
        _hitBlockMask |= 1 << _wallsLayer;
        _animator = GetComponent<Animator>();
        _animator.SetInteger("MoveDirection", 3); // down 방향을 기본값으로
    }

    // Update is called once per frame
    async void Update()
    {
        if (GameManager.Instance.GameStatus is not (GameStatus.Playing or GameStatus.HurryUp))
        {
            return;
        }

        if (_player is null)
        {
            Debug.LogError("The player module is not set.");
            return;
        }

        if (IsGameSet())
        {
            return;
        }

        await MoveNext();
    }

    public async Task MoveNext()
    {
        if (_isMoving)
        {
            return;
        }

        if (IsGameSet())
        {
            return;
        }

        _isMoving = true;

        var (column, row, map) = GameInfoService.Instance.GetMapInfo();
        var x = RoundToHalf(transform.position.x);
        var y = RoundToHalf(transform.position.y);
        var current = CoordinateService.ToIndex(column, row, x, y);

        var direction =
            GameManager.Instance.Mode is not GameMode.Test ? await CalcToWhere(map, current) : GetDirectionByInput();

        if (direction is null)
        {
            _isMoving = false;
            return;
        }

        SetMoveAnimation((int)direction);

        var moveTo = ConvertFromDirection(direction.Value);
        if (CanMoveNext(moveTo) is false)
        {
            _isMoving = false;
            return;
        }

        if (gameObject.activeSelf)
        {
            StartCoroutine(MoveSmoothGrid(moveTo));
        }
    }

    private bool CanMoveNext(Vector2 direction)
        => Physics2D.Raycast(transform.position, direction, _layDistance, _hitBlockMask).transform is null;

    /// <summary>
    /// 사용자 함수 호출. 비동기로 수행하여 main thread 에 영향을 주지 않게 처리한다.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    private async UniTask<MoveDirection?> CalcToWhere(int[] map, int current)
    {
        // 요기서 사용자 알고리즘 함수 호출.
        var result = await _player.MoveNext(map, current);
        if (result is null || result == -1)  // null or -1 이면 비정상 결과.
        {
            // 1초 페널티.
            await UniTask.Delay(1000);
            return null;
        }
        return (MoveDirection)result;
    }

    private Vector2 ConvertFromDirection(MoveDirection direction)
        => direction switch
        {
            MoveDirection.Left => Vector2.left,
            MoveDirection.Right => Vector2.right,
            MoveDirection.Up => Vector2.up,
            MoveDirection.Down => Vector2.down,
            _ => throw new NotImplementedException()
        };

    private float RoundToHalf(float number)
    {
        // 위치 좌표가 미묘하게 틀어져서 들어오는 경우가 있다. 이를 0.5 단위로 맞춰준다.
        // 숫자를 2배 증가시켜 가장 가까운 정수로 반올림한 후 다시 2로 나눈다..
        return (float)(Math.Round(number * 2, MidpointRounding.AwayFromZero) / 2.0);
    }

    private IEnumerator MoveSmoothGrid(Vector3 direction)
    {
        _isMoving = true;

        if (IsGameSet())
        {
            yield break;
        }

        var startPosition = transform.position;
        var endPosition = startPosition + direction;
        var elapsed = 0f;
        while (elapsed < _moveTime)
        {
            elapsed += Time.deltaTime;

            transform.position = Vector2.Lerp(startPosition, endPosition, elapsed / _moveTime);
            yield return null;
        }

        var lastPosition = new Vector3(RoundToHalf(endPosition.x), RoundToHalf(endPosition.y));
        transform.position = lastPosition; // 최종 위치 보정.
        _isMoving = false;
    }

    private void SetMoveAnimation(int direction)
    {
        if (IsGameSet())
        {
            return;
        }

        if (_animator is null)
        {
            return;
        }

        var last = _animator.GetInteger("MoveDirection");
        if (last == direction)
        {
            return;
        }

        _animator.SetInteger("MoveDirection", direction);
    }

    private bool IsGameSet()
        => GameManager.Instance.GameStatus is GameStatus.GameSet;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Items"))
        {
            var coin = collision.gameObject.GetComponent<CoinContext>();

            // remove coin
            Destroy(collision.gameObject);
            GameManager.Instance.SetPlayerScore(collision.gameObject, Position, coin.CoinPoint);
        }
    }

    #region test key input

    private MoveDirection? GetDirectionByInput()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            return MoveDirection.Up;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            return MoveDirection.Down;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            return MoveDirection.Left;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            return MoveDirection.Right;
        }

        return null;
    }

    #endregion
}
