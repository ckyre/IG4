using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        public bool activateLights = false;
        public Material skyMaterial;
        public Color fogColor = Color.white;
        public float fogDensity = 0.0008f;
        public Color underwaterColorFilter;
        public Color cloudsColor;
    }

    [SerializeField] public Volume underwaterVolume;
    [SerializeField] private Light sun;
    [SerializeField] private CloudsSettings clouds;
    [SerializeField] private List<GameObject> NightLights = new List<GameObject>();
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
        ApplyFogSettings(mapSettings);
        ApplyUnderWaterColorFilter(mapSettings);

        clouds.CloudsColor = mapSettings.cloudsColor;
        water.material = mapSettings.waterMaterial;

        foreach (GameObject l in NightLights)
        {
            l.gameObject.SetActive(mapSettings.activateLights);
        }
    }

    private void ApplySunSettings(MapSettings settings)
    {
        sun.transform.eulerAngles = new Vector3(settings.sunAngle, sun.transform.eulerAngles.y, sun.transform.eulerAngles.z);
        sun.intensity = settings.sunIntensity;
        sun.color = settings.sunColor;
    }

    private void ApplyFogSettings(MapSettings settings)
    {
        RenderSettings.fogColor = settings.fogColor;
        RenderSettings.fogDensity = settings.fogDensity;
    }

    private void ApplySkyboxSettings(MapSettings settings)
    {
        RenderSettings.skybox = settings.skyMaterial;
        DynamicGI.UpdateEnvironment();
    }

    private void ApplyUnderWaterColorFilter(MapSettings settings)
    {
        if (underwaterVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
        {
            // Modify the color filter attribute
            colorAdjustments.colorFilter.value = settings.underwaterColorFilter;
        }
    }
}
