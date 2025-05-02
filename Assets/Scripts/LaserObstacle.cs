using UnityEngine;

public class LaserObstacle : MonoBehaviour {
    public enum LaserState {
        Inactive,       // Невидимый, безопасный
        Activating,     // Силуэт, безопасный
        Active          // Лазер, наносит урон
    }
    [Header("Delays")]
    public float delayStart = 0f;
    public float delayInactive = 2f;       // Сколько времени лазер "спит"
    public float delayActivating = 1f;     // Сколько времени "силуэт" отображается
    public float delayActive = 2f;         // Сколько времени лазер активен

    [Header("Sprites")]
    public Sprite spriteInactive;    // Можно оставить пустым, если хотим просто скрывать рендер
    public Sprite spriteActivating;  
    public Sprite spriteActive;      

    [Header("Damage Settings")]
    public int damageToPlayer = 1;   // Урон, наносимый игроку  
    public bool damageOnStay = true; // Наносить урон в OnTriggerStay2D?
    public float damageInterval = 0.5f; // Как часто наносить урон при "зависании" в лазере

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private LaserState currentState = LaserState.Inactive;

    private float stateTimer = 0f;         // Отсчитывает время внутри текущего состояния
    private float damageTimer = 0f;        // Отсчитывает время между уронами (при stay)

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Если компоненты лежат на дочерних объектах, нужно сделать GetComponentInChildren и т. п.
        // Переключаемся в неактивное состояние в самом начале
        SetState(LaserState.Inactive);
        stateTimer = -delayStart;
    }

    void Update() {
        // Считаем таймер текущего состояния
        stateTimer += Time.deltaTime;
        
        switch (currentState)
        {
            case LaserState.Inactive:
                if (stateTimer >= delayInactive)
                {
                    // Переход к Activating
                    SetState(LaserState.Activating);
                }
                break;

            case LaserState.Activating:
                if (stateTimer >= delayActivating)
                {
                    // Переход к Active
                    SetState(LaserState.Active);
                }
                break;

            case LaserState.Active:
                if (stateTimer >= delayActive)
                {
                    // Снова переходим в Inactive
                    SetState(LaserState.Inactive);
                }
                break;
        }
    }

    void SetState(LaserState newState) {
        currentState = newState;
        stateTimer = 0f; // Сброс таймера

        switch (newState) {
            case LaserState.Inactive:
                // Скрываем спрайт или ставим spriteInactive
                if (spriteInactive == null)
                    spriteRenderer.enabled = false;
                else
                {
                    spriteRenderer.enabled = true;
                    spriteRenderer.sprite = spriteInactive;
                }

                // Выключаем коллайдер, чтобы не наносил урон
                if (boxCollider2D != null)
                    boxCollider2D.enabled = false;
                break;

            case LaserState.Activating:
                // Ставим силуэт (более блеклый спрайт)
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = spriteActivating;

                // Тоже безопасное состояние, коллайдер выключен
                if (boxCollider2D != null)
                    boxCollider2D.enabled = false;
                break;

            case LaserState.Active:
                // Включаем основной спрайт лазера
                spriteRenderer.enabled = true;
                spriteRenderer.sprite = spriteActive;

                // Включаем коллайдер, чтобы наносил урон
                if (boxCollider2D != null)
                    boxCollider2D.enabled = true;
                break;
        }
    }

    // Если ваш игрок имеет коллайдер и Rigidbody2D (trigger/collider),
    // можно использовать OnTriggerEnter2D / OnTriggerStay2D / OnTriggerExit2D
    // для нанесения урона.

    void OnTriggerStay2D(Collider2D other) {
        if (currentState != LaserState.Active) return; // Наносим урон только в Active

        // Пример логики: если это игрок (проверка по тэгу)
        if (other.CompareTag("Player"))
        {
            if (damageOnStay)
            {
                // Можно наносить урон раз в damageInterval секунд
                damageTimer += Time.deltaTime;
                if (damageTimer >= damageInterval)
                {
                    damageTimer = 0f;
                    DealDamageToPlayer(other.gameObject);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (currentState != LaserState.Active) return;

        if (other.CompareTag("Player"))
        {
            // Если хотим моментальный урон при входе
            if (!damageOnStay) 
            {
                DealDamageToPlayer(other.gameObject);
            }

            // Сбросим таймер, чтобы считать с нуля, если дальше будем оставаться в триггере
            damageTimer = 0f;
        }
    }

    private void DealDamageToPlayer(GameObject player) {
        // Здесь вы вызываете логику у игрока (например, player.GetComponent<Health>().TakeDamage(damageToPlayer); )
        Debug.Log("Player получил урон от лазера: " + damageToPlayer);
    }
}
