using System;
using System.Collections.Generic;
using UnityEngine;

public class DayTimeManager : MonoBehaviour
{
    [Serializable]
    public class MapSettings
    {
        public Color sunColor = Color.white;
        public float sunAngle = 50.0f;
        [Range(0.0f, 1.0f)]
        public float sunIntensity = 1.0f;
        public Material waterMaterial;
        public bool activatePlaneLights = false;
        public Material skyMaterial;
    }

    [SerializeField] private Light sun;
    [SerializeField] private List<Light> planeNightLights = new List<Light>();
    [SerializeField] private MeshRenderer water;

    [SerializeField] private MapSettings daySettings, duskSettings, nightSettings;

    private void Start()
    {
        if(GameManager.instance == null)
            Debug.LogWarning("No GameManager in the scene!");
        else
            ApplySettings(GameManager.instance.mapSettings);
    }
    
    public void ApplySettings(GameManager.MapSettings settings)
    {
        MapSettings mapSettings = daySettings;
        if (settings == GameManager.MapSettings.Dusk)
        {
            mapSettings = duskSettings;
        }
        else if (settings == GameManager.MapSettings.Night)
        {
            mapSettings = nightSettings;
        }

        ApplySunSettings(mapSettings);
        ApplySkyboxSettings(mapSettings);
        
        water.material = mapSettings.waterMaterial;

        foreach (Light l in planeNightLights)
        {
            l.gameObject.SetActive(mapSettings.activatePlaneLights);
        }
    }

    private void ApplySunSettings(MapSettings settings)
    {
        sun.transform.eulerAngles = new Vector3(settings.sunAngle, sun.transform.eulerAngles.y, sun.transform.eulerAngles.z);
        sun.intensity = settings.sunIntensity;
        sun.color = settings.sunColor;
    }

    private void ApplySkyboxSettings(MapSettings settings)
    {
        RenderSettings.skybox = settings.skyMaterial;
        DynamicGI.UpdateEnvironment();
    }
}
