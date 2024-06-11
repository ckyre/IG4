//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using UnityEngine;
using Plane = MFlight.Demo.Plane;

namespace MFlight
{
    /// <summary>
    /// Combination of camera rig and controller for aircraft. Requires a properly set
    /// up rig. I highly recommend either using or referencing the included prefab.
    /// </summary>
    ///

    public class MouseFlightController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] [Tooltip("Transform of the aircraft the rig follows and references")]
        private Transform aircraft = null;
        [SerializeField] [Tooltip("Transform of the object the mouse rotates to generate MouseAim position")]
        private Transform mouseAim = null;
        [SerializeField] [Tooltip("Transform of the object on the rig which the camera is attached to")]
        private Transform cameraRig = null;
        [SerializeField] [Tooltip("Transform of the camera itself")]
        private Transform cam = null;

        [Header("Options")]
        public Vector3 offset;
        [SerializeField] [Tooltip("Follow aircraft using fixed update loop")]
        private bool useFixed = true;

        [SerializeField] [Tooltip("How quickly the camera tracks the mouse aim point.")]
        private float camSmoothSpeed = 5f;

        [SerializeField] [Tooltip("Mouse sensitivity for the mouse flight target")]
        private float mouseSensitivity = 3f;

        [SerializeField] [Tooltip("How far the boresight and mouse flight are from the aircraft")]
        private float aimDistance = 500f;

        public bool freezeControls = true;

        [SerializeField] private float shakeFrequency;
        [SerializeField] private float freeFallShakeFrequency;
        [SerializeField] private float freeFallShakeFadeDuration;
        
        [SerializeField] private float maxHeightShake;
        [SerializeField] private float xShakeFactor;
        [SerializeField] private float yShakeFactor;
        [SerializeField] private float zShakeFactor;

        [SerializeField] private float xDiveShakeFactor;
        [SerializeField] private float yDiveShakeFactor;
        [SerializeField] private float zDiveShakeFactor;

        float currentFrequency;


        [Space]
        [SerializeField] [Tooltip("How far the boresight and mouse flight are from the aircraft")]
        private bool showDebugInfo = false;

        private Vector3 frozenDirection = Vector3.forward;
        private bool isMouseAimFrozen = false;

        private float shakeFactorTarget = 0.0f;
        float shakeFactorCurrent = 0.0f;
        private bool stop = false;

        private Plane planeController;
        
        /// <summary>
        /// Get a point along the aircraft's boresight projected out to aimDistance meters.
        /// Useful for drawing a crosshair to aim fixed forward guns with, or to indicate what
        /// direction the aircraft is pointed.
        /// </summary>
        public Vector3 BoresightPos
        {
            get
            {
                return aircraft == null
                     ? transform.forward * aimDistance
                     : (aircraft.transform.forward * aimDistance) + aircraft.transform.position;
            }
        }

        /// <summary>
        /// Get the position that the mouse is indicating the aircraft should fly, projected
        /// out to aimDistance meters. Also meant to be used to draw a mouse cursor.
        /// </summary>
        public Vector3 MouseAimPos
        {
            get
            {
                if (mouseAim != null)
                {
                    return isMouseAimFrozen
                        ? GetFrozenMouseAimPos()
                        : mouseAim.position + (mouseAim.forward * aimDistance);
                }
                else
                {
                    return transform.forward * aimDistance;
                }
            }
        }

        private void Awake()
        {
            if (aircraft == null)
                Debug.LogError(name + "MouseFlightController - No aircraft transform assigned!");
            if (mouseAim == null)
                Debug.LogError(name + "MouseFlightController - No mouse aim transform assigned!");
            if (cameraRig == null)
                Debug.LogError(name + "MouseFlightController - No camera rig transform assigned!");
            if (cam == null)
                Debug.LogError(name + "MouseFlightController - No camera transform assigned!");

            planeController = aircraft.GetComponent<Plane>();
            
            // To work correctly, the entire rig must not be parented to anything.
            // When parented to something (such as an aircraft) it will inherit those
            // rotations causing unintended rotations as it gets dragged around.
            transform.parent = null;

            if (!Application.isEditor) {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;
            }
        }

        private void Start()
        {
            if (aircraft != null)
            {
                transform.rotation = aircraft.rotation;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (useFixed == false)
            {
                UpdateCameraPos();
            }
            
            if (freezeControls == false)
            {
                RotateRig();
            }

            // test
            if (Input.GetKey(KeyCode.Space))
            {
                if (!stop)
                {
                    stop = true;
                    shakeFactorTarget = 1.0f;
                }
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (stop)
                {
                    stop = false;
                    shakeFactorTarget = 0.0f;
                }
            }

            shakeFactorCurrent = Mathf.Lerp(shakeFactorCurrent, shakeFactorTarget, Time.deltaTime / freeFallShakeFadeDuration);
        }

        private void FixedUpdate()
        {
            if (useFixed == true && freezeControls == false)
            {
                UpdateCameraPos();
            }
        }

        void LateUpdate()
        {
            // Move camera to follow the rig.
            cam.position = cameraRig.position;
            cam.rotation = cameraRig.rotation;
            cam.position += cam.forward * offset.z;
            cam.position += cam.up * offset.y;
            cam.position += cam.right * offset.x;
            
            // Add noise to camera position.
            float normalizedHeight = Mathf.Clamp01(aircraft.position.y / maxHeightShake);
            float secondaryNormalizedHeight = Mathf.Clamp01(aircraft.position.y / 250);

            float mainHeightShake = shakeFactorCurrent.Remap(0.0f, 1.0f, shakeFrequency * normalizedHeight, freeFallShakeFrequency*2.0f);

            float divePower = shakeFactorCurrent.Remap(0.0f, 1.0f, shakeFrequency * normalizedHeight, freeFallShakeFrequency)*secondaryNormalizedHeight;

            float xNoise = Mathf.PerlinNoise1D(Mathf.Abs(aircraft.position.x) * mainHeightShake) * 2 - 1;
            float yNoise = Mathf.PerlinNoise1D(Mathf.Abs(aircraft.position.y) * mainHeightShake) * 2 - 1;
            float zNoise = Mathf.PerlinNoise1D(Mathf.Abs(aircraft.position.z) * mainHeightShake) * 2 - 1;

            float xGlobalShakeFactor = shakeFactorCurrent.Remap(0.0f, 1.0f, xShakeFactor, xDiveShakeFactor);
            float yGlobalShakeFactor = shakeFactorCurrent.Remap(0.0f, 1.0f, yShakeFactor, yDiveShakeFactor);
            float zGlobalShakeFactor = shakeFactorCurrent.Remap(0.0f, 1.0f, zShakeFactor, zDiveShakeFactor);
            cam.position += new Vector3(xNoise * divePower* xGlobalShakeFactor, yNoise * divePower* yGlobalShakeFactor, zNoise * divePower* zGlobalShakeFactor);
        }

        private void RotateRig()
        {
            if (mouseAim == null || cam == null || cameraRig == null)
                return;

            // Freeze the mouse aim direction when the free look key is pressed.
            if (Input.GetKeyDown(KeyCode.C))
            {
                isMouseAimFrozen = true;
                frozenDirection = mouseAim.forward;
            }
            else if  (Input.GetKeyUp(KeyCode.C))
            {
                isMouseAimFrozen = false;
                mouseAim.forward = frozenDirection;
            }

            // Mouse input.
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotate the aim target that the plane is meant to fly towards.
            // Use the camera's axes in world space so that mouse motion is intuitive.
            mouseAim.Rotate(cam.right, mouseY, Space.World);
            mouseAim.Rotate(cam.up, mouseX, Space.World);

            // The up vector of the camera normally is aligned to the horizon. However, when
            // looking straight up/down this can feel a bit weird. At those extremes, the camera
            // stops aligning to the horizon and instead aligns to itself.
            Vector3 upVec = (Mathf.Abs(mouseAim.forward.y) > 0.9f) ? cameraRig.up : Vector3.up;

            // Smoothly rotate the camera to face the mouse aim.
            cameraRig.rotation = Damp(cameraRig.rotation,
                                      Quaternion.LookRotation(mouseAim.forward, upVec),
                                      camSmoothSpeed,
                                      Time.deltaTime);
        }

        private Vector3 GetFrozenMouseAimPos()
        {
            if (mouseAim != null)
                return mouseAim.position + (frozenDirection * aimDistance);
            else
                return transform.forward * aimDistance;
        }

        private void UpdateCameraPos()
        {
            if (aircraft != null)
            {
                // Move the whole rig to follow the aircraft.
                transform.position = aircraft.position;
            }
        }

        // Thanks to Rory Driscoll
        // http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
        /// <summary>
        /// Creates dampened motion between a and b that is framerate independent.
        /// </summary>
        /// <param name="a">Initial parameter</param>
        /// <param name="b">Target parameter</param>
        /// <param name="lambda">Smoothing factor</param>
        /// <param name="dt">Time since last damp call</param>
        /// <returns></returns>
        private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        private void OnDrawGizmos()
        {
            if (showDebugInfo == true)
            {
                Color oldColor = Gizmos.color;

                // Draw the boresight position.
                if (aircraft != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(BoresightPos, 10f);
                }

                if (mouseAim != null)
                {
                    // Draw the position of the mouse aim position.
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(MouseAimPos, 10f);

                    // Draw axes for the mouse aim transform.
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(mouseAim.position, mouseAim.forward * 50f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(mouseAim.position, mouseAim.up * 50f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(mouseAim.position, mouseAim.right * 50f);
                }

                Gizmos.color = oldColor;
            }
        }

    }

    public static class ExtensionMethods
    {
        public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }
    }

}
