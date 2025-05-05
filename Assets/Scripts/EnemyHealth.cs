using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] public int currentHealth;
    
    [Header("Invincibility Settings")]
    [Tooltip("Время неуязвимости после получения урона (в секундах)")]
    public float invincibilityTime = 0.2f;
    [Tooltip("Мигание во время неуязвимости")]
    public bool flashDuringInvincibility = true;
    
    [Header("Death Settings")]
    [Tooltip("Префаб трупа, который будет создан при смерти")]
    public GameObject corpsePrefab;
    [Tooltip("Должен ли труп наследовать скорость врага")]
    public bool transferVelocityToCorpse = true;
    [Tooltip("Время до исчезновения объекта врага после смерти")]
    public float destroyDelay = 0.2f;
    
    [Header("Visual Feedback")]
    [Tooltip("Материал для эффекта получения урона (мигание)")]
    public Material hitFlashMaterial;
    [Tooltip("Продолжительность эффекта получения урона")]
    public float hitFlashDuration = 0.1f;
    [Tooltip("Цвет эффекта получения урона")]
    public Color hitFlashColor = Color.red;

    [Header("Effects")]
    [Tooltip("Эффект при смерти")]
    public GameObject deathEffectPrefab;
    [Tooltip("Звук при получении урона")]
    public AudioClip hitSound;
    [Tooltip("Звук при смерти")]
    public AudioClip deathSound;
    
    [Header("Item Drop")]
    [Tooltip("Шанс дропа предмета (0-1)")]
    [Range(0, 1)]
    public float dropChance = 0.5f;
    [Tooltip("Префаб предмета для дропа")]
    public GameObject itemDropPrefab;

    public string nextScene = "";
    
    // Приватные переменные
    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Rigidbody2D rb;
    private bool isDead = false;
    private bool isInvincible = false; // Флаг неуязвимости

    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damageAmount)
    {
        // Проверяем, не мертв ли враг и не находится ли в состоянии неуязвимости
        if (isDead || isInvincible) return;

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
        
        // Активируем состояние неуязвимости
        StartCoroutine(InvincibilityPeriod());
        
        // Визуальный эффект получения урона
        StartCoroutine(HitFlash());
        
        // Звуковой эффект
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }

        Debug.Log($"{gameObject.name} received {damageAmount} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Период неуязвимости после получения урона
    private IEnumerator InvincibilityPeriod()
    {
        isInvincible = true;
        
        // Мигание во время неуязвимости
        if (flashDuringInvincibility && spriteRenderer != null)
        {
            float endTime = Time.time + invincibilityTime;
            float flashInterval = 0.05f;
            
            while (Time.time < endTime)
            {
                // Мигаем, изменяя прозрачность
                spriteRenderer.color = new Color(
                    spriteRenderer.color.r,
                    spriteRenderer.color.g,
                    spriteRenderer.color.b,
                    spriteRenderer.color.a == 1f ? 0.5f : 1f
                );
                
                yield return new WaitForSeconds(flashInterval);
            }
            
            // Восстанавливаем нормальный цвет
            spriteRenderer.color = new Color(
                spriteRenderer.color.r,
                spriteRenderer.color.g, 
                spriteRenderer.color.b,
                1f
            );
        }
        else
        {
            // Просто ждем время неуязвимости
            yield return new WaitForSeconds(invincibilityTime);
        }
        
        isInvincible = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log($"{gameObject.name} died!");
        
        // Проигрываем звук смерти
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
        // Создаем эффект смерти
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        if (nextScene != "") {
            SceneManager.LoadScene(nextScene);
        }
        
        // Спавним труп
        SpawnCorpse();
        
        // Шанс дропа предмета
        TryDropItem();
        
        // Отключаем компоненты
        DisableComponents();
        
        // Уничтожаем объект с задержкой
        Destroy(gameObject, destroyDelay);
    }
    
    private void SpawnCorpse()
    {
        if (corpsePrefab != null)
        {
            // Создаем труп на месте врага
            GameObject corpse = Instantiate(corpsePrefab, transform.position, transform.rotation);
            
            // Если у трупа есть Rigidbody2D и нужно передать скорость
            if (transferVelocityToCorpse && rb != null)
            {
                Rigidbody2D corpseRb = corpse.GetComponent<Rigidbody2D>();
                if (corpseRb != null)
                {
                    corpseRb.linearVelocity = rb.linearVelocity;
                    corpseRb.angularVelocity = rb.angularVelocity;
                }
            }
            
            // Пытаемся сохранить направление спрайта
            SpriteRenderer corpseRenderer = corpse.GetComponent<SpriteRenderer>();
            if (corpseRenderer != null && spriteRenderer != null)
            {
                corpseRenderer.flipX = spriteRenderer.flipX;
                corpseRenderer.flipY = spriteRenderer.flipY;
            }
        }
    }
    
    private void TryDropItem()
    {
        if (itemDropPrefab != null && Random.value <= dropChance)
        {
            Vector3 dropPosition = transform.position;
            // Небольшое смещение, чтобы предмет не застрял в трупе
            dropPosition += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0.1f, 0.5f), 0);
            
            Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);
        }
    }
    
    private void DisableComponents()
    {
        // Отключаем все скрипты, кроме этого
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
                script.enabled = false;
        }
        
        // Отключаем коллайдеры
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
    }
    
    private IEnumerator HitFlash()
    {
        if (spriteRenderer != null && hitFlashMaterial != null)
        {
            // Меняем материал на мигающий
            spriteRenderer.material = hitFlashMaterial;
            
            // Устанавливаем цвет мигания через свойство материала
            spriteRenderer.material.SetColor("_FlashColor", hitFlashColor);
            
            // Ждем указанное время
            yield return new WaitForSeconds(hitFlashDuration);
            
            // Возвращаем исходный материал
            spriteRenderer.material = originalMaterial;
        }
        else
        {
            // Если нет материала или рендерера, просто делаем паузу
            yield return new WaitForSeconds(hitFlashDuration);
        }
    }
    
    public void Heal(int healAmount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"{gameObject.name} healed for {healAmount}. Current HP: {currentHealth}");
    }
    
    // Метод для проверки, жив ли враг
    public bool IsAlive()
    {
        return !isDead && currentHealth > 0;
    }
    
    // Метод для проверки, неуязвим ли враг
    public bool IsInvincible()
    {
        return isInvincible;
    }

    [ContextMenu("Test Take 10 Damage")]
    void TestTakeDamage()
    {
        TakeDamage(10);
    }

    [ContextMenu("Kill Enemy")]
    void TestKill()
    {
        TakeDamage(maxHealth);
    }
}
