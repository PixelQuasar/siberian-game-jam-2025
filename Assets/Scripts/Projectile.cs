using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 1.5f;
    public int damage = 10;
    public float knockbackForce = 5f;

    private Rigidbody2D rb;
    private Collider2D ownerCollider;

    public void Initialize(Collider2D owner)
    {
        ownerCollider = owner;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) { Destroy(gameObject); return; }

        rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider == ownerCollider) return;

        Debug.Log($"Projectile hit: {otherCollider.gameObject.name}");

        KnockbackReceiver knockbackTarget = otherCollider.GetComponent<KnockbackReceiver>();
        if (knockbackTarget != null)
        {
            Vector2 knockbackDirection = (otherCollider.transform.position - transform.position);
            knockbackDirection = new Vector2(knockbackDirection.x, 0).normalized;
            if (knockbackDirection.sqrMagnitude < 0.01f) 
            {
                 knockbackDirection = new Vector2(rb.linearVelocity.normalized.x, 0).normalized;
                 if (knockbackDirection.sqrMagnitude < 0.01f) knockbackDirection = Vector2.right;
            }

            knockbackTarget.ApplyKnockback(knockbackDirection, knockbackForce);
        }

        PlayerHealth playerHealth = otherCollider.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        if (!otherCollider.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
}
