using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    [SerializeField] Transform helixTransform;

    private float moveSpeed = 13.0f;
    private float helixRotationSpeed = 1400.0f;
    
    void Start()
    {
        
    }

    void Update()
    {
        // Plane movements.
        transform.position += transform.forward * (moveSpeed * Time.deltaTime);
        
        // Helix animation.
        helixTransform.RotateAround(helixTransform.position, helixTransform.forward, helixRotationSpeed * Time.deltaTime);
    }
}
