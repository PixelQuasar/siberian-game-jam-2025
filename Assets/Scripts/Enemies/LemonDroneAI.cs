using UnityEngine;

public class LemonDroneAI : MonoBehaviour
{
    [Header("Target and Components")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform firePoint;
    private Collider2D droneCollider;
    private float originalScaleX;

    private PatrolBehaviour patrolBehaviour;

    [Header("Movement parameters")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float desiredHeight = 3f;
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float farDistance = 10f;
    [SerializeField] private float closeDistance = 4f;
    [SerializeField] private float hoverForce = 5f;
    [SerializeField] private float dampingFactor = 0.95f;

    [Header("Attack parameters")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float shootingRange = 12f;

    private float nextFireTime = 0f;
    private Vector2 currentVelocity = Vector2.zero;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player with tag 'Player' not found!", this);
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on drone!", this);
        }

        droneCollider = GetComponent<Collider2D>();
        if (droneCollider == null) Debug.LogError("Collider2D not found on drone!", this);

        patrolBehaviour = GetComponent<PatrolBehaviour>();
        if (patrolBehaviour == null) Debug.LogError("PatrolBehaviour not found on drone!", this);

        nextFireTime = Time.time + Random.Range(0f, fireRate);
        originalScaleX = transform.localScale.x;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        bool playerDetected = false;
        if (playerTarget != null) {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= detectionRange) {
                playerDetected = true;
                HandleCombatMovement();
            }
        }

        if (!playerDetected) {
            if (patrolBehaviour != null && patrolBehaviour.enabled) {
                // Debug.Log("Drone State: Patroling");
                patrolBehaviour.UpdatePatrolMovement();
            }
            else {
                rb.linearVelocity *= dampingFactor;
            }
        }
    }

    void Update()
    {
         if (playerTarget == null) return;

         FlipTowardsPlayer();

         if (projectilePrefab != null) {
             HandleShooting();
         }
    }

    void HandleCombatMovement()
    {
        Vector2 directionToPlayer = playerTarget.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
        {
            rb.linearVelocity *= dampingFactor;
            return;
        }

        Vector2 desiredVelocity = Vector2.zero;
        Vector2 targetPosition = Vector2.zero;

        if (distanceToPlayer > farDistance)
        {
            targetPosition = (Vector2)playerTarget.position + Vector2.up * desiredHeight;
            desiredVelocity = (targetPosition - (Vector2)transform.position).normalized * moveSpeed;
            // Debug.Log("Drone State: Approaching");
        }
        else if (distanceToPlayer < closeDistance)
        {
            Vector2 awayDirection = -directionToPlayer.normalized;
            Vector2 horizontalVelocity = awayDirection * moveSpeed;
            float heightError = (playerTarget.position.y + desiredHeight) - transform.position.y;
            Vector2 verticalVelocity = Vector2.up * heightError * hoverForce;
            desiredVelocity = horizontalVelocity + verticalVelocity;
            // Debug.Log("Drone State: Retreating");
        }
        else
        {
            float midDistance = (closeDistance + farDistance) / 2f;

            Vector2 tangentDirection = new Vector2(-directionToPlayer.normalized.y, directionToPlayer.normalized.x);
            Vector2 tangentialVelocity = tangentDirection * moveSpeed * 0.75f;

            float distanceError = distanceToPlayer - midDistance;
            Vector2 radialVelocity = -directionToPlayer.normalized * distanceError * hoverForce * 0.5f; // Сила для коррекции дистанции

            float heightError = (playerTarget.position.y + desiredHeight) - transform.position.y;
            Vector2 verticalVelocity = Vector2.up * heightError * hoverForce;

            desiredVelocity = tangentialVelocity + radialVelocity + verticalVelocity;
             Debug.Log("Drone State: Strafing");
        }

        Vector2 force = (desiredVelocity - rb.linearVelocity) * hoverForce;
        rb.AddForce(force);

        rb.linearVelocity *= dampingFactor;

         if (rb.linearVelocity.magnitude > moveSpeed * 1.5f)
         {
             rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed * 1.5f;
         }
    }

     void HandleShooting()
    {
        if (Time.time >= nextFireTime)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
            if (distanceToPlayer <= shootingRange)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    void Shoot()
    {
         if (projectilePrefab == null || firePoint == null || playerTarget == null) return;

        Vector2 directionToPlayer = (playerTarget.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectileScript = projectileInstance.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(droneCollider);
        }
        else
        {
             Debug.LogError("Script Projectile not found on drone projectile prefab!", projectilePrefab);
        }

        Debug.Log("Drone fired!");
    }

    void FlipTowardsPlayer()
    {
        if (playerTarget == null) return;

        float directionToTarget = playerTarget.position.x - transform.position.x;

        if (directionToTarget > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
        }
        else if (directionToTarget < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
        }
    }
}
