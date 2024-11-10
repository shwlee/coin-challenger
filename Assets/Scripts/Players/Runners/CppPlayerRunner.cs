public class CppPlayerRunner : PlayerRunnerBase<CppPlayerRunner>
{
    public CppPlayerRunner()
      : base(GameManager.Instance.Settings.CppHostport, Consts.Cpp, GameManager.Instance.Settings.CppHostPath)
    {
    }
}
