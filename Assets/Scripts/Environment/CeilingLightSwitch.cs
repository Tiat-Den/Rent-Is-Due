using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Audio;

namespace RentIsDue.Environment
{
    public class CeilingLightSwitch : MonoBehaviour, IInteractable
    {
        [Header("Light Components")]
        public Light ceilingLight;
        public Renderer lampFixtureRenderer;
        
        [Header("Initial State")]
        public bool isLightOn = false;

        [Header("Audio Settings")]
        public AudioClip switchSound;

        private void Start()
        {
            ApplyLightState();
        }

        public bool CanInteract(RentIsDue.Player.PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            return isLightOn ? "[E] Tắt Đèn Trần" : "[E] Bật Đèn Trần";
        }

        public void Interact(RentIsDue.Player.PlayerInteractor player)
        {
            isLightOn = !isLightOn;
            ApplyLightState();
            PlayClickSound();
        }

        public void SetLightState(bool on)
        {
            isLightOn = on;
            ApplyLightState();
        }

        private void ApplyLightState()
        {
            if (ceilingLight != null)
            {
                ceilingLight.enabled = isLightOn;
            }

            if (lampFixtureRenderer != null)
            {
                Color emissionColor = isLightOn ? new Color(1f, 0.95f, 0.8f) * 2f : Color.black;
                if (lampFixtureRenderer.material.HasProperty("_EmissionColor"))
                {
                    lampFixtureRenderer.material.SetColor("_EmissionColor", emissionColor);
                    if (isLightOn)
                    {
                        lampFixtureRenderer.material.EnableKeyword("_EMISSION");
                    }
                    else
                    {
                        lampFixtureRenderer.material.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        private void PlayClickSound()
        {
            if (AudioManager.Instance != null)
            {
                // Tiếng click công tắc điện vui tai
                AudioManager.Instance.PlaySearch();
            }
        }
    }
}
