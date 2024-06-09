using Cysharp.Threading.Tasks;
using UnityEngine;

public static class Escape
{
    public async static UniTask<bool> ExitIfInputEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Esc 키가 눌려있는지 확인

            var isForcePlayerHostShutdown = Input.GetKey(KeyCode.LeftShift);

            Debug.Log($"Escape game ~~~~~~~~~~~ force player host shutdown: {isForcePlayerHostShutdown}");

            await GameManager.Instance.ExitGame(isForcePlayerHostShutdown);
            return true;
        }

        return false;
    }
}
