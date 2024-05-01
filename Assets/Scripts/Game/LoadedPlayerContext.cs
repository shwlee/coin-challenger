using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadedPlayerContext : MonoBehaviour
{
    public Image Thumbnail;
    public TextMeshProUGUI Name;

    public async UniTask Set(PlayerContext context)
    {
        var image = Thumbnail;
        image.color = ColorSet.GetColorBySeq(context.Position);

        Name.text = await context.Player.GetName();
    }
}
