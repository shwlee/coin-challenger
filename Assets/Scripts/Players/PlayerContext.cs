using UnityEngine;

public class PlayerContext
{
    public int Position { get; set; }
    public IPlayer Player { get; set; }
    public GameObject PlayerObject { get; set; }
    public Sprite DefaultSprite { get; set; }
    public Color Color { get; set; }
    public string Name { get; set; }
    public Vector3 IntiPosition { get; set; }
    public PlayerController Controller { get; set; }
    public int Score { get; set; }
}
