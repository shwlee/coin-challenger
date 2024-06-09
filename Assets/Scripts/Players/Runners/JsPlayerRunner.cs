public class JsPlayerRunner : PlayerRunnerBase<JsPlayerRunner>
{
    public JsPlayerRunner()
      : base(GameManager.Instance.Settings.JsHostport, Consts.Js, GameManager.Instance.Settings.JsHostPath)
    {
    }
}
