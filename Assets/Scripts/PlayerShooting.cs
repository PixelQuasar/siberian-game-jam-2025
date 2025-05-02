using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Links")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float fireRate = 0.08f;

    private float nextFireTime = 0f;
    private bool isFiring = false;

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

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
