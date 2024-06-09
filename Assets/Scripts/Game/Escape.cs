using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public static class Escape
{
    public async static UniTask<bool> ExitIfInputEscape(Action preProcess = null)
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Esc 키가 눌려있는지 확인
        {
            var isForcePlayerHostShutdown = Input.GetKey(KeyCode.LeftShift); // left shift 와 함께 esc 이면 playerHost 까지 강종.

            Debug.Log($"Escape game ~~~~~~~~~~~ force player host shutdown: {isForcePlayerHostShutdown}");

            // exit 수행 전 전처리가 필요한 경우 먼저 수행한다.
            preProcess?.Invoke();
            await GameManager.Instance.ExitGame(isForcePlayerHostShutdown);
            return true;
        }

        return false;
    }
}
