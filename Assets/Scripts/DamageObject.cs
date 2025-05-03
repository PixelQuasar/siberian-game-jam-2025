using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageObject : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Количество урона при столкновении")]
    public int damageAmount = 10;
    [Tooltip("Наносить урон только один раз")]
    public bool damageOnce = true;
    [Tooltip("Задержка между повторными столкновениями")]
    public float damageCooldown = 1.0f;

    [Header("Knockback Settings")]
    [Tooltip("Сила отталкивания")]
    public float knockbackForce = 10f;
    [Tooltip("Длительность отталкивания в секундах")]
    public float knockbackDuration = 0.25f;
    [Tooltip("Отключить управление игрока во время отталкивания")]
    public bool disableControlsDuringKnockback = true;

    [Header("Visual Effects")]
    [Tooltip("Префаб эффекта при попадании")]
    public GameObject hitEffectPrefab;
    [Tooltip("Длительность эффекта")]
    public float effectDuration = 1.0f;
    [Tooltip("Проигрывать звук при попадании")]
    public AudioClip hitSound;
    [Range(0, 1)]
    public float hitSoundVolume = 0.5f;

    [Header("Projectile Settings")]
    [Tooltip("Уничтожать объект после столкновения (для пуль)")]
    public bool destroyOnImpact = false;
    [Tooltip("Задержка перед уничтожением")]
    public float destroyDelay = 0.1f;

    // Приватные переменные
    private bool canDamage = true;
    private HashSet<GameObject> damagedObjects = new HashSet<GameObject>();

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject, collision.GetContact(0).point);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        HandleCollision(collider.gameObject, collider.transform.position);
    }

    private void HandleCollision(GameObject other, Vector2 contactPoint)
    {
        // Проверяем, есть ли у объекта компонент PlayerHealth
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        
        if (playerHealth != null && canDamage)
        {
            // Проверяем, наносили ли мы уже урон этому объекту
            if (damageOnce && damagedObjects.Contains(other))
                return;

            // Наносим урон
            playerHealth.TakeDamage(damageAmount);
            
            // Отталкиваем игрока
            ApplyKnockback(other, contactPoint);
            
            // Создаем визуальные эффекты
            CreateHitEffect(contactPoint);
            
            // Отмечаем объект как получивший урон
            damagedObjects.Add(other);
            
            // Устанавливаем кулдаун
            if (damageCooldown > 0)
            {
                canDamage = false;
                StartCoroutine(ResetDamageCooldown());
            }
            
            // Уничтожаем объект если это пуля
            if (destroyOnImpact)
            {
                StartCoroutine(DestroySelf());
            }
        }
    }

    private void ApplyKnockback(GameObject target, Vector2 contactPoint)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            // Получаем базовое направление отталкивания от точки контакта
            Vector2 horizontalDirection = (target.transform.position - new Vector3(contactPoint.x, contactPoint.y, 0)).normalized;
            
            // Создаем новое направление с гарантированной вертикальной составляющей
            Vector2 knockbackDirection = new Vector2(
                horizontalDirection.x * 0.7f, // Немного снижаем силу горизонтального отталкивания
                Mathf.Max(0.5f, horizontalDirection.y) // Гарантируем минимум 0.5 по Y (вверх)
            ).normalized;
            
            // Для дополнительного контроля можно задать фиксированное вертикальное отталкивание
            float upwardKnockbackMultiplier = 1.2f; // Добавьте это поле в заголовок класса
            
            // Сбрасываем текущую скорость
            targetRb.linearVelocity = Vector2.zero;
            
            // Применяем комбинированную силу
            targetRb.AddForce(
                new Vector2(
                    knockbackDirection.x * knockbackForce,
                    knockbackDirection.y * knockbackForce * upwardKnockbackMultiplier
                ), 
                ForceMode2D.Impulse
            );
            
            Debug.Log($"Knockback applied: {knockbackDirection} with force {knockbackForce}");
            
            // Отключаем управление игрока на время отталкивания
            if (disableControlsDuringKnockback)
            {
                // Находим компонент управления игроком по имени или типу
                MonoBehaviour playerController = null;
                
                // Вариант 1: Поиск конкретного компонента по имени класса
                MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (component.GetType().Name.Contains("Controller") || 
                        component.GetType().Name.Contains("Movement"))
                    {
                        playerController = component;
                        break;
                    }
                }
                
                // Вариант 2: Альтернативно можно использовать конкретный тип, если известен
                // playerController = target.GetComponent<PlayerController>();
                
                if (playerController != null)
                {
                    StartCoroutine(DisableControlsTemporarily(playerController));
                }
            }
        }
    }
    
    private IEnumerator DisableControlsTemporarily(MonoBehaviour controller)
    {
        // Отключаем скрипт управления
        controller.enabled = false;
        
        // Ждем
        yield return new WaitForSeconds(knockbackDuration);
        
        // Включаем скрипт управления
        controller.enabled = true;
    }

    private void CreateHitEffect(Vector2 position)
    {
        // Проигрываем звук
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position, hitSoundVolume);
        }
        
        // Создаем эффект
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            
            // Уничтожаем эффект через заданное время
            Destroy(effect, effectDuration);
        }
    }

    private IEnumerator ResetDamageCooldown()
    {
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true;
    }
    
    private IEnumerator DestroySelf()
    {
        // Отключаем коллайдер и рендерер
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
        
        // Ждем заданное время
        yield return new WaitForSeconds(destroyDelay);
        
        // Уничтожаем объект
        Destroy(gameObject);
    }

    // Метод для сброса состояния (полезно для объектов из пула)
    public void Reset()
    {
        canDamage = true;
        damagedObjects.Clear();
    }
}
