public class JsPlayerRunner : PlayerRunnerBase
{
    public JsPlayerRunner()
      : base(GameManager.Instance.Settings.JsHostport, Consts.Js, GameManager.Instance.Settings.JsHostPath)
    {
    }
}
