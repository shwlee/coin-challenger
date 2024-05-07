using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class MainGameContext : MonoBehaviour
{
    public GameObject InformText;
    public Canvas PlayerInfoPanel;
    public GameObject PlayerInfoPrefab;

    private GameText _gameText;
    private PlayerInfoPanelController _playerInfoPanelController;
    private bool _isGameSet;

    // Start is called before the first frame update
    void Start()
    {
        _gameText = InformText.GetComponent<GameText>();
        _playerInfoPanelController = PlayerInfoPanel.GetComponent<PlayerInfoPanelController>();
    }

    public async UniTask InitGame()
    {
        GameManager.Instance.InitMap();
        await GameManager.Instance.InitPlayers();
        InitPlayerInfoPanel();
    }

    private void InitPlayerInfoPanel()
    {
        var playerContexts = GameManager.Instance.GetAllPlayerContexts();
        foreach (var playerContext in playerContexts)
        {
            var playerInfo = Instantiate(PlayerInfoPrefab);
            var infoController = playerInfo.GetComponent<InGamePlayerInfoController>();
            infoController.InitPlayerInfo(playerContext);

            playerInfo.transform.SetParent(PlayerInfoPanel.transform, false);
        }
    }

    void Update()
    {
        if (_isGameSet)
        {
            return;
        }

        switch (GameManager.Instance.GameStatus)
        {
            case GameStatus.BeforeStart:
                InformText.SetActive(true);
                _gameText.DisplayGameStart();
                break;
            case GameStatus.Starting:
            case GameStatus.Playing:
                break;
            case GameStatus.GameSet:
                ExecuteEndingProcedure();
                break;
        }
    }

    private void ExecuteEndingProcedure()
    {
        InformText.SetActive(true);
        _gameText.DisplayGameEnd();
        _isGameSet = true;

        StartCoroutine(ReorderRanking());
    }

    private IEnumerator ReorderRanking()
    {
        yield return new WaitForSeconds(1f);
        _playerInfoPanelController.ReorderRank();
    }
}
