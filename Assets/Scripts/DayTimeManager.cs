using System.Collections.Generic;
using UnityEngine;

public class DayTimeManager : MonoBehaviour
{
    [SerializeField] private float daySunAngle = 50.0f, duskSunAngle = 170.0f, nightSunAngle = 270.0f;
    [Space]
    [SerializeField] private Transform sunTransform;

    [SerializeField] private List<Light> planeNightLights = new List<Light>();
    
    public void ApplySettings(GameManager.MapSettings settings)
    {
        if (settings == GameManager.MapSettings.Day)
        {
            sunTransform.eulerAngles = new Vector3(daySunAngle, sunTransform.eulerAngles.y, sunTransform.eulerAngles.z);
            foreach (Light l in planeNightLights)
            {
                l.gameObject.SetActive(false);
            }
        }
        else if (settings == GameManager.MapSettings.Dusk)
        {
            sunTransform.eulerAngles = new Vector3(duskSunAngle, sunTransform.eulerAngles.y, sunTransform.eulerAngles.z);
            foreach (Light l in planeNightLights)
            {
                l.gameObject.SetActive(true);
            }
        }
        else if (settings == GameManager.MapSettings.Night)
        {
            sunTransform.eulerAngles = new Vector3(nightSunAngle, sunTransform.eulerAngles.y, sunTransform.eulerAngles.z);
            foreach (Light l in planeNightLights)
            {
                l.gameObject.SetActive(true);
            }
        }
    }
}
