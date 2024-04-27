using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plane : MonoBehaviour
{
    [SerializeField] Transform helixTransform;
    
    private float helixRotationSpeed = 1400.0f;
    
    void Start()
    {
        
    }

    void Update()
    {
        helixTransform.RotateAround(helixTransform.position, helixTransform.forward, helixRotationSpeed * Time.deltaTime);
    }
}
