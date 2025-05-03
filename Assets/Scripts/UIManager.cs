using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    [Tooltip("Ссылка на игрока. Если не указана, будет найдена автоматически по тегу Player")]
    public GameObject player;

    [Header("Health UI")]
    [Tooltip("Префаб иконки здоровья (UI Image)")]
    public GameObject healthIconPrefab;
    [Tooltip("Панель, где будут отображаться иконки здоровья")]
    public Transform healthPanel;
    [Tooltip("Ограничение на максимальное количество отображаемых иконок здоровья")]
    public int maxDisplayedHealth = 10;

    [Header("Ammo UI")]
    [Tooltip("Иконка патрона (опционально)")]
    public Image ammoIcon;
    [Tooltip("Текст для отображения патронов")]
    public TextMeshProUGUI ammoText;
    
    // Приватные переменные
    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;
    private List<Image> healthIcons = new List<Image>();
    private int lastHealthValue = -1;
    private int lastAmmoValue = -1;
    private int lastTotalAmmoValue = -1;

    void Awake()
    {
        // Находим игрока если не указан
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Игрок не найден! Добавьте тег Player вашему персонажу или укажите вручную в инспекторе.");
                return;
            }
        }

        // Получаем необходимые компоненты
        playerHealth = player.GetComponent<PlayerHealth>();
        playerShooting = player.GetComponent<PlayerShooting>();

        if (playerHealth == null)
            Debug.LogError("Компонент PlayerHealth не найден на игроке!");

        if (playerShooting == null)
            Debug.LogError("Компонент PlayerShooting не найден на игроке!");
    }

    void Start()
    {
        // Сразу обновляем UI при старте
        UpdateHealthUI(true);
        UpdateAmmoUI(true);
    }

    void Update()
    {
        // Проверяем обновления каждый кадр
        // Обновляем только при изменении значений для оптимизации
        UpdateHealthUI(false);
        UpdateAmmoUI(false);
    }

    /// <summary>
    /// Обновляет UI здоровья, создавая или удаляя иконки при необходимости
    /// </summary>
    /// <param name="forceUpdate">Принудительное обновление, даже если значение не изменилось</param>
    private void UpdateHealthUI(bool forceUpdate)
    {
        if (playerHealth == null || healthPanel == null || healthIconPrefab == null)
            return;

        // Получаем текущее здоровье напрямую через публичное поле
        int currentHealth = playerHealth.currentHealth;
        
        // Ограничиваем количество отображаемых иконок
        int displayHealth = Mathf.Min(currentHealth, maxDisplayedHealth);

        // Обновляем UI только при изменении здоровья или принудительном обновлении
        if (forceUpdate || displayHealth != lastHealthValue)
        {
            lastHealthValue = displayHealth;

            // Сначала удалим все существующие иконки для чистого старта
            foreach (Transform child in healthPanel)
            {
                Destroy(child.gameObject);
            }
            healthIcons.Clear();

            // Создаем иконки заново
            for (int i = 0; i < displayHealth; i++)
            {
                GameObject newIcon = Instantiate(healthIconPrefab, healthPanel);
                
                // Сбрасываем все привязки и позиции, чтобы Layout Group мог работать
                RectTransform rectTransform = newIcon.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Сбрасываем позицию
                    rectTransform.localPosition = Vector3.zero;
                    
                    // Устанавливаем якоря и пивот
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    
                    // Сбрасываем смещения
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                Image iconImage = newIcon.GetComponent<Image>();
                Debug.Log($"CURRENT: {currentHealth}");
                if (i <= currentHealth) {
                    iconImage.enabled = true;
                } else {
                    iconImage.enabled = false;
                }
               
                if (iconImage != null)
                {
                    healthIcons.Add(iconImage);
                }

            }
        }
    }


    /// <summary>
    /// Обновляет UI патронов, отображая текущее и общее количество
    /// </summary>
    /// <param name="forceUpdate">Принудительное обновление, даже если значение не изменилось</param>
    private void UpdateAmmoUI(bool forceUpdate)
    {
        if (playerShooting == null || ammoText == null)
            return;

        // Получаем информацию о патронах через рефлексию
        int currentAmmo = GetFieldValue<int>(playerShooting, "currentAmmo");
        int totalAmmo = GetFieldValue<int>(playerShooting, "totalAmmo");

        // Обновляем только при изменении или принудительном обновлении
        if (forceUpdate || currentAmmo != lastAmmoValue || totalAmmo != lastTotalAmmoValue)
        {
            lastAmmoValue = currentAmmo;
            lastTotalAmmoValue = totalAmmo;

            // Обновляем текст
            ammoText.text = $"{currentAmmo} / {totalAmmo}";
        }
    }

    /// <summary>
    /// Получает значение поля через рефлексию
    /// </summary>
    private T GetFieldValue<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.Instance);
            
        if (field != null)
        {
            return (T)field.GetValue(obj);
        }
        
        return default(T);
    }
}
