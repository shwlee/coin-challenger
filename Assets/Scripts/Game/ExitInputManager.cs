using System;
using UnityEngine;

public class ExitInputManager : MonoBehaviour
{
    public static event Action OnExitPressed;

    public static event Action OnCleanExitPressed;

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftShift)) && Input.GetKeyDown(KeyCode.Escape))
        {
            OnCleanExitPressed?.Invoke();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnExitPressed?.Invoke();
            return;
        }
    }
}
