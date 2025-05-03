using UnityEngine;
using System.Collections.Generic;

public enum ProjectileOwner { Player, Enemy }

public class Projectile : MonoBehaviour
{
    [Header("Base Settings")]
    public float speed = 30f;
    public float lifetime = 1.5f;
    public int damage = 10;
    public float knockbackForce = 5f;
    
    [Header("Team Settings")]
    [Tooltip("Кому принадлежит снаряд - игроку или врагу")]
    public ProjectileOwner owner = ProjectileOwner.Player;
    
    [Header("Impact Settings")]
    [Tooltip("Эффект при попадании")]
    public GameObject impactEffectPrefab;
    [Tooltip("Количество целей, которые может пробить снаряд (0 = только одна)")]
    public int pierceCount = 0;
    [Tooltip("Слои, которые снаряд должен игнорировать")]
    public LayerMask ignoreLayers;
    
    private Rigidbody2D rb;
    private Collider2D ownerCollider;
    private int hitCount = 0;
    
    // Список объектов, которым пуля уже нанесла урон
    private HashSet<Collider2D> hitObjects = new HashSet<Collider2D>();

    public void Initialize(Collider2D owner)
    {
        ownerCollider = owner;
        
        // Игнорируем столкновения с владельцем
        if (ownerCollider != null)
        {
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, ownerCollider, true);
            }
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) 
        { 
            Debug.LogError("Rigidbody2D не найден на снаряде!");
            Destroy(gameObject); 
            return; 
        }

        // Устанавливаем начальную скорость
        rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, lifetime);
    }
    
    // Используем OnTriggerEnter2D для первого контакта
    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        HandleCollision(otherCollider);
    }
    
    // Добавляем OnTriggerStay2D для случаев, когда пуля остается внутри коллайдера
    void OnTriggerStay2D(Collider2D otherCollider)
    {
        // Если мы только входим в коллайдер, OnTriggerEnter2D уже обработает это
        // Эта функция нужна только если OnTriggerEnter2D не сработал по какой-то причине
    }
    
    void HandleCollision(Collider2D otherCollider)
    {
        // Игнорируем коллизию с владельцем снаряда
        if (otherCollider == ownerCollider) return;
        
        // Игнорируем другие снаряды
        if (otherCollider.CompareTag("Projectile")) return;
        
        // Проверяем, должны ли мы игнорировать этот слой
        if (((1 << otherCollider.gameObject.layer) & ignoreLayers.value) != 0) return;
        
        // Проверяем, наносили ли мы уже урон этому объекту
        if (hitObjects.Contains(otherCollider))
        {
            return; // Пропускаем обработку, если уже наносили урон
        }
        
        // Добавляем объект в список "пораженных"
        hitObjects.Add(otherCollider);
        
        // Расчет направления отдачи
        Vector2 knockbackDirection = CalculateKnockbackDirection(otherCollider.transform.position);
        
        bool hitTarget = false;
        
        // Проверяем, кому наносить урон в зависимости от владельца снаряда
        if (owner == ProjectileOwner.Player)
        {
            // Снаряд игрока наносит урон врагам
            EnemyHealth enemyHealth = otherCollider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log($"Нанесен урон {damage} врагу {otherCollider.name}");
                enemyHealth.TakeDamage(damage);
                hitTarget = true;
            }
        }
        else // ProjectileOwner.Enemy
        {
            // Снаряд врага наносит урон игроку
            PlayerHealth playerHealth = otherCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hitTarget = true;
            }
        }
        
        // Применяем отдачу, если у объекта есть компонент KnockbackReceiver
        KnockbackReceiver knockbackTarget = otherCollider.GetComponent<KnockbackReceiver>();
        if (knockbackTarget != null && hitTarget)
        {
            knockbackTarget.ApplyKnockback(knockbackDirection, knockbackForce);
        }
        
        // Создаем эффект попадания
        if (impactEffectPrefab != null && hitTarget)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Проверяем, должен ли снаряд быть уничтожен
        if (hitTarget)
        {
            hitCount++;
            if (hitCount > pierceCount)
            {
                // Если снаряд уничтожается, немедленно отключаем коллайдер
                Collider2D myCollider = GetComponent<Collider2D>();
                if (myCollider != null)
                {
                    myCollider.enabled = false;
                }
                
                Destroy(gameObject);
            }
        }
        else if (!otherCollider.isTrigger)
        {
            // Уничтожаем снаряд, если он столкнулся с объектом, не являющимся триггером
            // (стены, земля и т.д.) и не нанес урон
            
            // Отключаем коллайдер перед уничтожением
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }
            
            Destroy(gameObject);
        }
    }
    
    private Vector2 CalculateKnockbackDirection(Vector3 targetPosition)
    {
        // Получаем направление от снаряда к цели
        Vector2 direction = (targetPosition - transform.position);
        
        // Если направление слишком короткое, используем направление движения снаряда
        if (direction.sqrMagnitude < 0.01f)
        {
            direction = rb.linearVelocity.normalized;
            
            // Если скорость также близка к нулю, используем направление снаряда
            if (direction.sqrMagnitude < 0.01f)
            {
                direction = transform.right;
            }
        }
        else
        {
            direction.Normalize();
        }
        
        // Настраиваем вертикальную составляющую: немного вверх для естественного эффекта
        direction = new Vector2(direction.x, Mathf.Abs(direction.y) + 0.2f).normalized;
        
        return direction;
    }
}
