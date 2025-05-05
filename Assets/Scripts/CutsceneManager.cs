using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class CutsceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI cutsceneTextElement;
    public Button nextButton;

    [Header("Cutscene Content")]
    [TextArea(3, 10)]
    public string[] dialogueLines;

    [Header("Navigation")]
    public string nextSceneName;

    [Header("Text Fading")]
    [Tooltip("Duration of old text fading out")]
    public float fadeOutDuration = 0.25f;
    [Tooltip("Duration of new text appearing")]
    public float fadeInDuration = 0.25f;
    [Tooltip("Delay before input is accepted after scene starts")]
    public float inputDelay = 1f;

    private int currentLine = 0;
    private bool cutsceneFinished = false;
    private bool isTransitioning = false;
    private bool canAcceptInput = false;

    void Start()
    {
        if (cutsceneTextElement == null) { /* ... error ... */ return; }
        if (string.IsNullOrEmpty(nextSceneName)) { /* ... error ... */}
        if (dialogueLines == null || dialogueLines.Length == 0) { /* ... exit ... */ return; }

        cutsceneFinished = false;
        isTransitioning = false;
        canAcceptInput = false;
        currentLine = 0;

        cutsceneTextElement.text = dialogueLines[currentLine];
        cutsceneTextElement.canvasRenderer.SetAlpha(1.0f);

        StartCoroutine(EnableInputAfterDelay(inputDelay));
    }

    IEnumerator EnableInputAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        canAcceptInput = true;
        Debug.Log("Cutscene input enabled.");
    }

    void Update()
    {
        if (canAcceptInput && !isTransitioning && !cutsceneFinished)
        {
            bool nextPressed = (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                              || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (nextPressed)
            {
                ShowNextLine();
            }
        }
    }

    public void ShowNextLine()
    {
        if (!canAcceptInput || isTransitioning || cutsceneFinished) return;

        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            StartCoroutine(FadeAndChangeTextCoroutine());
        }
        else
        {
            cutsceneFinished = true;
            Debug.Log("Cutscene finished.");
            LoadNextScene();
        }
    }

    IEnumerator FadeAndChangeTextCoroutine()
    {
        isTransitioning = true;

        cutsceneTextElement.CrossFadeAlpha(0f, fadeOutDuration, true);
        yield return new WaitForSecondsRealtime(fadeOutDuration);

        cutsceneTextElement.text = dialogueLines[currentLine];
        Debug.Log($"Showing line {currentLine}: {dialogueLines[currentLine]}");

        cutsceneTextElement.CrossFadeAlpha(0f, 0f, true);
        cutsceneTextElement.CrossFadeAlpha(1f, fadeInDuration, true);
        yield return new WaitForSecondsRealtime(fadeInDuration);

        isTransitioning = false;
    }


    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else {
            Debug.LogError("Next scene name not specified!");
        }
        enabled = false;
    }
}
