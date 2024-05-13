using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoadFailedPlayerContext : MonoBehaviour, ILoadedPlayerContext
{
    public TextMeshProUGUI Name;

    public UniTask Set(PlayerContext context)
    {
        Name.text = context.FileName;
        return UniTask.CompletedTask;
    }
}
