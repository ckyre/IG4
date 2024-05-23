using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAnimation : MonoBehaviour {

    public Transform propeller;
    public float propSpeed = 100;

    public float smoothTime = .5f;
    [Header ("Aileron (Roll)")]
    public Transform aileronLeft;
    public Transform aileronRight;
    public float aileronMax = 20;
    [Header ("Elevator (Pitch)")]
    public Transform elevator;
    public float elevatorMax = 20;
    [Header ("Rudder (Yaw)")]
    public Transform rudder;
    public float rudderMax = 20;
    
    [Space] [SerializeField] private PlaneExplodeAnimation planeExplodeAnimation;
    
    // Smoothing vars
    float smoothedRoll;
    float smoothRollV;
    float smoothedPitch;
    float smoothPitchV;
    float smoothedYaw;
    float smoothYawV;

    MFlight.Demo.Plane plane;
    private Rigidbody rb;

    void Start () {
        plane = GetComponent<MFlight.Demo.Plane> ();
        rb = GetComponent<Rigidbody>();
    }

    void Update () {
        // https://en.wikipedia.org/wiki/Aircraft_principal_axes

        if (plane.IsStopped() == true)
        {
            propeller.Rotate (Vector3.forward * (propSpeed / 4) * Time.deltaTime);
        }

        else
        {
            propeller.Rotate (Vector3.forward * propSpeed * Time.deltaTime);
        }

        
        // Roll
        float targetRoll = plane.Roll;
        smoothedRoll = Mathf.SmoothDamp (smoothedRoll, targetRoll, ref smoothRollV, Time.deltaTime * smoothTime);
        aileronLeft.localEulerAngles = new Vector3 (-smoothedRoll * aileronMax, aileronLeft.localEulerAngles.y, aileronLeft.localEulerAngles.z);
        aileronRight.localEulerAngles = new Vector3 (smoothedRoll * aileronMax, aileronRight.localEulerAngles.y, aileronRight.localEulerAngles.z);

        // Pitch
        float targetPitch = plane.Pitch;
        smoothedPitch = Mathf.SmoothDamp (smoothedPitch, targetPitch, ref smoothPitchV, Time.deltaTime * smoothTime);
        elevator.localEulerAngles = new Vector3 (-smoothedPitch * elevatorMax, elevator.localEulerAngles.y, elevator.localEulerAngles.z);

        // Yaw
        float targetYaw = plane.Yaw;
        smoothedYaw = Mathf.SmoothDamp (smoothedYaw, targetYaw, ref smoothYawV, Time.deltaTime * smoothTime);
        rudder.localEulerAngles = new Vector3 (rudder.localEulerAngles.x, -smoothedYaw * rudderMax, rudder.localEulerAngles.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable") == true)
            return;

        if (other.CompareTag("effect") == true)
            return;

        float velocity = rb.velocity.magnitude;
        if (velocity > 5.0f)
        {
            planeExplodeAnimation.transform.position = transform.position;
            planeExplodeAnimation.transform.rotation = transform.rotation;

            Vector3 explodePosition = new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z);
            planeExplodeAnimation.Trigger(explodePosition);

            rb.isKinematic = true;
            rb.gameObject.SetActive(false);
            
            GameManager.instance.PlayerCrash();
        }
    }
}