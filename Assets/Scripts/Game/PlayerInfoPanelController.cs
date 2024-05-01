using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInfoPanelController : MonoBehaviour
{
    public Canvas InfoPanel;

    private Coroutine _rankReorder;

    // Update is called once per frame
    void Update()
    {
        switch (GameManager.Instance.GameStatus)
        {
            case GameStatus.BeforeStart:
            case GameStatus.Starting:
                break;
            case GameStatus.Playing:
                _rankReorder ??= StartCoroutine(ReorderRankCorountine());
                break;
            case GameStatus.GameSet:
                if (_rankReorder is not null)
                {
                    StopCoroutine(_rankReorder);
                }
                break;
            default:
                break;
        }
    }

    public void ReorderRank()
    {
        var allPlayerInfos = GetOrderedAllPlayerInfo();
        RelocateSibling(allPlayerInfos);
    }

    private IEnumerator ReorderRankCorountine()
    {
        var allPlayerInfos = GetOrderedAllPlayerInfo();
        while (GameManager.Instance.GameStatus is GameStatus.Playing or GameStatus.HurryUp)
        {
            RelocateSibling(allPlayerInfos);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerable<PlayerInfoController> GetOrderedAllPlayerInfo()
    {
        var children = new List<PlayerInfoController>();
        var count = InfoPanel.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            var child = InfoPanel.transform.GetChild(i);
            children.Add(child.gameObject.GetComponent<PlayerInfoController>());
        }

        return children;
    }

    private void RelocateSibling(IEnumerable<PlayerInfoController> allPlayerInfos)
    {
        var reordered = allPlayerInfos.OrderByDescending(info => info._score);
        for (int i = allPlayerInfos.Count() - 1; i > 0; i--)
        {
            var playerInfo = reordered.ElementAt(i);
            playerInfo.transform.SetSiblingIndex(i);
        }
    }
}
