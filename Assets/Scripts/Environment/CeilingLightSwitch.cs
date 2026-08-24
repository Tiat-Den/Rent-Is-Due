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

        // Cached material to avoid allocating a new instance each toggle
        private Material _cachedLampMaterial;

        private void Start()
        {
            if (ceilingLight == null)
                Debug.LogWarning($"[CeilingLightSwitch] '{name}': ceilingLight is not assigned in Inspector — light toggle will have no effect.");

            if (lampFixtureRenderer != null)
                _cachedLampMaterial = lampFixtureRenderer.material; // cache once, reuse

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

            if (_cachedLampMaterial != null)
            {
                Color emissionColor = isLightOn ? new Color(1f, 0.95f, 0.8f) * 2f : Color.black;
                if (_cachedLampMaterial.HasProperty("_EmissionColor"))
                {
                    _cachedLampMaterial.SetColor("_EmissionColor", emissionColor);
                    if (isLightOn)
                        _cachedLampMaterial.EnableKeyword("_EMISSION");
                    else
                        _cachedLampMaterial.DisableKeyword("_EMISSION");
                }
            }
        }

        private void PlayClickSound()
        {
            if (AudioManager.Instance == null) return;

            if (switchSound != null)
                AudioManager.Instance.PlayClip(switchSound);
            else
                AudioManager.Instance.PlaySearch(); // fallback nếu chưa gán clip
        }
    }
}
