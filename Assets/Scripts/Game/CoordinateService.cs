using System;
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

       // 그리드의 절대적 좌표를 기준으로 계산
        int width = (int)Math.Floor(x + column / 2.0); // x축은 좌우로 펼쳐지므로 2로 나눔
        int height = (int)Math.Floor(row / 2.0 - y); // y축은 위아래로 움직이므로 반대로 처리

        // 범위를 벗어나는 경우 처리
        width = Math.Clamp(width, 0, column - 1);
        height = Math.Clamp(height, 0, row - 1);

        // 1차원 인덱스 계산
        return height * column + width;
    }

    public static (float x, float y) ToUnity2dCoordinate(int column, int row, int index, float anchor = 0.5f)
    {
        var cMin = -(column / 2);
        var rMax = (row / 2) - 1;
        var x = (index % column) + cMin;
        var y = rMax - (index / column);
        return (x + anchor, y + anchor);
    }
}