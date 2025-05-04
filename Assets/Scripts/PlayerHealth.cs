using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Добавляем для управления сценами
using System.Collections;          // Для использования корутин

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    public int currentHealth;

    [Header("Death Settings")]
    [Tooltip("Задержка перед перезагрузкой сцены после смерти")]
    public float deathDelay = 1.5f;
    [Tooltip("Визуальный эффект при смерти")]
    public GameObject deathEffectPrefab;
    [Tooltip("Звук при смерти")]
    public AudioClip deathSound;
    [Tooltip("Громкость звука смерти")]
    [Range(0, 1)]
    public float deathSoundVolume = 1f;

    void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"Player health initialized: {currentHealth}/{maxHealth}");
    }

    public int GetMaxHealth() { return maxHealth; }

    public void TakeDamage(int damageAmount)
    {
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        Debug.Log($"{gameObject.name} received {damageAmount} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }

        float intensity = Mathf.Clamp01((float)damageAmount / 20f);
        if (SmoothCameraFollow.Instance != null)
        {
            SmoothCameraFollow.Instance.Shake(intensity, 0.3f);
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"{gameObject.name} healed for {healAmount}. Current HP: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        
        // Отключаем управление игрока
        DisablePlayerControls();
        
        // Воспроизводим звук смерти
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, deathSoundVolume);
        }
        
        // Создаем эффект смерти, если он задан
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Запускаем сильную тряску камеры
        if (SmoothCameraFollow.Instance != null)
        {
            SmoothCameraFollow.Instance.Shake(0.8f, 0.5f);
        }
        
        // Перезагружаем сцену с задержкой
        StartCoroutine(ReloadSceneAfterDelay(deathDelay));
    }
    
    private void DisablePlayerControls()
    {
        // Отключаем все компоненты управления
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;
        
        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null) shooting.enabled = false;
        
        // Можно отключить коллайдеры, чтобы игрок не взаимодействовал с миром после смерти
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        // Опционально: отключаем спрайт игрока или запускаем анимацию смерти
        Animator animator = GetComponent<Animator>();
        if (animator != null && animator.parameters.Length > 0)
        {
            // Проверяем, есть ли параметр "Dead" или подобный
            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].name == "Dead" || 
                    animator.parameters[i].name == "IsDead")
                {
                    animator.SetBool(animator.parameters[i].name, true);
                    break;
                }
            }
        }
    }
    
    private IEnumerator ReloadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Перезагружаем текущую сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [ContextMenu("Test Take 10 Damage")]
    void TestTakeDamage()
    {
        TakeDamage(10);
    }
     
    [ContextMenu("Test Heal 10 HP")]
    void TestHeal()
    {
        Heal(10);
    }
}
