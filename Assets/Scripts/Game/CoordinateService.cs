using UnityEngine;

public static class CoordinateService
{
    /// <summary>
    /// object 의 position 에서 map 배열의 index 를 반환합니다.
    /// </summary>
    /// <param name="column">map 배열의 total column</param>
    /// <param name="row">map 배열의 total row</param>
    /// <param name="x">gameObject 의 x</param>
    /// <param name="y">gameObject 의 y</param>
    /// <param name="anchor">game</param>
    /// <returns></returns>
    public static int ToIndex(int column, int row, float x, float y, float anchor = 0.5f)
        => CovertToIndex(column, row, x, y, anchor);

    public static int ToIndex(int column, int row, Vector2 position, float anchor = 0.5f)
        => CovertToIndex(column, row, position.x, position.y, anchor);

    private static int CovertToIndex(int column, int row, float x, float y, float anchor = 0.5f)
    {
        x -= anchor;
        y -= anchor;

        var rMax = (row / 2);
        var cHalf = (column / 2);

        var xi = cHalf + x;
        var yi = column * (rMax - y - 1);

        return (int)(xi + yi);
    }

    public static (float x, float y) ToUnity2dCoordiate(int column, int row, int index, float anchor = 0.5f)
    {
        var cMin = -(column / 2);
        var rMax = (row / 2) - 1;
        var x = (index % column) + cMin;
        var y = rMax - (index / column);
        return (x + anchor, y + anchor);
    }
}