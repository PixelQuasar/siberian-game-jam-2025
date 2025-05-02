using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 15f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public GameObject gun;
    public GameObject head;

    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private bool isGrounded;

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
    }

    void Update()
    {
        CheckIfGrounded();
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
        if (context.performed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
    }

    void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void FlipTowardsAim()
    {
        float directionToAim = aimWorldPosition.x - transform.position.x;

        if (directionToAim > 0.01f)
        {
            mySR.flipX = true;
            //transform.localScale = new Vector3(-Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            gunSR.flipY = false;
            headSR.flipY = false;
            //gunSR.flipY = true;
            //headSR.flipX = false;
        }
        else if (directionToAim < -0.01f)
        {
            mySR.flipX = false;
            //transform.localScale = new Vector3(Mathf.Abs(originalScaleX), transform.localScale.y, transform.localScale.z);
            gunSR.flipY = true;
            headSR.flipY = true;
            //gunSR.flipY = false;
            //headSR.flipX = true;
        }
    }

    void SetAnimatorSpeedValue() 
    {
        float speed = Mathf.Abs(rb.linearVelocity.magnitude);
            
        animator.SetFloat("Speed", speed);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
