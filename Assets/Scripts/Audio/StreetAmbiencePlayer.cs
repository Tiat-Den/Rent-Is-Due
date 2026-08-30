using UnityEngine;

namespace RentIsDue.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class StreetAmbiencePlayer : MonoBehaviour
    {
        private AudioSource audioSource;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            
            if (AudioManager.Instance != null)
            {
                AudioClip streetNoise = AudioManager.Instance.CreateStreetNoise();
                audioSource.clip = streetNoise;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }
}
