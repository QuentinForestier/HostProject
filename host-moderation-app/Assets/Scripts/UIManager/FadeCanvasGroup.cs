using System.Collections;
using UnityEngine;

public class FadeCanvasGroup : MonoBehaviour
{
    static public FadeCanvasGroup instance;

    private void Start()
    {
        instance = this;
    }

    public static void fadeCouroutine(CanvasGroup cg, float start, float end, float duration)
    {
        instance.StartCoroutine(fade(cg, start, end, duration));
    }

    private static IEnumerator fade(CanvasGroup cg, float start, float end, float duration)
    {

        float counter = 0.0f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, counter / duration);

            yield return null;
        }
    }
}
