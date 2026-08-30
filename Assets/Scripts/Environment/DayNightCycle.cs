using UnityEngine;
using RentIsDue.Core;
using System.Collections.Generic;

namespace RentIsDue.Environment
{
    public class DayNightCycle : MonoBehaviour
    {
        public Light sunLight;
        public List<Light> streetLamps = new List<Light>();

        private bool lampsOn = false;

        private void Start()
        {
            if (sunLight == null)
            {
                Light[] allLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var l in allLights)
                {
                    if (l.type == LightType.Directional)
                    {
                        sunLight = l;
                        break;
                    }
                }
            }
            
            SetLampsState(false);
        }

        private void Update()
        {
            if (TimeManager.Instance == null || sunLight == null) return;

            int minutes = TimeManager.Instance.currentTimeMinutes;
            
            // 8:00 = 480, 18:00 = 1080, 22:00 = 1320
            
            // Turn on lamps at 18:00 (1080)
            if (minutes >= 1080 && !lampsOn)
            {
                SetLampsState(true);
            }
            else if (minutes < 1080 && lampsOn)
            {
                SetLampsState(false);
            }

            // Adjust sun intensity and rotation
            if (minutes <= 1080) // 8:00 to 18:00
            {
                float t = Mathf.InverseLerp(480, 1080, minutes);
                // Sun goes from high angle to low angle
                float angle = Mathf.Lerp(45f, 5f, t);
                sunLight.transform.rotation = Quaternion.Euler(angle, -30f, 0f);
                
                sunLight.intensity = Mathf.Lerp(1.2f, 0.3f, t);
                sunLight.color = Color.Lerp(new Color(1f, 0.95f, 0.9f), new Color(1f, 0.6f, 0.2f), t);
            }
            else // 18:00 to 22:00
            {
                float t = Mathf.InverseLerp(1080, 1320, minutes);
                // Sunset to night
                float angle = Mathf.Lerp(5f, -10f, t);
                sunLight.transform.rotation = Quaternion.Euler(angle, -30f, 0f);
                
                sunLight.intensity = Mathf.Lerp(0.3f, 0.05f, t);
                sunLight.color = Color.Lerp(new Color(1f, 0.6f, 0.2f), new Color(0.2f, 0.3f, 0.6f), t);
            }
        }

        private void SetLampsState(bool on)
        {
            lampsOn = on;
            foreach (var lamp in streetLamps)
            {
                if (lamp != null) lamp.enabled = on;
            }
        }
    }
}
