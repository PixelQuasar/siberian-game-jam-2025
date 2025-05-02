using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolBehaviour : MonoBehaviour
{
    [Tooltip("Waypoints of the patrol route.")]
    public Transform[] waypoints;

    [Tooltip("Patrol speed.")]
    public float patrolSpeed = 2f;

    [Tooltip("Force applied to achieve patrol speed.")]
    public float patrolForce = 5f;

    [Tooltip("Distance at which a point is considered reached.")]
    public float waypointReachedDistance = 0.5f;

    [Tooltip("Delay at the patrol point (in seconds).")]
    public float waitAtWaypointTime = 1.0f;

    private int currentWaypointIndex = 0;
    private Rigidbody2D rb;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found for PatrolBehaviour!", this);
            enabled = false;
            return;
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning("Waypoints for PatrolBehaviour not assigned.", this);
            enabled = false;
        }
        else
        {
            for(int i = 0; i < waypoints.Length; ++i) {
                if (waypoints[i] == null) {
                     Debug.LogError($"Waypoint {i} not assigned!", this);
                     enabled = false;
                     return;
                }
            }
        }
    }

    public void UpdatePatrolMovement()
    {
        if (!enabled || waypoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            rb.linearVelocity *= 0.9f;
            return;
        }

        Transform currentWaypoint = waypoints[currentWaypointIndex];
        Vector2 directionToWaypoint = (Vector2)currentWaypoint.position - (Vector2)transform.position;
        float distanceToWaypoint = directionToWaypoint.magnitude;

        if (distanceToWaypoint < waypointReachedDistance)
        {
            Debug.Log($"Patrol: Reached waypoint {currentWaypointIndex}");
            isWaiting = true;
            waitTimer = waitAtWaypointTime;
        }
        else
        {
            Vector2 desiredVelocity = directionToWaypoint.normalized * patrolSpeed;
            Vector2 force = (desiredVelocity - rb.linearVelocity) * patrolForce;
            rb.AddForce(force);
            rb.linearVelocity *= 0.98f;
        }

        Debug.DrawLine(transform.position, currentWaypoint.position, Color.green);
    }
}