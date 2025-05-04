using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class ElevatorScript : MonoBehaviour
{
    [Header("Scene Loading")]
    [Tooltip("Exact scene name to load (from Build Settings)")]
    public string sceneNameToLoad;

    [Header("Interaction")]
    [Tooltip("Key to activate elevator (from the NEW Input System Key enum)")]
    public Key interactionKey = Key.E;
    // public GameObject interactionPrompt; // Опционально: подсказка

    [Header("Visuals & Fade")]
    [Tooltip("Open elevator sprite")]
    public Sprite openSprite;
    [Tooltip("Closed elevator sprite")]
    public Sprite closedSprite;
    [Tooltip("UI Image for fade effect")]
    public Image fadeImage;
    [Tooltip("Fade duration in seconds")]
    public float fadeDuration = 0.5f;

    private SpriteRenderer spriteRenderer;
    private bool playerIsInside = false;
    private bool isTransitioning = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
             Debug.LogError("Elevator sprite renderer not found on object!", this.gameObject);
        }

        // Проверка Fade Image
        if (fadeImage == null) {
            Debug.LogError("Fade Image not assigned!", this.gameObject);
        } else {
            // Убедимся, что экран изначально прозрачный
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.gameObject.SetActive(true); // Убедимся, что сам объект Image активен
        }
    }

    void Start()
    {
        if (string.IsNullOrEmpty(sceneNameToLoad)) {
            Debug.LogError("Scene name to load not specified!", this.gameObject);
        }
        // Ставим открытый спрайт по умолчанию
        if (spriteRenderer != null && openSprite != null) {
            spriteRenderer.sprite = openSprite;
        }
        // if (interactionPrompt != null) interactionPrompt.SetActive(false);
        isTransitioning = false;
    }

    void Update()
    {
        if (playerIsInside && Keyboard.current != null && Keyboard.current[interactionKey].wasPressedThisFrame && !isTransitioning)
        {
            StartCoroutine(TransitionSequence());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            playerIsInside = true;
            Debug.Log("Player entered elevator.");
            // if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            playerIsInside = false;
            Debug.Log("Player exited elevator.");
            // if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }

    IEnumerator TransitionSequence()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;
        Debug.Log("Starting transition...");

        if (spriteRenderer != null && closedSprite != null) {
            spriteRenderer.sprite = closedSprite;
            Debug.Log("Elevator closed.");
        } else {
             Debug.LogWarning("Failed to change elevator sprite (closedSprite not assigned?)");
        }

        if (fadeImage != null) {
            yield return StartCoroutine(FadeScreen(true));
            Debug.Log("Screen faded out.");
        } else {
             Debug.LogWarning("Fade Image not assigned, skipping fade.");
             yield return new WaitForSeconds(0.5f);
        }


        Debug.Log($"Loading scene: {sceneNameToLoad}");
        SceneManager.LoadScene(sceneNameToLoad);

        // --- ВАЖНО: Логика осветления экрана ---
        // Осветление должно происходить уже В НОВОЙ сцене.
        // Обычно для этого в новой сцене есть похожий Canvas с FadeImage
        // и скрипт, который при запуске сцены вызывает FadeScreen(false).
        // Либо можно сделать FadeCanvas неразрушаемым (DontDestroyOnLoad)
        // и добавить логику осветления в этот же скрипт при загрузке новой сцены.
        // --- ---

        // Код ниже для асинхронной загрузки, если понадобится:
        /*
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNameToLoad);
        asyncLoad.allowSceneActivation = false; // Не активировать сразу

        // Ждем, пока сцена не загрузится почти полностью (0.9)
        while (asyncLoad.progress < 0.9f)
        {
            // Здесь можно показывать прогресс загрузки, если нужно
            yield return null;
        }

        // Теперь можно активировать новую сцену
        Debug.Log("Scene loaded, activating...");
        asyncLoad.allowSceneActivation = true;

        // После активации нужно будет осветлить экран в новой сцене
        */
    }

    IEnumerator FadeScreen(bool fadeOut)
    {
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
