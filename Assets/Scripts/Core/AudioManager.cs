using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        public bool loop = false;
    }

    [Header("Sound Effects")]
    public SoundEffect[] soundEffects;

    private AudioSource[] audioSources;
    private int currentSourceIndex = 0;
    private const int MAX_AUDIO_SOURCES = 10;  // Số lượng AudioSource tối đa

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Tạo pool các AudioSource
        audioSources = new AudioSource[MAX_AUDIO_SOURCES];
        for (int i = 0; i < MAX_AUDIO_SOURCES; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioSources[i] = source;
        }
    }

    public void PlaySound(string soundName)
    {
        SoundEffect sound = System.Array.Find(soundEffects, s => s.name == soundName);
        if (sound == null)
        {
            Debug.LogWarning($"Sound: {soundName} not found!");
            return;
        }

        // Lấy AudioSource tiếp theo trong pool
        AudioSource source = audioSources[currentSourceIndex];
        currentSourceIndex = (currentSourceIndex + 1) % MAX_AUDIO_SOURCES;

        source.clip = sound.clip;
        source.volume = sound.volume;
        source.loop = sound.loop;
        source.Play();

        Debug.Log($"Playing sound: {soundName}");
    }

    public void StopSound(string soundName)
    {
        foreach (AudioSource source in audioSources)
        {
            if (source.clip != null && source.clip.name == soundName)
            {
                source.Stop();
            }
        }
    }

    public void StopAllSounds()
    {
        foreach (AudioSource source in audioSources)
        {
            source.Stop();
        }
    }
}