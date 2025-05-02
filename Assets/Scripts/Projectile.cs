using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 1.5f;
    public int damage = 10;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) { Destroy(gameObject); return; }

        rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        Debug.Log($"Projectile hit: {otherCollider.gameObject.name}");

        if (!otherCollider.CompareTag("Player") && !otherCollider.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
}
