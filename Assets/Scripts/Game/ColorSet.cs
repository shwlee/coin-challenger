using UnityEngine;

public class ColorSet // NOTE: 매 플레이어마다 자른 sprite 를 적용하면 필요 없어짐.
{
    public static Color GetColorBySeq(int seq)
    {
        return seq switch
        {
            0 => Color.white,
            1 => Color.green,
            2 => Color.grey,
            3 => Color.magenta,
            _ => Color.cyan
        };
    }
}