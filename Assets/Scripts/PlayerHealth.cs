using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
[   Header("Health")]
    [SerializeField] private int maxHealth = 5;

    public int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        Debug.Log($"Player health initialized: {currentHealth}/{maxHealth}");
    }

    public int GetMaxHealth() { return maxHealth; }

    public void TakeDamage(int damageAmount)
    {
        Debug.Log("DAMAGE");
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        Debug.Log($"{gameObject.name} received {damageAmount} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }

        float intensity = Mathf.Clamp01((float)damageAmount / 20f);
        SmoothCameraFollow.Instance.Shake(intensity, 0.3f);
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        Debug.Log($"{gameObject.name} healed for {healAmount}. Current HP: {currentHealth}");
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
    }

    [ContextMenu("Test Take 10 Damage")]
    void TestTakeDamage()
    {
        TakeDamage(10);
    }
     [ContextMenu("Test Heal 10 HP")]
    void TestHeal()
    {
        Heal(10);
    }
}
