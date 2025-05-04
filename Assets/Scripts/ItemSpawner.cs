using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    // Префаб подбираемого предмета
    public GameObject itemPrefab;
    
    // Интервал между спаунами в секундах
    public float spawnInterval = 10f;
    
    // Приватные переменные
    private float timeUntilNextSpawn;
    private GameObject currentItem;
    
    void Start()
    {
        timeUntilNextSpawn = spawnInterval;
    }
    
    void Update()
    {
        // Если предмет уже есть на сцене, ничего не делаем
        if (currentItem != null)
            return;
            
        // Уменьшаем счетчик времени
        timeUntilNextSpawn -= Time.deltaTime;
        
        // Когда счетчик достигает нуля, создаем предмет
        if (timeUntilNextSpawn <= 0)
        {
            SpawnItem();
            timeUntilNextSpawn = spawnInterval;
        }
    }
    
    void SpawnItem()
    {
        // Создаем предмет на позиции спаунера
        currentItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
    }
    
    // Вызывайте этот метод, когда предмет подбирается
    public void ItemCollected()
    {
        currentItem = null;
    }
}
