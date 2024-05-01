using Cysharp.Threading.Tasks;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntryLoader : MonoBehaviour
{
    public Canvas LoadedPlayerPanel;
    public TextMeshProUGUI InfoText;
    public TextMeshProUGUI PrepareText;
    public GameObject LoadedPlayerPrefab;

    private bool _waitForGameModeSet = true;
    private bool _isReadyToStart;

    // Update is called once per frame
    async void Update()
    {
        if (SceneManager.GetActiveScene().name != "Entry")
        {
            return;
        }

        if (_waitForGameModeSet)
        {
            _waitForGameModeSet = false;
            PrepareText.gameObject.SetActive(true);

            GameMode mode;
            if (Input.GetKeyDown(KeyCode.Alpha1))  // single mode index 0
            {
                mode = GameMode.Single1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))  // single mode index 1
            {
                mode = GameMode.Single2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))  // single mode index 2
            {
                mode = GameMode.Single3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))  // single mode index 3
            {
                mode = GameMode.Single4;
            }
            else if (Input.GetKeyDown(KeyCode.F10))  // test mode
            {
                mode = GameMode.Test;
            }
            else if (Input.GetKeyDown(KeyCode.Return))  // contest mode
            {
                mode = GameMode.Contest;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitGame();
                return;
            }
            else
            {
                _waitForGameModeSet = true;
                return;
            }

            PrepareText.text = "LOADING..";

            GameManager.Instance.SetGameMode(mode);
            await LoadPlayer();
            return;
        }

        if (_isReadyToStart)
        {
            if (InfoText.gameObject.activeSelf is false) // key press display.
            {
                InfoText.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.Return)) // game start.
            {
                SceneManager.LoadScene("Game");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitGame();
            }
        }
    }

    private async UniTask LoadPlayer()
    {
        _waitForGameModeSet = false;

        // check folder
        if (Directory.Exists(Consts.PlayerModulePath) is false)
        {
            Directory.CreateDirectory(Consts.PlayerModulePath);
        }

        var loadedPlayers = await GameManager.Instance.LoadPlayers();

        PrepareText.gameObject.SetActive(false);

        foreach (var player in loadedPlayers)
        {
            await UniTask.Delay(500);

            var loadedPlayer = Instantiate(LoadedPlayerPrefab);
            var context = loadedPlayer.GetComponent<LoadedPlayerContext>();
            await context.Set(player);

            loadedPlayer.transform.SetParent(LoadedPlayerPanel.transform);
        }

        _isReadyToStart = true;
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
}
