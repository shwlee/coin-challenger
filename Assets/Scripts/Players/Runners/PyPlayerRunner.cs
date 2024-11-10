public class PyPlayerRunner : PlayerRunnerBase<PyPlayerRunner>
{
    public PyPlayerRunner()
      : base(GameManager.Instance.Settings.PyHostport, Consts.Python, GameManager.Instance.Settings.PyHostPath)
    {
    }
}
