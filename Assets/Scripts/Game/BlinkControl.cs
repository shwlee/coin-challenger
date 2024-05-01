using System.Collections;
using TMPro;
using UnityEngine;

public class BlinkControl : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float blinkTime = 0.5f;

    void Start()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            var currentColor = text.color;
            text.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1);
            yield return new WaitForSeconds(blinkTime);

            text.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0);
            yield return new WaitForSeconds(blinkTime);
        }
    }
}
