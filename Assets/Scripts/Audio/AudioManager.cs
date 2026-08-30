using UnityEngine;

namespace RentIsDue.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Custom Audio Clips (Optional)")]
        public AudioClip pickupClip;
        public AudioClip sellClip;
        public AudioClip searchClip;
        public AudioClip dayPassClip;
        public AudioClip gameOverClip;

        private AudioSource sfxSource;
        private AudioSource bgmSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f; // 2D Sound (Phát trực tiếp vào tai không bị giảm âm theo khoảng cách)
            sfxSource.volume = 1f;

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;
            bgmSource.playOnAwake = false;

            // Đảm bảo trong Scene luôn có 1 AudioListener để nghe được âm thanh
            if (Object.FindAnyObjectByType<AudioListener>() == null)
            {
                Camera cam = Camera.main != null ? Camera.main : Object.FindAnyObjectByType<Camera>();
                if (cam != null)
                {
                    cam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    gameObject.AddComponent<AudioListener>();
                }
            }
        }

        private void Start()
        {
            PlayAmbientBGM();
        }

        public void PlayAmbientBGM()
        {
            if (bgmSource == null) return;

            int sampleRate = 44100;
            float duration = 2.0f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            // Tự động tạo âm thanh nền Ambient (Drone/Pad)
            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;
                
                // Trộn vài sóng sine ở tần số thấp để tạo drone pad
                float wave1 = Mathf.Sin(2 * Mathf.PI * 55f * time); // A1
                float wave2 = Mathf.Sin(2 * Mathf.PI * 55.5f * time); // Chênh phô nhẹ (detune)
                float wave3 = Mathf.Sin(2 * Mathf.PI * 110f * time); // A2

                float sample = (wave1 + wave2 + wave3) / 3f;
                
                // LFO điều biến nhẹ âm lượng
                float modulation = Mathf.Sin(2 * Mathf.PI * 0.5f * time); 
                sample *= 0.5f + (0.5f * modulation);

                samples[i] = sample * 0.3f;
            }

            AudioClip clip = AudioClip.Create("AmbientDrone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void PlayPickup()
        {
            if (pickupClip != null)
            {
                sfxSource.PlayOneShot(pickupClip, 1.0f);
            }
            else
            {
                // Âm thanh 'Pop' nhặt đồ vui tai
                PlayProceduralBeep(520f, 980f, 0.09f, 0.7f);
            }
        }

        public void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
                sfxSource.PlayOneShot(clip, volume);
        }

        public void PlaySell()
        {
            if (sellClip != null)
            {
                sfxSource.PlayOneShot(sellClip, 1.0f);
            }
            else
            {
                // Âm thanh 'Ka-ching' leng keng bán đồ
                PlayProceduralBeep(987f, 1480f, 0.15f, 0.8f);
            }
        }

        public void PlaySearch()
        {
            if (searchClip != null)
            {
                sfxSource.PlayOneShot(searchClip, 0.8f);
            }
            else
            {
                PlayProceduralBeep(350f, 480f, 0.05f, 0.4f);
            }
        }

        public void PlayDayPass()
        {
            if (dayPassClip != null)
            {
                sfxSource.PlayOneShot(dayPassClip, 1f);
            }
            else
            {
                PlayProceduralBeep(523f, 1046f, 0.35f, 0.8f);
            }
        }

        public void PlayGameOver()
        {
            if (gameOverClip != null)
            {
                sfxSource.PlayOneShot(gameOverClip, 1f);
            }
            else
            {
                PlayProceduralBeep(250f, 110f, 0.5f, 0.8f);
            }
        }

        // Tự động tạo âm thanh tổng hợp (Procedural Audio) tức thì
        private void PlayProceduralBeep(float startFreq, float endFreq, float duration, float volume)
        {
            if (sfxSource == null) return;

            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float envelope = Mathf.Sin(t * Mathf.PI); // Smooth attack and release envelope
                samples[i] = Mathf.Sin(2 * Mathf.PI * currentFreq * ((float)i / sampleRate)) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("ProceduralTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            sfxSource.PlayOneShot(clip);
        }
    }
}
