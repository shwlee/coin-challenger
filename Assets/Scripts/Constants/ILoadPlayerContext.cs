using Cysharp.Threading.Tasks;

public interface ILoadedPlayerContext
{
    UniTask Set(PlayerContext context);
}
