using Cysharp.Threading.Tasks;

public interface IPlayer
{
    /// <summary>
    /// player host 에 현재 게임 정보 셋업 요청을 보냅니다.
    /// </summary>
    /// <param name="gameId"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    UniTask GameSetup(string gameId, int column, int row);

    /// <summary>
    /// 기준 파일의 경로를 설정합니다. 경로에 위치한 파일을 읽어서 인스턴스를 설정합니다.
    /// </summary>
    /// <param name="path"></param>
    UniTask Setup(int myNumber, string path);

    /// <summary>
    /// Player 를 초기화 합니다. 게임 환경을 인자로 전달받습니다. 전달받은 인자는 게임 동안 유지해야합니다.
    /// </summary>
    /// <param name="myNumber">배정받은 번호.(플레이 순서)</param>
    /// <param name="column">현재 생성된 보드의 열.</param>
    /// <param name="row">현재 생성된 보드의 행.</param>		
    UniTask Initialize(int column, int row);

    /// <summary>
    /// Player 의 이름을 반환합니다. 현재 플레이어의 이름을 하드코딩하여 반환합니다.
    /// </summary>
    /// <param name="myNumber">현재 플레이어 순번.</param>    
    /// <returns>현재 플레이어 이름.</returns>
    UniTask<string> GetName();

    /// <summary>
    /// 현재 턴에서 진행할 방향을 결정합니다. map 정보와 현재 플레이어의 위치를 전달받은 후 다음 이동 방향을 반환해야 합니다.
    /// <para>결정 방향은 left, up, right, down 순서로 0, 1, 2, 3 정수로 표현해야합니다.</para>
    /// </summary>
    /// <param name="myNumber">현재 순번.</param>    
    /// <param name="map">1차원 배열로 표현된 현재 map 정보.</param>
    /// <param name="currentPosition">현재 플레이어의 위치. map 배열의 인덱스로 표시됨.</param>    
    /// <returns>이번 프레임에 진행할 방향. left, up, right, down 순서오 0, 1, 2, 3 으로 표현.</returns>
    UniTask<int?> MoveNext(int turn, int[] map, int currentPosition);
}
