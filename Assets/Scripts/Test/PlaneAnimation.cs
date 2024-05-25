using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

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

    [Space] public VisualEffect visualEffect1; // First VisualEffect component
    public VisualEffect visualEffect2; // Second VisualEffect component
    public float amplitude = 1.0f; // Amplitude of the sine wave
    public float frequency = 1.0f; // Frequency of the sine wave
    public float maxAltitude = 100.0f; // Altitude at which power is 0
    public float minAltitude = 96.0f;  // Altitude at which power is 1
    private float currentSpeed = 1.0f;
    private float targetSpeed = 1.0f;

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
        
        // Audio.
        FlightAudioManager.instance.SetWindVolume(transform.position.y);

        // Splash

        // Calculate the new flow rate using a sine wave function
        float interpolationSpeed = 5.0f;
        

        if (plane.IsStopped())
        {
            targetSpeed = 0.1f;
        }
        else
        {
            targetSpeed = 1.0f;
        }
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * interpolationSpeed);
        float altitude = transform.position.y;
        float power = CalculatePower(altitude);
        float flowRate = currentSpeed* power * 100;

        // Set the "Flow_Rate" parameter in both VisualEffects
        visualEffect1.SetFloat("Flow_Rate", flowRate);
        visualEffect2.SetFloat("Flow_Rate", flowRate);
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

    float CalculatePower(float altitude)
    {
        // clipping altitude
        altitude = Mathf.Clamp(altitude, minAltitude, maxAltitude);

        // Lerp
        float power = Mathf.InverseLerp(maxAltitude, minAltitude, altitude);

        return power;
    }
}