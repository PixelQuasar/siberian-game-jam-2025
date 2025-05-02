using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Tooltip("The target to follow.")]
    public Transform target;

    [Tooltip("How smooth the camera follows the target. Lower value = faster.")]
    public float smoothTime = 0.3f;

    [Tooltip("Optional offset from the target.")]
    public Vector3 offset = new Vector3(0, 0, -10);
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target not assigned in SmoothCameraFollow!", this);
        }

        if (Mathf.Approximately(offset.z, 0))
        {
            offset.z = transform.position.z;
            Debug.LogWarning("Z offset not set, using current camera Z position: " + offset.z);
        }

    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
