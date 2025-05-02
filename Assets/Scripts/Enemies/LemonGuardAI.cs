using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class LemonGuardAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D guardCollider;
    private float originalScaleX;
    private Transform playerTarget;

    [Header("Movement")]
    [Tooltip("Guard movement speed.")]
    public float moveSpeed = 2f;

    [Header("Facing Logic")]
    [Tooltip("Minimum horizontal velocity to turn the enemy.")]
    public float minimumVelocityToFlip = 0.1f;

    [Header("Burst Fire Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.15f;
    public float burstCooldown = 1.5f;
    public float attackRange = 10f;

    private int shotsFiredInBurst = 0;
    private float currentBurstCooldown = 0f;
    private float currentTimeBetweenShots = 0f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D not found on LemonGuard!", this);

        guardCollider = GetComponent<Collider2D>();
        if (guardCollider == null) Debug.LogError("Collider2D not found on LemonGuard!", this);

        originalScaleX = transform.localScale.x;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerTarget = playerObject.transform;
        else Debug.LogError("Player with tag 'Player' not found! Attack will not work.", this);

        currentBurstCooldown = Random.Range(0f, burstCooldown);
    }

    void Update()
    {
        if (currentBurstCooldown > 0) currentBurstCooldown -= Time.deltaTime;
        if (currentTimeBetweenShots > 0) currentTimeBetweenShots -= Time.deltaTime;

        FlipBasedOnVelocity();

        HandleBurstAttack();
    }

    void FixedUpdate()
    {

    }

    void FlipBasedOnVelocity()
    {
        if (rb == null) return;
        float horizontalVelocity = rb.linearVelocity.x;

        if (Mathf.Abs(horizontalVelocity) > minimumVelocityToFlip)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalVelocity) * Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
        }
    }

    void HandleBurstAttack()
    {
        if (playerTarget == null || projectilePrefab == null || firePoint == null) {
            // Debug.Log("Guard Attack Check Failed: Missing references."); // Раскомментировать для отладки
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        bool canSeePlayer = CanSeePlayer();

        if (distanceToPlayer <= attackRange && canSeePlayer && currentBurstCooldown <= 0)
        {
            if (shotsFiredInBurst == 0) 
            {
                Debug.Log("Guard: Starting new burst!");
                shotsFiredInBurst = 0;
                currentTimeBetweenShots = 0;
            }
        }
        else
        {
            if (shotsFiredInBurst > 0) {
                Debug.Log("Guard: Burst interrupted.");
                shotsFiredInBurst = shotsPerBurst;
                currentBurstCooldown = burstCooldown; 
             }
        }


        if (shotsFiredInBurst < shotsPerBurst && currentBurstCooldown <= 0 && currentTimeBetweenShots <= 0)
        {
            ShootBurst();
            shotsFiredInBurst++;
            currentTimeBetweenShots = timeBetweenShots;

            if (shotsFiredInBurst >= shotsPerBurst)
            {
                Debug.Log("Guard: Burst finished, starting cooldown.");
                currentBurstCooldown = burstCooldown;
                shotsFiredInBurst = 0; 
            }
        }
    }

    void ShootBurst()
    {
        Debug.Log($"LemonGuard Shooting Burst! Shot {shotsFiredInBurst + 1}/{shotsPerBurst}");

        Vector3 direction = Vector3.right;
        if (playerTarget.position.x < transform.position.x)
        {
            direction = Vector3.left;
        }
        firePoint.rotation = (direction == Vector3.right) ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectileScript = projectileInstance.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(guardCollider);
        }
        else
        {
            Debug.LogError("Script Projectile not found on projectile prefab!", projectilePrefab);
        }
    }

    bool CanSeePlayer() {
        if (playerTarget == null) return false;
        
        Vector2 directionToPlayer = playerTarget.position - transform.position;
        
        return true; 
    }
}

