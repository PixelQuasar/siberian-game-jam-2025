using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    [Tooltip("Link to the text element for displaying HP.")]
    public TextMeshProUGUI healthTextUI;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthTextUI != null)
        {
            healthTextUI.text = $"HP: {currentHealth} / {maxHealth}";
        }
        else
        {
            Debug.LogWarning("Link to Health Text UI is not set in PlayerHealth!", this);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        UpdateHealthUI();

        Debug.Log($"{gameObject.name} received {damageAmount} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthUI();
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
