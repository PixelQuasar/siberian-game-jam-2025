using UnityEngine;
using System.Collections;

public class SenorPomidorAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 500f;
    private float currentHealth;

    [Header("References")]
    public Transform playerTarget;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Movement")]
    public float moveSpeed = 3f;
    [SerializeField] private float idealDistanceFromPlayer = 8f;
    [SerializeField] private float tooCloseDistance = 4f;
    [SerializeField] private float tooFarDistance = 12f;
    private Vector2 movementDirection = Vector2.zero;

    [Header("Attack Settings")]
    [SerializeField] private float timeBetweenAttacks = 3f;
    private float attackCooldownTimer = 0f;

    [Header("Ground Slam Attack")]
    [SerializeField] private float groundSlamPrepareTime = 0.75f;
    [SerializeField] private float groundSlamRiseHeight = 2.0f;
    [SerializeField] private float groundSlamPauseDuration = 0.3f;
    [SerializeField] private float groundSlamRadius = 3.0f;
    [SerializeField] private float groundSlamDamage = 50f;
    [SerializeField] private Color groundSlamPrepareColor = Color.yellow;
    private Vector2 originalPosition;
    private Color originalColor;

    [Header("Charge Attack")]
    [SerializeField] private float chargePrepareTime = 0.6f;
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDuration = 1.0f;
    [SerializeField] private float chargeDamage = 75f;
    [SerializeField] private Color chargeColor = Color.red;
    private bool isCharging = false;
    private Vector2 chargeDirection;

    [Header("Projectile Volley Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int volleyProjectileCount = 5;
    [SerializeField] private float volleyDelayBetweenShots = 0.2f;
    [SerializeField] private Color volleyPrepareColor = Color.blue;
    [SerializeField] private float volleyPrepareTime = 0.5f;
    private Collider2D bossCollider;

    [Header("Physics")]
    [SerializeField] private float defaultGravityScale = 3f;
    private RigidbodyType2D originalBodyType;

    private enum BossState
    {
        Idle,
        Chasing,
        Attacking,
        Cooldown,
        Dead
    }
    private BossState currentState = BossState.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        // animator = GetComponent<Animator>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (rb != null)
        {
             rb.gravityScale = defaultGravityScale;
             rb.bodyType = RigidbodyType2D.Dynamic;
             rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            Debug.LogError("SenorPomidorAI requires a Rigidbody2D component!", this);
            enabled = false;
        }
    }

    void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogError("SenorPomidorAI: Player not found! Make sure the player has the 'Player' tag.");
            enabled = false;
        }

        currentState = BossState.Idle;
        attackCooldownTimer = timeBetweenAttacks;
    }

    void Update()
    {
        if (playerTarget == null || currentState == BossState.Dead)
        {
            if (rb != null && currentState == BossState.Dead)
            {
                 rb.linearVelocity = Vector2.zero;
            }
            movementDirection = Vector2.zero;
            return;
        }

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        movementDirection = Vector2.zero;

        switch (currentState)
        {
            case BossState.Idle:
                FacePlayer();
                if (attackCooldownTimer <= 0)
                {
                    ChooseAndStartAttack();
                }
                break;

            case BossState.Chasing:
                FacePlayer();
                movementDirection = (playerTarget.position - transform.position).normalized;
                break;

            case BossState.Attacking:
                break;

            case BossState.Cooldown:
                FacePlayer();
                if (distanceToPlayer > tooFarDistance)
                {
                    movementDirection = (playerTarget.position - transform.position).normalized;
                    Debug.Log("Pomidor State: Cooldown - Moving Towards Player");
                }
                else if (distanceToPlayer < tooCloseDistance)
                {
                    movementDirection = (transform.position - playerTarget.position).normalized;
                    Debug.Log("Pomidor State: Cooldown - Retreating from Player");
                }
                else
                {
                    movementDirection = Vector2.zero;
                    Debug.Log("Pomidor State: Cooldown - Holding Position");
                }

                if (attackCooldownTimer <= 0)
                {
                     currentState = BossState.Idle;
                }
                break;
        }
    }

    void FixedUpdate()
    {
         if (playerTarget == null || currentState == BossState.Dead || currentState == BossState.Attacking)
         {
             if (rb != null && currentState != BossState.Attacking && rb.bodyType == RigidbodyType2D.Dynamic)
              {
                   if (movementDirection == Vector2.zero)
                   {
                      rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); 
                   }
              }
             return;
         }

         if (rb != null && movementDirection != Vector2.zero && rb.bodyType == RigidbodyType2D.Dynamic)
         {
             rb.linearVelocity = new Vector2(movementDirection.x * moveSpeed, rb.linearVelocity.y);
         }
         else if (rb != null && movementDirection == Vector2.zero && rb.bodyType == RigidbodyType2D.Dynamic)
         {
             rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
         }
    }

    void ChooseAndStartAttack()
    {
        movementDirection = Vector2.zero;
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Остановить горизонтальное движение
        }

        int attackChoice = Random.Range(0, 3);

        currentState = BossState.Attacking;

        switch (attackChoice)
        {
            case 0:
                Debug.Log("Starting Attack 1: Ground Slam (Placeholder)");
                StartCoroutine(GroundSlamAttack());
                break;
            case 1:
                Debug.Log("Starting Attack 2: Charge");
                 StartCoroutine(ChargeAttack());
                break;
            case 2:
                 Debug.Log("Starting Attack 3: Projectile Volley");
                 StartCoroutine(ProjectileVolleyAttack());
                break;
        }
    }

    System.Collections.IEnumerator GroundSlamAttack()
    {
        Debug.Log("Starting Ground Slam...");
        currentState = BossState.Attacking;
        originalPosition = rb.position;

        if (rb != null)
        {
            originalBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        if (spriteRenderer != null) spriteRenderer.color = groundSlamPrepareColor;
        float timer = 0f;
        Vector2 startPos = originalPosition;
        Vector2 targetPos = originalPosition + Vector2.up * groundSlamRiseHeight;

        while (timer < groundSlamPrepareTime)
        {
            rb.position = Vector2.Lerp(startPos, targetPos, timer / groundSlamPrepareTime);
            timer += Time.deltaTime;
            yield return null;
        }
        rb.position = targetPos;

        yield return new WaitForSeconds(groundSlamPauseDuration);

        rb.position = originalPosition;
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        Collider2D[] hits = Physics2D.OverlapCircleAll(originalPosition, groundSlamRadius, playerLayer);
        foreach (Collider2D hit in hits)
        {
             Debug.Log($"Ground Slam hit: {hit.name}");
             PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
             if (playerHealth != null)
             {
                 playerHealth.TakeDamage((int)groundSlamDamage);
             }
        }

        Debug.Log("Ground Slam Finished.");
        StartCooldown();
    }

     System.Collections.IEnumerator ChargeAttack()
    {
        Debug.Log("Preparing Charge...");
        currentState = BossState.Attacking;
        isCharging = true;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; 
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
        }

        if (spriteRenderer != null) spriteRenderer.color = chargeColor;

        if (playerTarget != null)
        { 
            chargeDirection = ((Vector2)playerTarget.position - rb.position).normalized;
            FaceDirection(chargeDirection);
        }
        else
        {
            chargeDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
        
        yield return new WaitForSeconds(chargePrepareTime);

        Debug.Log("Charging!");
        if (rb != null)
        {
             rb.linearVelocity = chargeDirection * chargeSpeed;
        }

        yield return new WaitForSeconds(chargeDuration);

        Debug.Log("Charge Finished.");
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = defaultGravityScale;
        }
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        
        isCharging = false;
        StartCooldown();
    }

     System.Collections.IEnumerator ProjectileVolleyAttack()
    {
        Debug.Log("Preparing Projectile Volley...");
        currentState = BossState.Attacking;

        if (spriteRenderer != null) spriteRenderer.color = volleyPrepareColor;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(volleyPrepareTime);

        Debug.Log("Firing Volley!");
        if (projectilePrefab == null)
        {
             Debug.LogError("Projectile Prefab not assigned in SenorPomidorAI!");
             yield break;
        }
        if (firePoint == null)
        {
            Debug.LogError("Fire Point not assigned in SenorPomidorAI!");
            yield break;
        }

        if (spriteRenderer != null) spriteRenderer.color = originalColor; // Возвращаем обычный цвет перед стрельбой

        for (int i = 0; i < volleyProjectileCount; i++)
        {
            if (playerTarget == null) break;

            Vector2 directionToPlayer = (playerTarget.position - firePoint.position).normalized;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            var projScript = projectileInstance.GetComponent<Projectile>();
            if (projScript != null)
            {
                 try
                 {
                    projScript.Initialize(bossCollider);
                 }
                 catch (System.MissingMethodException)
                 {
                     Debug.LogError($"Script 'Projectile' on prefab '{projectilePrefab.name}' does not have the expected 'Initialize(Collider2D owner)' method!", projectileInstance);
                 }
                 catch (System.Exception ex)
                 {
                     Debug.LogError($"An error occurred while initializing projectile '{projectilePrefab.name}': {ex.Message}", projectileInstance);
                 }
            }
            else
            {
                Debug.LogError($"Prefab '{projectilePrefab.name}' assigned to SenorPomidorAI must have a 'Projectile' script attached!", projectileInstance);
            }

            yield return new WaitForSeconds(volleyDelayBetweenShots);
        }

        Debug.Log("Projectile Volley Finished.");
        StartCooldown();
    }


    void StartCooldown()
    {
        currentState = BossState.Cooldown;
        attackCooldownTimer = timeBetweenAttacks;

        if (rb != null)
        {
             rb.bodyType = originalBodyType;
             rb.gravityScale = defaultGravityScale;
        }

        isCharging = false; 

        Debug.Log("Entering Cooldown state.");
    }

    public void TakeDamage(float damageAmount)
    {
        if (currentState == BossState.Dead) return;

        currentHealth -= damageAmount;
        Debug.Log($"Senor Pomidor took {damageAmount} damage. Current health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Senor Pomidor is defeated!");
        currentState = BossState.Dead;
        StopAllCoroutines();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 3f);
    }

    void FacePlayer()
    {
        if (playerTarget == null || spriteRenderer == null) return;

        if (playerTarget.position.x > rb.position.x)
        {
            spriteRenderer.flipX = false;
        }
        else if (playerTarget.position.x < rb.position.x)
        {
             spriteRenderer.flipX = true;
        }
    }

    void FaceDirection(Vector2 direction)
    {
        if (spriteRenderer == null) return;

        if (direction.x > 0.01f)
        {
            spriteRenderer.flipX = false;
        }
        else if (direction.x < -0.01f)
        {
             spriteRenderer.flipX = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, groundSlamRadius);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCharging && collision.gameObject.CompareTag("Player"))
        { 
            Debug.Log("Charge hit Player!");
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)chargeDamage);
            }
        }
    }
}
