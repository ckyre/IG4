using UnityEngine;

[CreateAssetMenu(fileName = "NoiseProfile", menuName = "Terrain/NoiseProfile")]
public class NoiseProfile : ScriptableObject
{
    public int octaves = 3;
    
    public float amplitude = 1.0f;
    public float frequency = 1.0f;
    public float scale = 0.3f;
    
    public float persistance = 0.7f;
    public float lacunarity = 0.8f;

    public float Sample(float x, float y)
    {
        float a = amplitude;
        float f = frequency;
        
        float height = 0.0f;
        
        for (int i = 0; i < octaves; i++)
        {
            float xSample = x / scale * f;
            float ySample = y / scale * f;
            float noise = Mathf.PerlinNoise(xSample, ySample) * 2 - 1;
            height += noise * a;

            a *= persistance;
            f *= lacunarity;
        }

        return height;
    }
}
