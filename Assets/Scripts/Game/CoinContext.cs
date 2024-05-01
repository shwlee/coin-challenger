using UnityEngine;

public class CoinContext : MonoBehaviour
{
    public CoinType type;

    public int CoinPoint => (int)type;
}
