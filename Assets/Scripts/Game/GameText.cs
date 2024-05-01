using Cysharp.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameText : MonoBehaviour
{
    public GameObject MainGameObject;
    public TextMeshProUGUI StartCountDown;
    public TextMeshProUGUI RunningTime;
    public GameObject GregUp;

    private MainGameContext _gameContext;


    void Start()
    {
        _gameContext = MainGameObject.GetComponent<MainGameContext>();
        RunningTime.text = GameManager.Instance.runningTimeBeforeHurryUp.ToString();
    }

    public async void DisplayGameStart()
    {
        GameManager.Instance.GameStatus = GameStatus.Starting;

        await StartCountDownSplash();
    }

    public void DisplayGameEnd()
    {
        StartCoroutine(GameSet());
    }

    private async UniTask StartCountDownSplash()
    {
        for (var i = 3; i >= 0; i--)
        {
            if (i == 0)
            {
                StartCountDown.text = null;
                GregUp.gameObject.SetActive(true);
            }
            else
            {
                StartCountDown.text = i.ToString();
            }

            await UniTask.Delay(1000);
        }

        await UniTask.Delay(300);

        await _gameContext.InitGame();
        GameManager.Instance.GameStatus = GameStatus.Playing;
        GregUp.gameObject.SetActive(false);

        // game time 
        RunningTime.gameObject.SetActive(true);
        StartCoroutine(StartGameTime());
    }

    private IEnumerator StartGameTime()
    {
        var gameTime = (int)GameManager.Instance.runningTimeBeforeHurryUp;
        for (var i = gameTime; i >= 0; i--)
        {
            if (GameManager.Instance.GameStatus is not (GameStatus.Playing or GameStatus.HurryUp))
            {
                yield break;
            }

            RunningTime.text = i.ToString();

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator GameSet()
    {
        StartCountDown.text = "GAME OVER";
        StartCountDown.fontSize = 40;
        yield return new WaitForSeconds(5);

        // ending 전환
        SceneManager.LoadScene("EndingScene");
    }
}