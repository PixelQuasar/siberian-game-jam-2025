using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float fireRate = 0.08f;

    private float nextFireTime = 0f;
    private bool isFiring = false;
    private Collider2D playerCollider;

    void Start()
    {
        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.LogError("Collider2D not found on player! Projectiles may collide with it.", this);
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started) isFiring = true;
        else if (context.canceled) isFiring = false;
    }

    void Update()
    {
        if (isFiring && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Projectile projectileScript = projectileInstance.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(playerCollider);
        }
        else
        {
            Debug.LogError("Script Projectile not found on projectile prefab!", projectilePrefab);
        }
        // TODO: Sound, effects
    }
}
