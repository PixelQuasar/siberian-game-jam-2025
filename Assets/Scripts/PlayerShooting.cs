using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public GameObject projectilePrefab;
    public GameObject shellPrefab;
    public Transform firePoint;
    public Transform shellPoint;
    public Transform gunTransform; // Ссылка на объект оружия для эффекта отдачи
    
    [Header("Ammo System")]
    public int magazineSize = 6;             // Размер магазина
    public int totalAmmo = 30;               // Общее количество патронов
    public float reloadTime = 2f;          // Время перезарядки в секундах
    public TextMeshProUGUI ammoText;         // UI текст для отображения патронов
    
    [Header("Effects - Audio Clips")]
    public AudioClip shootSound;
    public AudioClip reloadSound;            
    public AudioClip emptyClickSound;

    [Header("Effects - Audio Volumes")]
    [Range(0f, 1f)] public float shootVolume = 0.5f;
    [Range(0f, 1f)] public float reloadVolume = 1.0f;
    [Range(0f, 1f)] public float emptyClickVolume = 0.9f;
    
    [Header("Effects - Visuals")]
    public GameObject muzzleFlashPrefab;

    [Header("Settings")]
    public float cooldownTime = 0.2f; // Время между выстрелами
    public LayerMask projectileLayer; // Слой для снарядов
    
    [Header("Recoil Settings")]
    public float recoilDistance = 0.1f; // Дистанция отдачи
    public float recoilDuration = 0.05f; // Длительность отдачи
    public float returnDuration = 0.1f; // Длительность возврата в исходное положение

    private bool canShoot = true;
    private bool isReloading = false;
    private int currentAmmo;
    private Collider2D playerCollider;
    private Vector3 gunOriginalPosition;
    private SpriteRenderer mySR;
    private AudioSource audioSource;

    public event Action<int, int> OnAmmoChanged;
    
    void Awake() 
    {
        playerCollider = GetComponent<Collider2D>();
        mySR = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
             Debug.LogError("AudioSource component not found on this GameObject!", this);
        }
    }
    
    void Start()
    {
        if (playerCollider == null)
        {
            Debug.LogError("Collider2D not found on player! Projectiles may collide with it.", this);
            playerCollider = GetComponent<Collider2D>(); // Пробуем получить снова
        }

        if (gunTransform != null)
        {
            gunOriginalPosition = gunTransform.localPosition;
        }
        else
        {
            Debug.LogWarning("Gun transform not assigned! Recoil effect will not work.", this);
        }
        
        currentAmmo = magazineSize;
        UpdateAmmoUI();
        
        // Вызываем событие для инициализации UI
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
    }
    
    public void OnReload(InputAction.CallbackContext context)
    {
        // Реагируем только на нажатие
        if (context.started)
        {
            TryReload();
        }
    }

    // 3. Метод для попытки перезарядки
    private void TryReload()
    {
        // Проверяем, можно ли перезарядиться
        if (isReloading || totalAmmo <= 0 || currentAmmo >= magazineSize)
            return;
        
        // Начинаем перезарядку
        StartCoroutine(ReloadRoutine());
    }

    // 4. Корутина перезарядки
    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        
        if (reloadSound != null && audioSource != null) 
        {
            audioSource.PlayOneShot(reloadSound, reloadVolume); 
        }
        
        // Ждем заданное время
        yield return new WaitForSeconds(reloadTime);
        
        // Вычисляем, сколько патронов нужно добавить
        int ammoToAdd = Mathf.Min(magazineSize - currentAmmo, totalAmmo);
        
        // Обновляем значения
        currentAmmo += ammoToAdd;
        totalAmmo -= ammoToAdd;
        
        // Обновляем UI
        UpdateAmmoUI();
        
        isReloading = false;
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{currentAmmo} / {totalAmmo}";
        }
        
        // Вызываем событие
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
    }
    
    // Геттеры для доступа к значениям патронов
    public int GetCurrentAmmo() { return currentAmmo; }
    public int GetTotalAmmo() { return totalAmmo; }

    public void OnFire(InputAction.CallbackContext context)
    {
        // Только при нажатии кнопки (started)
        if (context.started && canShoot && !isReloading)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                StartCoroutine(CooldownRoutine());
            }
            else
            {
                // Щелчок пустого оружия
                PlayEmptyClickSound(); // Вызываем звук пустого магазина
                // Автоматическая перезарядка при попытке выстрелить с пустым магазином
                if (totalAmmo > 0)
                {
                    TryReload();
                }
            }
        }
    }

    IEnumerator CooldownRoutine()
    {
        canShoot = false;
        yield return new WaitForSeconds(cooldownTime);
        canShoot = true;
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || currentAmmo <= 0) return;
        
        // Уменьшаем количество патронов
        currentAmmo--;
        UpdateAmmoUI();

        // Тряска камеры
        if (SmoothCameraFollow.Instance != null)
        {
            SmoothCameraFollow.Instance.Shake(Mathf.Clamp01(1f / 50f), 0.3f);
        }
        
        if (shootSound != null && audioSource != null) 
        {
            audioSource.PlayOneShot(shootSound, shootVolume); 
        }

        // Создаем снаряд
        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Явно игнорируем столкновения между снарядом и игроком
        Collider2D projectileCollider = projectileInstance.GetComponent<Collider2D>();
        if (projectileCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(projectileCollider, playerCollider, true);
            // Debug.Log("Ignoring collision between projectile and player"); // Можно убрать лишний лог
        }
        else 
        {
            Debug.LogWarning("Could not set up collision ignore - player or projectile collider missing!");
        }

        // Устанавливаем слой снаряда, если указан
        if (projectileLayer != 0)
        {
            // Находим первый установленный бит в маске слоя
            int layerNumber = 0;
            int mask = projectileLayer.value;
            while (mask > 0)
            {
                if ((mask & 1) != 0)
                    break;
                mask = mask >> 1;
                layerNumber++;
            }
            projectileInstance.layer = layerNumber;
        }

        // Инициализируем его
        Projectile projectileScript = projectileInstance.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(playerCollider);
        }
        else
        {
            Debug.LogError("Script Projectile not found on projectile prefab!", projectilePrefab);
        }

        // Эффект отдачи
        if (gunTransform != null)
        {
            StartCoroutine(RecoilEffect());
        }
        
        // Создаем вспышку выстрела
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f); // Уничтожаем через короткое время
        }

        // Выбрасываем гильзу
        if (shellPrefab != null)
        {
            GameObject shell = Instantiate(
                shellPrefab, 
                shellPoint.position, 
                shellPoint.rotation
            );
            
            // Устанавливаем направление в зависимости от направления игрока
            PhysicalPart shellScript = shell.GetComponent<PhysicalPart>();
            if (shellScript != null)
            {
                shellScript.SetEjectionDirection(mySR.flipX ? -1 : 1);
            }
        }
    }
    
    // Функция для звука пустого магазина
    private void PlayEmptyClickSound() 
    {
         if (emptyClickSound != null && audioSource != null) 
         {
              audioSource.PlayOneShot(emptyClickSound, emptyClickVolume);
              Debug.Log("Playing Empty Click Sound");
         }
    }

    IEnumerator RecoilEffect()
    {
        // Запоминаем направление, противоположное направлению выстрела
        Vector3 recoilDirection = -gunTransform.right;
        
        // Позиция с отдачей
        Vector3 recoilPosition = gunOriginalPosition + recoilDirection * recoilDistance;
        
        // Быстрое движение назад (отдача)
        float elapsedTime = 0;
        while (elapsedTime < recoilDuration)
        {
            gunTransform.localPosition = Vector3.Lerp(
                gunOriginalPosition, 
                recoilPosition, 
                elapsedTime / recoilDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Устанавливаем максимальную позицию отдачи
        gunTransform.localPosition = recoilPosition;
        
        // Плавное возвращение в исходное положение
        elapsedTime = 0;
        while (elapsedTime < returnDuration)
        {
            gunTransform.localPosition = Vector3.Lerp(
                recoilPosition, 
                gunOriginalPosition, 
                elapsedTime / returnDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Возвращаем в исходную позицию
        gunTransform.localPosition = gunOriginalPosition;
    }
    
    // Публичный метод для подбора патронов (опционально)
    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
        UpdateAmmoUI();
        Debug.Log($"Picked up {amount} ammo. Total: {totalAmmo}");
    }
}
