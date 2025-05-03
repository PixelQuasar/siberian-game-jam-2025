using UnityEngine;

public class PlayerSpawnAtElevator : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Elevator object, where the player should spawn")]
    public GameObject targetElevator;

    [Tooltip("Player object tag")]
    public string playerTag = "Player";

    void Start()
    {
        if (targetElevator == null)
        {
            Debug.LogError("Target Elevator not assigned!", this.gameObject);
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            playerObject.transform.position = targetElevator.transform.position;
            Debug.Log($"Player '{playerObject.name}' moved to elevator '{targetElevator.name}'");

            // Optional: Set rotation (if needed)
            // playerObject.transform.rotation = targetElevator.transform.rotation; 
        }
        else
        {
            Debug.LogError($"Player object with tag '{playerTag}' not found for setting position!", this.gameObject);
        }

        // The component has completed its task, so we disable it to avoid triggering again
        // if the scene is reloaded or the player is recreated.
        enabled = false;
        // Destroy(this); // Alternative - full deletion
    }
}
