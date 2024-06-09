public class CsPlayerRunner : PlayerRunnerBase<CsPlayerRunner>
{
    public CsPlayerRunner()
        : base(GameManager.Instance.Settings.CsharpHostport, Consts.Csharp, GameManager.Instance.Settings.CsharpHostPath)
    {
    }
}
