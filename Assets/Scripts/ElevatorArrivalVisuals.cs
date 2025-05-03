using UnityEngine;
using System.Collections;

public class ElevatorArrivalVisuals : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Open elevator sprite")]
    public Sprite openSprite;
    [Tooltip("Closed elevator sprite")]
    public Sprite closedSprite;

    [Header("Settings")]
    [Tooltip("Delay before closing elevator (in seconds)")]
    public float closeDelay = 0.75f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on this object for elevator visuals!", this.gameObject);
            enabled = false;
            return;
        }

        if (openSprite == null || closedSprite == null)
        {
             Debug.LogError("Open Sprite and/or Closed Sprite not assigned!", this.gameObject);
             enabled = false;
             return;
        }
    }

    void Start()
    {
        spriteRenderer.sprite = openSprite;
        Debug.Log($"Elevator '{gameObject.name}' set to open state.");

        StartCoroutine(CloseElevatorAfterDelay());
    }

    IEnumerator CloseElevatorAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);

        spriteRenderer.sprite = closedSprite;
        Debug.Log($"Elevator '{gameObject.name}' closed after delay.");
    }
}
