using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

public partial class RunnerHostMediator
{
    private static List<string> _checked = new();

    /// <summary>
    /// 해당 runner 가 떠 있는 지 확인.
    /// </summary>
    /// <param name="port"></param>
    /// <param name="platform"></param>
    /// <returns></returns>
    internal async static UniTask<bool> CheckHealthy(int port, string platform)
    {
        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartGame}/{Healthy}";
        try
        {
            using var request = UnityWebRequest.Get(url);
            var response = await request.SendWebRequest().WithCancellation(CancellationToken.None);
            return response.result == UnityWebRequest.Result.Success;
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
            return false;
        }
    }

    internal async static UniTask SetCurrentGame(int port, string platform, string gameId, int column, int row)
    {
        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartGame}/{Set}?gameId={gameId}&column={column}&row={row}";
        try
        {
            using var request = UnityWebRequest.Post(url, (WWWForm)null);
            await request.SendWebRequest().WithCancellation(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    internal async static UniTask<bool> CloseRunner(int port, string platform)
    {
        if (GameManager.Instance.Settings.CloseWithoutPlayerHostExit)
        {
            return true;
        }

        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartGame}/{Shutdown}";
        try
        {
            using var request = UnityWebRequest.Post(url, (WWWForm)null);
            await request.SendWebRequest().WithCancellation(CancellationToken.None);

            await UniTask.Delay(1000);
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }

    internal async static UniTask LoadPlayerInstance(int port, string platform, int position, string filePath)
    {
        var form = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("position", position.ToString()),
            new MultipartFormDataSection("filePath", filePath)
        };

        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartPlayer}/{LoadPlayer}";
        try
        {
            using var request = UnityWebRequest.Post(url, form);
            await request.SendWebRequest().WithCancellation(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    internal async static UniTask<string> GetPlayerName(int port, string platform, int myNumber)
    {
        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartPlayer}/{GetName}/{myNumber}";
        try
        {
            using var request = UnityWebRequest.Get(url);
            var response = await request.SendWebRequest().WithCancellation(CancellationToken.None);
            if (response.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"error in get name process.port:{port}, platform:{platform}");
            }

            return response.downloadHandler.text;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);

            throw;
        }
    }

    internal async static UniTask InitializePlayer(int port, string platform, int myNumber, int column, int row)
    {
        var form = new List<IMultipartFormSection>
        {
            new MultipartFormDataSection("position", myNumber.ToString()),
            new MultipartFormDataSection("column", column.ToString()),
            new MultipartFormDataSection("row", row.ToString())
        };

        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartPlayer}/{InitPlayer}";
        try
        {
            using var request = UnityWebRequest.Post(url, form);
            await request.SendWebRequest().WithCancellation(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    internal async static UniTask<int?> GetMoveNextDirection(int port, string platform, int myNumber, int turn, int[] map, int currentPosition)
    {
        var message = new GameMessage { turn = turn, position = myNumber, map = map, current = currentPosition };
        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartPlayer}/{Movenext}";
        try
        {
            var gameData = JsonUtility.ToJson(message);
            using var request = UnityWebRequest.Post(url, gameData, "application/json");
            var response = await request.SendWebRequest().WithCancellation(CancellationToken.None);
            if (int.TryParse(response.downloadHandler.text, out var result) is false)
            {
                return null;
            }
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return -1; // 예외 대신 무조건 -1. -1 이면 1턴 쉰다.
        }
    }

    internal async static UniTask PrepareRunnerHost(int port, string platform, string runnerPath)
    {
        var runnerHealthy = await CheckHealthy(port, platform);
        if (runnerHealthy)
        {
            if (_checked.Contains(platform)) // platform 별 1회만 체크.
            {
                return;
            }

            _checked.Add(platform);

            runnerHealthy = await CloseRunner(port, platform);
        }

        // runner execute

        if (runnerHealthy)
        {
            return;
        }

        var path = Path.Combine(Application.streamingAssetsPath, runnerPath);
        var runnerProcess = new Process();
        runnerProcess.StartInfo.FileName = path;
        runnerProcess.StartInfo.Arguments = port.ToString();
        runnerProcess.StartInfo.UseShellExecute = false;
        runnerProcess.StartInfo.CreateNoWindow = true;
        runnerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        runnerProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);

        var process = runnerProcess.Start();
        _checked.Add(platform);

        await UniTask.Delay(5000);
    }

    internal async static UniTask CleanupPlayerHost(int port, string platform)
    {
        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartGame}/{Cleanup}";
        try
        {
            using var request = UnityWebRequest.Post(url, null, "application/json");
            await request.SendWebRequest().WithCancellation(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }
}
