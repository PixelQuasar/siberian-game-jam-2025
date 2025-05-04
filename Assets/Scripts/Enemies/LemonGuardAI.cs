using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PatrolBehaviour))]
[RequireComponent(typeof(Collider2D))]
public class LemonGuardAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D guardCollider;
    private float originalScaleX;
    private Transform playerTarget;
    private PatrolBehaviour patrolBehaviour;

    [Header("Movement")]
    [Tooltip("Speed of movement.")]
    public float moveSpeed = 2f;
    [Tooltip("Distance at which the guard starts retreating.")]
    public float retreatDistance = 3f;
    private bool shouldRetreat = false;

    [Header("Facing Logic")]
    [Tooltip("Minimum horizontal velocity to turn the enemy.")]
    public float minimumVelocityToFlip = 0.1f;

    [Header("Burst Fire Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.15f;
    public float burstCooldown = 1.5f;
    [Tooltip("Distance at which the guard starts attacking.")]
    public float attackRange = 10f;

    private int shotsFiredInBurst = 0;
    private float currentBurstCooldown = 0f;
    private float currentTimeBetweenShots = 0f;
    private bool isPlayerInAttackRange = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D not found on LemonGuard!", this);

        guardCollider = GetComponent<Collider2D>();
        if (guardCollider == null) Debug.LogError("Collider2D not found on LemonGuard!", this);

        patrolBehaviour = GetComponent<PatrolBehaviour>();
        if (patrolBehaviour == null) Debug.LogError("PatrolBehaviour not found on LemonGuard!", this);

        originalScaleX = transform.localScale.x;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerTarget = playerObject.transform;
        else Debug.LogError("Player with tag 'Player' not found!", this);

        currentBurstCooldown = Random.Range(0f, burstCooldown);

        if (retreatDistance >= attackRange) {
            Debug.LogWarning("Retreat Distance should be less than Attack Range on LemonGuard!", this);
        }
    }

    void Update()
    {
        if (playerTarget == null) {
            shouldRetreat = false;
            isPlayerInAttackRange = false;
            return;
        }

        if (currentBurstCooldown > 0) currentBurstCooldown -= Time.deltaTime;
        if (currentTimeBetweenShots > 0) currentTimeBetweenShots -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer < retreatDistance)
        {
            shouldRetreat = true;
            isPlayerInAttackRange = false;
        }
        else if (distanceToPlayer <= attackRange)
        { 
            shouldRetreat = false;
            isPlayerInAttackRange = true;
        }
        else
        {
            shouldRetreat = false;
            isPlayerInAttackRange = false;
        }

        FlipTowardsPlayer();

        if (isPlayerInAttackRange)
        {
            HandleBurstAttack();
        }
        else
        {
             if (shotsFiredInBurst > 0)
             {
                 shotsFiredInBurst = 0;
             }
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        if (shouldRetreat)
        {
            Debug.Log("Guard State: Retreating");
            if (playerTarget != null) {
                Vector2 directionAwayFromPlayer = (transform.position - playerTarget.position).normalized;
                rb.linearVelocity = new Vector2(directionAwayFromPlayer.x * moveSpeed, rb.linearVelocity.y);
            } else {
                 rb.linearVelocity *= 0.9f;
            }
        }
        else if (isPlayerInAttackRange)
        {
             Vector2 currentVelocity = rb.linearVelocity;
             currentVelocity.x *= 0.8f;
             rb.linearVelocity = currentVelocity;
        }
        else
        {
            if (patrolBehaviour != null && patrolBehaviour.enabled)
            {
                patrolBehaviour.UpdatePatrolMovement();
            }
            else
            {
                // Debug.LogWarning("PatrolBehaviour missing or disabled!");
                rb.linearVelocity *= 0.9f;
            }
        }
    }

    void FlipTowardsPlayer()
    {
        if (playerTarget == null) return;
        float directionToPlayer = playerTarget.position.x - transform.position.x;
        if (directionToPlayer > 0.01f)
            transform.localScale = new Vector3(Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
        else if (directionToPlayer < -0.01f)
            transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
    }

    void HandleBurstAttack()
    {
        if (playerTarget == null || projectilePrefab == null || firePoint == null) return;

        if (currentBurstCooldown <= 0)
        {
            if (shotsFiredInBurst == 0)
            {
                currentTimeBetweenShots = 0;
            }

            if (shotsFiredInBurst < shotsPerBurst && currentTimeBetweenShots <= 0)
            {
                ShootBurst();
                shotsFiredInBurst++;
                currentTimeBetweenShots = timeBetweenShots;

                if (shotsFiredInBurst >= shotsPerBurst)
                {
                    currentBurstCooldown = burstCooldown;
                    shotsFiredInBurst = 0;
                }
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
        return true;
    }
}
