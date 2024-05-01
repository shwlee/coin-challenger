public static class Consts
{
    public const int MaxPlayerCount = 4;
    public const string ExternalsPath = "./externals";
    public const string PlayerModulePath = "./externals/players";
    public const string ResultSavePath = "./externals/Results";

    public const string CsFile = ".cs";
    public const string JsFile = ".js";
    public const string PyFile = ".py";
    public static readonly string[] PlatformFiles = new string[]
    {
        CsFile,
        JsFile,
        PyFile
    };

    public const string Csharp = "csharp";
    public const string Js = "js";
    public const string Cpp = "cpp";
    public const string python = "py";
}

public enum GameStatus
{
    BeforeStart,
    Starting,
    Playing,
    HurryUp, // 남은 코인이 하나씩 사라진다.
    GameSet
}

public enum GameMode
{
    Single1 = 0, // 1개 플레이어만 로딩. 0번 자리 로딩.
    Single2 = 1, // 1개 플레이어만 로딩. 1번 자리 로딩.
    Single3 = 2, // 1개 플레이어만 로딩. 2번 자리 로딩.
    Single4 = 3, // 1개 플레이어만 로딩. 3번 자리 로딩.
    Contest, // 정상 경쟁 모드
    Test, // 테스트 모드. 1개 더미만 로딩. 키보드로 움직임 조작.
}

public enum CoinType
{
    Cooper = 10,
    Silver = 30,
    Gold = 100,
    Diamond = 200,
    BlackMatter = 500
}

public enum MoveDirection
{
    Left = 0,
    Up = 1,
    Right = 2,
    Down = 3
}

public enum CoinActionResult
{
    Normal,
    NotExists,
    Deleted,
}
