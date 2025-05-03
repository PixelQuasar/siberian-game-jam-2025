using UnityEngine;

public enum ResourceType { Health, Ammo, Both }

public class Consumable : MonoBehaviour
{
    [Header("Resource Settings")]
    [Tooltip("Тип восполняемого ресурса")]
    public ResourceType resourceType = ResourceType.Health;
    
    [Tooltip("Количество восстанавливаемого здоровья")]
    public int healthAmount = 20;
    [Tooltip("Количество восстанавливаемых патронов")]
    public int ammoAmount = 10;
    
    [Header("Levitation Settings")]
    [Tooltip("Амплитуда колебания")]
    public float amplitude = 0.2f;
    [Tooltip("Частота колебания")]
    public float frequency = 1.5f;
    [Tooltip("Начальная фаза колебания (случайная = -1)")]
    public float startPhase = -1f; // -1 означает случайную фазу
    
    [Header("Visual Effects")]
    [Tooltip("Включить вращение предмета")]
    public bool rotateItem = true;
    [Tooltip("Скорость вращения в градусах в секунду")]
    public float rotationSpeed = 30f;
    [Tooltip("Визуальный эффект при подборе предмета")]
    public GameObject pickupEffect;
    
    [Header("Audio")]
    [Tooltip("Звук при подборе предмета")]
    public AudioClip pickupSound;
    [Range(0, 1)]
    public float soundVolume = 0.7f;
    
    // Приватные переменные
    private Vector3 startPosition;
    private float phase;
    
    void Start()
    {
        // Сохраняем начальную позицию
        startPosition = transform.position;
        
        // Устанавливаем случайную или заданную фазу для разнообразия движения
        phase = startPhase < 0 ? Random.Range(0f, Mathf.PI * 2) : startPhase;
    }
    
    void Update()
    {
        // Левитация по синусоиде
        float yOffset = Mathf.Sin((Time.time * frequency) + phase) * amplitude;
        transform.position = startPosition + new Vector3(0, yOffset, 0);
        
        // Вращение предмета (если включено)
        if (rotateItem)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, является ли объект игроком
        if (other.CompareTag("Player"))
        {
            bool resourceGiven = false;
            
            // Восстановление здоровья
            if (resourceType == ResourceType.Health || resourceType == ResourceType.Both)
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(healthAmount);
                    resourceGiven = true;
                    Debug.Log($"Player healed for {healthAmount} HP");
                }
            }
            
            // Восстановление патронов
            if (resourceType == ResourceType.Ammo || resourceType == ResourceType.Both)
            {
                PlayerShooting playerShooting = other.GetComponent<PlayerShooting>();
                if (playerShooting != null)
                {
                    playerShooting.AddAmmo(ammoAmount);
                    resourceGiven = true;
                    Debug.Log($"Player received {ammoAmount} ammo");
                }
            }
            
            // Если хотя бы один ресурс был успешно восстановлен
            if (resourceGiven)
            {
                // Создаем эффект при подборе
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Проигрываем звук
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
                }
                
                // Уничтожаем объект
                Destroy(gameObject);
            }
        }
    }
}
