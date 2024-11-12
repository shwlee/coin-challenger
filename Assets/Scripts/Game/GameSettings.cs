using System;

[Serializable]
public class GameSettings
{
    public int RunningTime;

    public int CsharpHostport;
    public string CsharpHostPath;

    public int JsHostport;
    public string JsHostPath;

    public int CppHostport;
    public string CppHostPath;

    public int PyHostport;
    public string PyHostPath;

    public bool CloseWithoutPlayerHostExit;
    public bool UseRandomGimmick = true;
}