using UnityEngine;

public class SceneMusicTrigger : MonoBehaviour
{
    [Tooltip("Music that should play in this scene. Leave None if music should stop.")]
    public AudioClip sceneMusic;

    void Start()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PlayMusic(sceneMusic);
        }
        else
        {
            Debug.LogError("MusicManager not found in scene! Ensure it is created in the initial scene.");
        }
    }
}
