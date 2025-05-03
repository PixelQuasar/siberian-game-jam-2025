using UnityEngine;
using System.Collections;

public class PhysicalPart : MonoBehaviour
{
    [Header("Physics Settings")]
    [Tooltip("Базовая сила выброса по X и Y")]
    public Vector2 ejectionForce = new Vector2(2f, 3f);
    [Tooltip("Случайное отклонение от базовой силы")]
    public Vector2 randomForceVariation = new Vector2(0.5f, 1f);
    [Tooltip("Сила вращения")]
    public float torqueForce = 10f;
    [Tooltip("Сторона выброса (1 = вправо, -1 = влево)")]
    public float ejectionDirection = 1f;
    [Tooltip("Угол выброса в градусах")]
    public float ejectionAngle = 30f;
    
    [Header("Lifetime")]
    [Tooltip("Время жизни в секундах")]
    public float lifetime = 3f;
    [Tooltip("Время исчезновения перед уничтожением")]
    public float fadeTime = 0.5f;
    
    [Header("Effects")]
    [Tooltip("Звуки столкновения")]
    public AudioClip[] impactSounds;
    [Tooltip("Минимальная скорость для проигрывания звука")]
    public float minImpactVelocity = 1f;
    [Tooltip("Громкость звука")]
    [Range(0, 1)]
    public float soundVolume = 0.5f;
    [Tooltip("Коэффициент затухания упругости")]
    public float bounceDamping = 0.6f;
    
    [Header("Optimization")]
    [Tooltip("Остановить физику, когда скорость падает ниже порога")]
    public float sleepThreshold = 0.05f;
    
    // Компоненты
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    
    // Внутренние переменные
    private bool isFading = false;
    private float lastImpactTime = 0f;
    private float impactCooldown = 0.1f;  // Минимальное время между звуками
    private bool isSleeping = false;
    private Vector3 originalScale;

    private float creationTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        creationTime = Time.time;
        // Устанавливаем время жизни

        
        // Применяем случайную силу с учетом угла выброса
        float angle = ejectionAngle * Mathf.Deg2Rad;
        Vector2 baseForce = new Vector2(
            ejectionForce.x * Mathf.Cos(angle),
            ejectionForce.y * Mathf.Sin(angle)
        );
        
        // Добавляем случайное отклонение
        Vector2 randomForce = new Vector2(
            baseForce.x + Random.Range(-randomForceVariation.x, randomForceVariation.x),
            baseForce.y + Random.Range(-randomForceVariation.y, randomForceVariation.y)
        );
        
        // Применяем направление выброса
        randomForce.x *= ejectionDirection;
        
        // Отражаем спрайт, если выбрасываем влево
        if (ejectionDirection < 0)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        
        // Применяем силу и вращение
        rb.AddForce(randomForce, ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-torqueForce, torqueForce), ForceMode2D.Impulse);
    }

    void Update()
    {
        float age = Time.time - creationTime;
            
        // Начать исчезновение перед уничтожением
        if (!isFading && age > lifetime - fadeTime)
        {
            StartCoroutine(FadeOut());
        }
            
        // Уничтожаем объект после окончания полного времени жизни
        if (age > lifetime && !isFading)
        {
            Destroy(gameObject);
        }


        
        // Проверка на "засыпание" объекта для оптимизации
        if (!isSleeping && rb.linearVelocity.magnitude < sleepThreshold && 
            Mathf.Abs(rb.angularVelocity) < sleepThreshold)
        {
            StartCoroutine(PutToSleep());
        }
    }

    IEnumerator PutToSleep()
    {
        // Ждем секунду, чтобы убедиться, что объект действительно остановился
        yield return new WaitForSeconds(1f);
        
        if (rb.linearVelocity.magnitude < sleepThreshold)
        {
            isSleeping = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();  // Отключаем физическую симуляцию
        }
    }

    IEnumerator FadeOut()
    {
        isFading = true;
        Color originalColor = sr.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsedTime / fadeTime);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector2(
                velocity.x * 0.5f,
                velocity.y
        );
                    
        // Уменьшаем вращение 
        rb.angularVelocity *= 0.5f;

        // Проигрываем звук столкновения
        if (collision.relativeVelocity.magnitude > minImpactVelocity && 
            Time.time - lastImpactTime > impactCooldown &&
            impactSounds != null && impactSounds.Length > 0)
        {
            lastImpactTime = Time.time;
        
            
            // Уменьшаем упругость при каждом столкновении
            if (rb.sharedMaterial != null && rb.sharedMaterial.bounciness > 0)
            {
                PhysicsMaterial2D newMaterial = new PhysicsMaterial2D();
                newMaterial.bounciness = rb.sharedMaterial.bounciness * bounceDamping;
                newMaterial.friction = rb.sharedMaterial.friction;
                rb.sharedMaterial = newMaterial;
            }
        }
    }

    // Метод для внешнего вызова (например, из системы пулинга объектов)
    public void SetEjectionDirection(float direction)
    {
        ejectionDirection = direction;
    }
}
