using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneExplodeAnimation : MonoBehaviour
{
    [SerializeField] private float explosionForce = 20.0f;
    [SerializeField] private float explosionRadius = 5.0f;
    [SerializeField] private List<Rigidbody> elements;
    
    private bool triggered = false;
    
    public void Trigger(Vector3 explositionPosition)
    {
        if (triggered == true)
            return;
        
        triggered = true;

        foreach (Rigidbody elem in elements)
        {
            elem.gameObject.SetActive(true);
            elem.isKinematic = false;
            elem.AddExplosionForce(explosionForce, explositionPosition, explosionRadius);
        }
    }
}
