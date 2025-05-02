using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 1.5f;
    public int damage = 10;

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
        if (otherCollider == ownerCollider)
        {
            return;
        }

        Debug.Log($"Projectile hit: {otherCollider.gameObject.name}");

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
