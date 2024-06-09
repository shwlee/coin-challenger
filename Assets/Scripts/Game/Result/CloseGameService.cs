using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class CloseGameService : MonoBehaviour
{
    public GameObject Winner;
    public GameObject SecondPlace;
    public GameObject ThirdPlace;
    public GameObject LastPlace;

    private GameResult _gameResult = new GameResult();

    void Start()
    {
        var allPlayers = GameManager.Instance.GetAllPlayerContexts().OrderByDescending(player => player.Score);
        SetResult(Winner, allPlayers, 0);
        if (GameManager.Instance.Mode is GameMode.Contest)
        {
            SetResult(SecondPlace, allPlayers, 1);
            SetResult(ThirdPlace, allPlayers, 2);
            SetResult(LastPlace, allPlayers, 3);
        }

        foreach (var player in allPlayers)
        {
            player.PlayerObject?.gameObject.SetActive(false);
        }
    }

    private void SetResult(GameObject position, IEnumerable<PlayerContext> allPlayers, int rank)
    {
        var context = allPlayers.ElementAt(rank);
        var rankContext = position.GetComponent<ResultRankContext>();
        rankContext.Set(context);

        _gameResult.Results.Add(new Result { Rank = rank, Name = context.Name, Score = context.Score });
    }

    // Update is called once per frame
    async void Update() 
        => await Escape.ExitIfInputEscape(() => ExitGame());

    public async void ExitGame()
    {
        // 결과를 파일로 저장한 후 종료한다.
        SaveResult();
        await GameManager.Instance.ExitGame();
    }

    private void SaveResult()
    {
        if (_gameResult.Results.Count < 1)
        {
            return;
        }

        try
        {
            if (Directory.Exists(Consts.ResultSavePath) is false)
            {
                Directory.CreateDirectory(Consts.ResultSavePath);
            }

            var fileName = $"Result_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log";
            var savePath = Path.Combine(Consts.ResultSavePath, fileName);
            using var file = File.Create(savePath);
            var result = JsonUtility.ToJson(_gameResult, true);
            var buffer = Encoding.UTF8.GetBytes(result);
            file.Write(buffer);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
