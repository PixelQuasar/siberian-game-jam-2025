using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private AudioSource audioSource;
    private AudioClip currentClip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);

        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (newClip == currentClip && audioSource.isPlaying)
        {
            return;
        }

        if (newClip == null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("Music stopped (requested clip was null).");
            }
            currentClip = null;
            return;
        }

        currentClip = newClip;
        audioSource.clip = currentClip;
        audioSource.Play();
        Debug.Log($"Music changed to: {currentClip.name}");
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", audioSource.volume);
    }
}
