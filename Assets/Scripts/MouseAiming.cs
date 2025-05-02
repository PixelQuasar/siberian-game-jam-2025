using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAiming : MonoBehaviour
{
    private Camera mainCam;
    private Vector2 mouseScreenPos;

    private PlayerControls playerControls;
    private InputAction lookAction;

    private PlayerMovement playerMovement;

    void Awake()
    {
        mainCam = Camera.main;
        if (mainCam == null)
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
        if (mainCam == null || playerMovement == null) return;

        mouseScreenPos = lookAction.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(
            mouseScreenPos.x,
            mouseScreenPos.y,
            Mathf.Abs(mainCam.transform.position.z)
        ));

        Vector2 aimDirection = (Vector2)mouseWorldPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        playerMovement.SetAimPosition(mouseWorldPos);
    }
}
