using UnityEngine;

[CreateAssetMenu(fileName = "CloudsSettings", menuName = "Clouds/CloudsSettings")]
public class CloudsSettings : ScriptableObject
{
    [Header("Raymarching settings")]
    public int Steps = 15;
    public int LightSteps = 10;
    public float RenderDistance = 1000.0f;
    
    [Header("Clouds settings")]
    public Color CloudsColor = new Color(1, 1, 1, 1);
    public float CloudsAlpha = 1.0f;
    public Texture2D CloudsNoiseTexure;
    public float CloudsScale = 1;
    public float CloudsSmooth = 5;
    public Vector3 Wind = new Vector3(1,0,0);
    
    [Header("Clouds lightning settings")]
    public float DensityThreshold = 0.25f;
    public float DensityMultiplier = 1;
    public float LightAbsorptionThroughCloud = 0.15f;
    public float LightAbsorptionTowardSun = 0.25f;
    public float DarknessThreshold = 0.1f;
    
    [Header("Container settings")]
    public Vector3 ContainerBoundsMin = new Vector3(-250,50,-250);
    public Vector3 ContainerBoundsMax = new Vector3(250,80,250);
    public float ContainerEdgeFadeDst = 45;

    public void BindMaterial(Material material)
    {
        material.SetInteger("_NumSteps", Steps);
        material.SetInteger("_NumLightSteps", LightSteps);
        material.SetFloat("_RenderDistance", RenderDistance);

        material.SetColor("_CloudsColor", CloudsColor);
        material.SetFloat("_CloudsAlpha", Mathf.Clamp01(CloudsAlpha));
        material.SetTexture("_CloudsShapeNoiseTexture", CloudsNoiseTexure);
        material.SetFloat("_CloudsScale", Mathf.Abs(CloudsScale));
        material.SetFloat("_CloudsSmooth", CloudsSmooth);
        material.SetVector("_Wind", Wind);
        
        material.SetFloat("_DensityThreshold", DensityThreshold);
        material.SetFloat("_DensityMultiplier", Mathf.Abs(DensityMultiplier));
        material.SetFloat("_LightAbsorptionThroughCloud", LightAbsorptionThroughCloud);
        material.SetFloat("_LightAbsorptionTowardSun", LightAbsorptionTowardSun);
        material.SetFloat("_DarknessThreshold", DarknessThreshold);
        
        material.SetVector("_ContainerBoundsMin", ContainerBoundsMin);
        material.SetVector("_ContainerBoundsMax", ContainerBoundsMax);
        material.SetFloat("_ContainerEdgeFadeDst", Mathf.Abs(ContainerEdgeFadeDst));

    }

}
