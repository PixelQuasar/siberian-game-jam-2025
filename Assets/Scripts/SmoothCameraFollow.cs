using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public static SmoothCameraFollow Instance { get; private set; }

    [Header("Target & Following")]
    [Tooltip("The target to follow.")]
    public Transform target;

    [Tooltip("How smooth the camera follows the target. Lower value = faster.")]
    public float smoothTime = 0.3f;

    [Tooltip("Base vertical offset from the target.")]
    public float verticalOffset = 0f;

    [Tooltip("Base distance from the target (Z offset). Should be negative for standard 2D.")]
    public float distance = -10f;

    [Header("Dynamic Horizontal Offset")]
    [Tooltip("How far the camera should shift horizontally based on facing direction.")]
    public float horizontalOffsetMagnitude = 3.0f;

    [Tooltip("How quickly the horizontal offset changes.")]
    public float offsetChangeSpeed = 5.0f;

    [Header("Shake Settings")]
    [Tooltip("Максимальное смещение камеры при тряске")]
    public float maxShakeOffset = 0.5f;

    [Tooltip("Скорость затухания тряски")]
    public float shakeDamping = 5.0f;

    private Vector3 velocity = Vector3.zero;
    private SpriteRenderer targetSpriteRenderer;
    private float currentHorizontalOffset = 0f;

    private float currentShakeIntensity = 0f;
    private float currentShakeTime = 0f;
    private float shakeTimer = 0f;
    private bool isShaking = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target not assigned in SmoothCameraFollow!", this);
            enabled = false;
            return;
        }

        targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        if (targetSpriteRenderer == null) {
            targetSpriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        }
        if (targetSpriteRenderer == null) {
             Debug.LogWarning("Could not find SpriteRenderer on the target or its children. Dynamic horizontal offset will not work.", this);
        }

        UpdateCurrentOffset(true);
    }

    void LateUpdate()
    {
        if (target == null) return;

        UpdateCurrentOffset(false);

        Vector3 currentBaseOffsetVector = new Vector3(currentHorizontalOffset, verticalOffset, distance);

        Vector3 targetPosition = target.position + currentBaseOffsetVector;

        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        if (isShaking)
        {
            UpdateShake();

            if (currentShakeIntensity > 0.01f)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * currentShakeIntensity * maxShakeOffset;
                shakeOffset.z = 0;

                smoothedPosition += shakeOffset;
            }
            else
            {
                isShaking = false;
            }
        }

        transform.position = smoothedPosition;
    }

    void UpdateCurrentOffset(bool instant)
    {
        float targetHorizontalOffset = 0f;
        if (targetSpriteRenderer != null)
        {
            targetHorizontalOffset = targetSpriteRenderer.flipX ? horizontalOffsetMagnitude : -horizontalOffsetMagnitude;
        }

        if (instant) {
            currentHorizontalOffset = targetHorizontalOffset;
        } else {
            currentHorizontalOffset = Mathf.Lerp(currentHorizontalOffset, targetHorizontalOffset, Time.deltaTime * offsetChangeSpeed);
        }
    }

    void UpdateShake()
    {
        shakeTimer += Time.deltaTime;

        if (shakeTimer >= currentShakeTime)
        {
            isShaking = false;
            currentShakeIntensity = 0f;
            return;
        }

        currentShakeIntensity = Mathf.Lerp(currentShakeIntensity, 0f, shakeDamping * Time.deltaTime);
    }

    public void StopShake()
    {
        isShaking = false;
        currentShakeIntensity = 0f;
    }

    public void Shake(float intensity, float duration)
    {
        if (intensity <= 0 || duration <= 0) return;
        Debug.Log($"Camera shake requested: intensity={intensity}, duration={duration}");

        if (intensity >= currentShakeIntensity || !isShaking)
        {
             isShaking = true;
             currentShakeIntensity = Mathf.Clamp01(intensity);
             currentShakeTime = duration;
             shakeTimer = 0f;
        }
    }
}
