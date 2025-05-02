using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    [Tooltip("Multiplier for knockback force (can be used to resist knockback).")]
    public float knockbackMultiplier = 1f;
    [Tooltip("Additional downward force applied when knocked back in the air.")]
    public float airKnockbackDownForce = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
         if (rb == null)
        {
             Debug.LogError("KnockbackReceiver requires a Rigidbody2D!", this);
             enabled = false;
             return;
        }
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (!enabled || rb == null) return;

        Vector2 impulse = direction.normalized * force * knockbackMultiplier;

        bool isInAir = false;
        if (playerMovement != null && playerMovement.isActiveAndEnabled)
        {
            isInAir = !playerMovement.GetIsGrounded();
        }
        else
        {
             isInAir = Mathf.Abs(rb.linearVelocity.y) > 0.1f; 
        }

        if (isInAir)
        {
            impulse += Vector2.down * airKnockbackDownForce;
        }

        Vector2 resultingVelocity = rb.linearVelocity + (impulse / rb.mass);
        
        Debug.Log($"{gameObject.name} received knockback. Calculated resulting velocity: {resultingVelocity}");

        if (playerMovement != null && playerMovement.isActiveAndEnabled)
        {
            playerMovement.SetKnockbackVelocity(resultingVelocity);
        }
        else
        {
            rb.AddForce(impulse, ForceMode2D.Impulse);
            Debug.LogWarning($"{gameObject.name} does not have PlayerMovement, applying AddForce directly.");
        }
    }
}
