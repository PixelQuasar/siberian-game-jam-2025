using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public static SmoothCameraFollow Instance { get; private set; }

    [Tooltip("The target to follow.")]
    public Transform target;

    [Tooltip("How smooth the camera follows the target. Lower value = faster.")]
    public float smoothTime = 0.3f;

    [Tooltip("Optional offset from the target.")]
    public Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 velocity = Vector3.zero;

    [Header("Shake Settings")]
    [Tooltip("Максимальное смещение камеры")]
    public float maxShakeOffset = 1.0f;
    [Tooltip("Максимальный наклон камеры в градусах")]
    public float maxShakeRotation = 3.0f;
    [Tooltip("Скорость затухания тряски")]
    public float shakeDamping = 5.0f;
    
    private float currentShakeIntensity = 0f;
    private float currentShakeTime = 0f;
    private float shakeTimer = 0f;
    private bool isShaking = false;
    private Vector3 originalPosition;

    // Добавляем метод Awake для инициализации синглтона
    void Awake()
    {
        // Устанавливаем Instance
        if (Instance == null)
        {
            Instance = this;
            // Если нужно сохранять между сценами, раскомментируйте:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Уничтожаем дубликаты
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target not assigned in SmoothCameraFollow!", this);
        }

        if (Mathf.Approximately(offset.z, 0))
        {
            offset.z = transform.position.z;
            Debug.LogWarning("Z offset not set, using current camera Z position: " + offset.z);
        }
        
        // Сохраняем исходную позицию (можно использовать для возврата)
        originalPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        
        Vector3 targetPosition = target.position + offset;

         transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        
        if (isShaking) {
            // Обновляем таймер тряски
            shakeTimer += Time.deltaTime;
            
            // Вычисляем интенсивность на основе оставшегося времени
            float progress = shakeTimer / currentShakeTime;
            
            // Плавное затухание
            float intensity = Mathf.Lerp(currentShakeIntensity, 0f, progress * shakeDamping * Time.deltaTime);
            
            // Случайное смещение камеры
            if (intensity > 0)
            {
                // Генерируем случайный сдвиг и поворот
                Vector3 shakeOffset = Random.insideUnitSphere * intensity * maxShakeOffset;
                shakeOffset.z = 0; // Не трясем по Z для 2D
                
                transform.position += shakeOffset;
            }
            else
            {
                isShaking = false;
            }
            
            // Если истекло время, завершаем тряску
            if (progress >= 1.0f)
            {
                StopShake();
            }
        }

       
        
    }

    public void StopShake()
    {
        isShaking = false;
        currentShakeIntensity = 0f;
    }

    /// <summary>
    /// Запускает тряску камеры с указанными параметрами
    /// </summary>
    /// <param name="intensity">Интенсивность тряски (от 0 до 1)</param>
    /// <param name="duration">Продолжительность в секундах</param>
    public void Shake(float intensity, float duration)
    {
        // Проверяем параметры
        if (intensity <= 0 || duration <= 0) return;
        
        // Для отладки
        Debug.Log($"Camera shake: intensity={intensity}, duration={duration}");
        
        // Сбрасываем состояние, если требуется более сильная тряска
        if (intensity > currentShakeIntensity)
        {
            currentShakeIntensity = Mathf.Clamp01(intensity);
            currentShakeTime = duration;
            shakeTimer = 0f;
            isShaking = true;
        }
    }
}
