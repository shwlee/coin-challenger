using System;
using System.Collections.Generic;

[Serializable]
public class GameResult
{
    public List<Result> Results = new();
}

[Serializable]
public class Result
{
    public int Rank;
    public string Name;
    public int Score;
}
