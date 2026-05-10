using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Audio/BGM Manager")]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.6f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;

    private AudioSource _backgroundAudio;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSource();

            if (playOnAwake)
            {
                PlayBg();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBg()
    {
        if (_backgroundAudio == null)
        {
            InitializeAudioSource();
        }

        if (_backgroundAudio == null)
        {
            return;
        }

        if (backgroundMusic != null && _backgroundAudio.clip != backgroundMusic)
        {
            _backgroundAudio.clip = backgroundMusic;
        }

        if (_backgroundAudio.clip == null)
        {
            return;
        }

        ApplyAudioSettings();

        if (!_backgroundAudio.isPlaying)
        {
            _backgroundAudio.Play();
        }
    }

    public void PauseBg()
    {
        _backgroundAudio?.Pause();
    }

    public void StopBg()
    {
        _backgroundAudio?.Stop();
    }

    public void SetBackgroundMusic(AudioClip clip, bool playImmediately = true)
    {
        if (_backgroundAudio == null)
        {
            InitializeAudioSource();
        }

        if (_backgroundAudio == null)
        {
            return;
        }

        bool isSameClip = _backgroundAudio.clip == clip;
        backgroundMusic = clip;
        _backgroundAudio.clip = backgroundMusic;
        ApplyAudioSettings();

        if (playImmediately)
        {
            if (isSameClip)
            {
                if (!_backgroundAudio.isPlaying)
                {
                    _backgroundAudio.Play();
                }
            }
            else
            {
                _backgroundAudio.Play();
            }
        }
    }

    private void OnValidate()
    {
        var audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            return;
        }

        _backgroundAudio = audioSource;
        ApplyAudioSettings();
    }

    private void InitializeAudioSource()
    {
        _backgroundAudio = GetComponent<AudioSource>();
        if (_backgroundAudio == null)
        {
            _backgroundAudio = gameObject.AddComponent<AudioSource>();
        }

        _backgroundAudio.clip = backgroundMusic;
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        if (_backgroundAudio == null)
        {
            return;
        }

        _backgroundAudio.playOnAwake = false;
        _backgroundAudio.loop = loop;
        _backgroundAudio.volume = volume;
        _backgroundAudio.spatialBlend = 0f;
    }
}
