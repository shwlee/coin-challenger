using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;

public static class PlayerLoader
{
    private static readonly Dictionary<string, Func<int, string, UniTask<IPlayer>>> _loaderPack = new Dictionary<string, Func<int, string, UniTask<IPlayer>>>()
    {
        { ".cs", (position, path) => LoadCSharpRunner(position, path) },
        { ".js", (position,path) => LoadJsRunner(position, path) },
        { ".cpp", (position,path) => LoadCppsRunner(position, path) },
        { ".py", (position,path) => LoadPyRunner(position, path) }.
    };

    public async static UniTask<IPlayer> Load(int position, string path)
    {
        var extension = Path.GetExtension(path);
        if (_loaderPack.ContainsKey(extension) is false)
        {
            return null;
        }

        var loader = _loaderPack[extension];
        return await loader(position, path);
    }

    private static UniTask<IPlayer> LoadCSharpRunner(int position, string path)
        => GeneratePlayer<CsPlayerRunner>(position, path);

    private static UniTask<IPlayer> LoadJsRunner(int position, string path)
        => GeneratePlayer<JsPlayerRunner>(position, path);

    private static UniTask<IPlayer> LoadCppsRunner(int position, string path)
        => GeneratePlayer<CppPlayerRunner>(position, path);

    private static UniTask<IPlayer> LoadPyRunner(int position, string path)
        => GeneratePlayer<PyPlayerRunner>(position, path);

    private async static UniTask<IPlayer> GeneratePlayer<T>(int position, string path) where T : IPlayer, new()
    {
        var player = new T();
        await player.Setup(position, path);
        return player;
    }
}