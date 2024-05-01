public class CsPlayerRunner : PlayerRunnerBase
{
    public CsPlayerRunner()
        : base(GameManager.Instance.Settings.CsharpHostport, Consts.Csharp, GameManager.Instance.Settings.CsharpHostPath)
    {
    }
}
