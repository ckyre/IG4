using UnityEngine;

public class Bobbling : MonoBehaviour
{
    // Height scale for vertical movement
    public float heightScale = 1.0f;

    // Speed at which Perlin noise is sampled for the height
    public float xScale = 1.0f;

    // Control object height above water
    public float objectHeight = 5.0f;

    // Rotation scales
    public float pitchScale = 10.0f; // Forward-backward tilt scale
    public float rollScale = 10.0f;  // Side-to-side tilt scale

    // Speed at which Perlin noise is sampled for rotation
    public float rotationSpeed = 1.0f;

    void Update()
    {
        // Update the height
        float height = heightScale * Mathf.PerlinNoise(Time.time * xScale, 0.0f) + 100 - objectHeight;
        Vector3 pos = transform.position;
        pos.y = height;
        transform.position = pos;

        // Calculate pitch and roll using Perlin noise
        float pitch = pitchScale * (Mathf.PerlinNoise(Time.time * rotationSpeed, 1.0f) - 0.5f) * 2; // Centered around zero
        float roll = rollScale * (Mathf.PerlinNoise(0.0f, Time.time * rotationSpeed) - 0.5f) * 2; // Centered around zero

        // Apply the rotation
        transform.rotation = Quaternion.Euler(pitch, 0.0f, roll);
    }
}
