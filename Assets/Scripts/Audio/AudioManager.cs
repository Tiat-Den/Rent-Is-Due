using UnityEngine;

namespace RentIsDue.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Custom Audio Clips (Optional)")]
        public AudioClip pickupClip;
        public AudioClip sellClip;
        public AudioClip searchClip;
        public AudioClip dayPassClip;
        public AudioClip gameOverClip;

        private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        public void PlayPickup()
        {
            if (pickupClip != null)
            {
                sfxSource.PlayOneShot(pickupClip, 0.8f);
            }
            else
            {
                PlayProceduralBeep(600f, 850f, 0.08f, 0.3f);
            }
        }

        public void PlaySell()
        {
            if (sellClip != null)
            {
                sfxSource.PlayOneShot(sellClip, 0.9f);
            }
            else
            {
                // Ka-ching dual tone
                PlayProceduralBeep(987f, 1318f, 0.12f, 0.4f);
            }
        }

        public void PlaySearch()
        {
            if (searchClip != null)
            {
                sfxSource.PlayOneShot(searchClip, 0.5f);
            }
            else
            {
                PlayProceduralBeep(300f, 400f, 0.04f, 0.15f);
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
                PlayProceduralBeep(523f, 1046f, 0.35f, 0.5f);
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
                PlayProceduralBeep(250f, 110f, 0.5f, 0.6f);
            }
        }

        // Tự động tạo âm thanh tổng hợp (Procedural Audio) tức thì mà không cần import file mp3/wav ngoài
        private void PlayProceduralBeep(float startFreq, float endFreq, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float envelope = 1f - t; // Linear fade out
                samples[i] = Mathf.Sin(2 * Mathf.PI * currentFreq * ((float)i / sampleRate)) * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("ProceduralTone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            sfxSource.PlayOneShot(clip);
        }
    }
}
