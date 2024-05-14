using UnityEngine;

public class HotAirBalloon : MonoBehaviour
{
    private Vector3 initialPosition;
    public float altitudeAmplitude = 1.0f;
    public float altitudeSpeed = 1.0f;
    public float lateralAmplitude = 1.0f;
    public float lateralSpeed = 1.0f;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        // Altitude Perlin Noise
        float altitude = altitudeAmplitude * Mathf.PerlinNoise(Time.time * altitudeSpeed, 0.0f);

        // Lateral movements Perlin Noise
        float lateralMovementX = lateralAmplitude * Mathf.PerlinNoise(Time.time * lateralSpeed, 0.0f);
        float lateralMovementY = lateralAmplitude * Mathf.PerlinNoise(0.0f, Time.time * lateralSpeed);

        // Apply
        Vector3 newPosition = initialPosition;
        newPosition.y += altitude;
        newPosition.x += lateralMovementX;
        newPosition.z += lateralMovementY;
        transform.position = newPosition;
    }
}