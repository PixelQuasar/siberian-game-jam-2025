using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("UI Image for fade effect (must be black and fully opaque)")]
    public Image fadeImage;
    [Tooltip("Fade duration in seconds")]
    public float fadeDuration = 0.5f;

    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 1f);
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeInSequence());
        }
        else
        {
            Debug.LogError("Fade Image not assigned in SceneFadeIn!", this.gameObject);
        }
    }

    IEnumerator FadeInSequence()
    {
        Debug.Log("Starting screen fade in...");
        yield return StartCoroutine(FadeScreen(false));
        Debug.Log("Screen faded in.");

        if (fadeImage != null)
        {
            // fadeImage.gameObject.SetActive(false);
        }
    }

    IEnumerator FadeScreen(bool fadeOut)
    {
        if (fadeImage == null) yield break;

        float targetAlpha = fadeOut ? 1.0f : 0.0f;
        float startAlpha = fadeImage.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, newAlpha);
            yield return null;
        }
        fadeImage.color = new Color(0f, 0f, 0f, targetAlpha);
    }
}
