using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoController : MonoBehaviour
{
    public Image Thumbnail;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Score;

    public int _score;
    private int _position;

    public void InitPlayerInfo(PlayerContext playerContext)
    {
        var playerRenderer = playerContext.PlayerObject.GetComponent<SpriteRenderer>();
        Thumbnail.sprite = playerRenderer.sprite;
        Thumbnail.color = playerRenderer.color;
        Name.text = playerContext.Name;
        _position = playerContext.Position;
    }

    // Update is called once per frame
    void Update()
    {
        var lastScore = GameManager.Instance.GetPlayerScore(_position);
        if (_score == lastScore)
        {
            return;
        }

        _score = lastScore;
        Score.text = _score.ToString();
    }
}
