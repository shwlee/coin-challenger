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

    internal async static UniTask CloseRunner(int port, string platform)
    {
        if (GameManager.Instance.Settings.CloseWithoutPlayerHostExit)
        {
            return;
        }

        var rootPath = string.Format(HostRoot, port, platform);
        var url = $"{rootPath}/{PartGame}/{Shutdown}";
        try
        {
            using var request = UnityWebRequest.Post(url, (WWWForm)null);
            await request.SendWebRequest().WithCancellation(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
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

    internal async static UniTask<int?> GetMoveNextDirection(int port, string platform, int myNumber, int[] map, int currentPosition)
    {
        var message = new GameMessage { position = myNumber, map = map, current = currentPosition };
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
            throw;
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

            await CloseRunner(port, platform);
            await UniTask.Delay(1000);
        }

        // runner execute
        var path = Path.Combine(Application.streamingAssetsPath, runnerPath);
        var runnerProcess = new Process();
        runnerProcess.StartInfo.FileName = path;
        runnerProcess.StartInfo.Arguments = port.ToString();
        runnerProcess.StartInfo.UseShellExecute = false;
        runnerProcess.StartInfo.CreateNoWindow = true;
        runnerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

        runnerProcess.Start();
        _checked.Add(platform);

        await UniTask.Delay(500);
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
