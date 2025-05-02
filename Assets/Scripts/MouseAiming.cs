using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAiming : MonoBehaviour
{
    [Header("Look Settings")]
    [Tooltip("Should the object look at mouse position?")]
    public bool lookAtMouse = true;

    [Header("Follow Settings")]
    [Tooltip("Should the object follow the mouse?")]
    public bool followMouse = true;
    [Tooltip("Speed at which the object follows the mouse")]
    public float followSpeed = 5f;
    [Tooltip("Maximum distance the object can move from its starting position")]
    public float maxDistance = 3f;
    [Tooltip("How smoothly the object follows the mouse (lower = smoother)")]
    public float smoothTime = 0.3f;

    // Internal variables
    private Vector3 initialPosition;
    private Vector3 currentVelocity = Vector3.zero;
    private Camera mainCamera;
    private Vector2 mouseScreenPos;

    private PlayerControls playerControls;
    private InputAction lookAction;

    private PlayerMovement playerMovement;

    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found.");
        }

        playerControls = new PlayerControls();

        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
             Debug.LogError("PlayerMovement not found in MouseAiming.", this.gameObject);
        }
    }

    private void OnEnable()
    {
        lookAction = playerControls.Player.Look;
        playerControls.Player.Enable();
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
    }

    void Update()
    {
        if (mainCamera == null || playerMovement == null) return;

        mouseScreenPos = lookAction.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            Mathf.Abs(mainCamera.transform.position.z)
        ));

        Vector3 direction = mouseWorldPos - transform.position;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0, 0, angle);

        playerMovement.SetAimPosition(mouseWorldPos);
    }
}
