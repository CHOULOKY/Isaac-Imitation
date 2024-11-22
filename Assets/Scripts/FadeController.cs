using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    private CanvasGroup feadeGroup;

    private void Awake()
    {
        feadeGroup = GetComponent<CanvasGroup>();
    }

    public IEnumerator FadeInCoroutine(CanvasGroup uiElement = null, float duration = 1.0f)
    {
        uiElement = uiElement != null ? uiElement : feadeGroup;

        yield return FadeCoroutine(uiElement, duration, 0f, 1f);
    }

    public IEnumerator FadeOutCoroutine(CanvasGroup uiElement = null, float duration = 1.0f)
    {
        uiElement = uiElement != null ? uiElement : feadeGroup;

        yield return FadeCoroutine(uiElement, duration, 1f, 0f);
    }

    private IEnumerator FadeCoroutine(CanvasGroup uiElement, float duration, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        uiElement.alpha = startAlpha;

        while (elapsedTime < duration) {
            elapsedTime += Time.unscaledDeltaTime;
            uiElement.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }
        uiElement.alpha = endAlpha;
    }
}
