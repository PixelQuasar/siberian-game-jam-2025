using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    
    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public float extraGroundCheckDistance = 0.05f;
    public float coyoteTime = 0.1f;
    
    [Header("Wall Detection Settings")]
    [Tooltip("Проверка столкновения со стенами для предотвращения 'зависания'")]
    public bool preventWallSticking = true;
    [Tooltip("Расстояние проверки стены")]
    public float wallCheckDistance = 0.2f;
    [Tooltip("Слои, которые считаются стенами")]
    public LayerMask wallLayer;
    [Tooltip("Множитель скорости падения при скольжении по стене")]
    public float wallSlidingGravityMultiplier = 2f;
    
    [Header("References")]
    public GameObject gun;
    public GameObject head;

    // --- Флаг и переменная для отбрасывания ---
    private bool applyKnockbackVelocity = false;
    private Vector2 knockbackVelocityToApply = Vector2.zero;
    // -----------------------------------------

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private bool isGrounded;
    private float lastGroundedTime;
    private bool isTouchingWallLeft = false;
    private bool isTouchingWallRight = false;
    private float originalGravityScale;

    private Vector2 aimWorldPosition;

    private Animator animator;

    private SpriteRenderer mySR;
    private SpriteRenderer gunSR;
    private SpriteRenderer headSR;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D not found!");
        if (groundCheck == null) Debug.LogError("GroundCheck not assigned!");

        animator = GetComponent<Animator>();

        mySR = GetComponent<SpriteRenderer>();
        gunSR = gun.GetComponent<SpriteRenderer>();
        headSR = head.GetComponent<SpriteRenderer>();
        
        // Сохраняем исходное значение гравитации
        originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        CheckIfGrounded();
        CheckForWalls();
        FlipTowardsAim();
        SetAnimatorSpeedValue();
    }

    public void SetAimPosition(Vector2 worldPos)
    {
        aimWorldPosition = worldPos;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveDirection = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        bool canJump = isGrounded || (Time.time - lastGroundedTime <= coyoteTime);
        
        if (context.performed && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        if (applyKnockbackVelocity)
        {
            rb.linearVelocity = knockbackVelocityToApply;
            applyKnockbackVelocity = false;
        }
        else
        {
            // Предотвращаем "зависание" на стенах
            HandleWallSliding();
            
            // Применяем горизонтальное движение с учетом стен
            float horizontalVelocity = moveDirection.x * moveSpeed;
            
            // Если включено предотвращение прилипания к стенам и игрок пытается двигаться в стену
            if (preventWallSticking)
            {
                if ((isTouchingWallLeft && moveDirection.x < 0) || 
                    (isTouchingWallRight && moveDirection.x > 0))
                {
                    // Игрок пытается двигаться в сторону стены, ограничиваем скорость
                    horizontalVelocity = 0;
                }
            }
            
            rb.linearVelocity = new Vector2(horizontalVelocity, rb.linearVelocity.y);
        }
    }

    void CheckIfGrounded()
    {
        // Метод 1: Использование OverlapCircle
        bool circleCheck = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // Метод 2: Использование OverlapBox для более точной проверки
        Vector2 boxSize = new Vector2(groundCheckRadius * 2, extraGroundCheckDistance);
        bool boxCheck = Physics2D.OverlapBox(groundCheck.position, boxSize, 0f, groundLayer);
        
        // Метод 3: Использование Raycast вниз
        bool raycastCheck = Physics2D.Raycast(
            groundCheck.position, 
            Vector2.down, 
            groundCheckRadius + extraGroundCheckDistance, 
            groundLayer
        );
        
        // Комбинируем методы для большей надежности
        bool wasGrounded = isGrounded;
        isGrounded = circleCheck || boxCheck || raycastCheck;
        
        // Обновляем время последнего контакта с землей для coyoteTime
        if (isGrounded && !wasGrounded)
        {
            lastGroundedTime = Time.time;
        }
    }
    
    void CheckForWalls()
    {
        // Проверяем наличие стены слева от игрока
        isTouchingWallLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        
        // Проверяем наличие стены справа от игрока
        isTouchingWallRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        
        // Для более точной проверки, можно использовать несколько лучей на разной высоте
        float halfHeight = GetComponent<Collider2D>().bounds.extents.y * 0.5f;
        
        if (!isTouchingWallLeft)
        {
            isTouchingWallLeft = Physics2D.Raycast(transform.position + new Vector3(0, halfHeight, 0), Vector2.left, wallCheckDistance, wallLayer);
        }
        
        if (!isTouchingWallRight)
        {
            isTouchingWallRight = Physics2D.Raycast(transform.position + new Vector3(0, halfHeight, 0), Vector2.right, wallCheckDistance, wallLayer);
        }
    }
    
    void HandleWallSliding()
    {
        bool isMovingTowardsWall = (isTouchingWallLeft && moveDirection.x < 0) || 
                                   (isTouchingWallRight && moveDirection.x > 0);
                                   
        bool isWallSliding = !isGrounded && isMovingTowardsWall && rb.linearVelocity.y < 0;
        
        if (isWallSliding)
        {
            // Увеличиваем гравитацию, чтобы игрок скользил по стене, а не зависал
            rb.gravityScale = originalGravityScale * wallSlidingGravityMultiplier;
        }
        else
        {
            // Возвращаем нормальную гравитацию
            rb.gravityScale = originalGravityScale;
        }
    }

    void FlipTowardsAim()
    {
        float directionToAim = aimWorldPosition.x - transform.position.x;

        if (directionToAim > 0.01f)
        {
            mySR.flipX = true;
            gunSR.flipY = false;
            headSR.flipY = false;
        }
        else if (directionToAim < -0.01f)
        {
            mySR.flipX = false;
            gunSR.flipY = true;
            headSR.flipY = true;
        }
    }

    public void SetKnockbackVelocity(Vector2 knockbackVelocity)
    {
        applyKnockbackVelocity = true;
        knockbackVelocityToApply = knockbackVelocity;
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    void SetAnimatorSpeedValue() 
    {
        float speed = Mathf.Abs(rb.linearVelocity.magnitude);
        animator.SetFloat("Speed", speed);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        
        // Рисуем сферу для OverlapCircle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        
        // Рисуем бокс для OverlapBox
        Gizmos.color = Color.green;
        Vector2 boxSize = new Vector2(groundCheckRadius * 2, extraGroundCheckDistance);
        Gizmos.DrawWireCube(groundCheck.position, boxSize);
        
        // Рисуем луч для Raycast
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(groundCheck.position, Vector2.down * (groundCheckRadius + extraGroundCheckDistance));
        
        // Рисуем лучи для проверки стен
        Gizmos.color = isTouchingWallLeft ? Color.red : Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.left * wallCheckDistance);
        
        Gizmos.color = isTouchingWallRight ? Color.red : Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.right * wallCheckDistance);
    }
}
