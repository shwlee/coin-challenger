using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultRankContext : MonoBehaviour
{
    public Image Thumbnail;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Score;

    public void Set(PlayerContext playerContext)
    {
        var playerRenderer = playerContext.PlayerObject.GetComponent<SpriteRenderer>();
        Thumbnail.sprite = playerContext.DefaultSprite;
        Thumbnail.color = playerContext.Color;
        Name.text = playerContext.Name;
        Score.text = $"({playerContext.Score})";
    }
}
