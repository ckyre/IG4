using UnityEngine;

public class Plane : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] Transform meshTransform;
    [SerializeField] Transform shootPointTransform;
    [SerializeField] Transform helixTransform;
    [Space]
    [SerializeField] private float moveSpeed = 13.0f;
    [SerializeField] private float rotationSpeed = 3.0f;
    [SerializeField] private float shootRate = 0.1f;

    private float helixRotationSpeed = 1600.0f;

    private Vector3 cursorWorldPosition;

    private bool dirty = true;
    private float rotationT = 0.0f;
    private Quaternion startRotation, targetRotation;

    private float nextShootTime = -100.0f;
    
    void Update()
    {
        // Plane movements.
        transform.position += transform.forward * (moveSpeed * Time.deltaTime);

        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || dirty == true)
        {
            // Find mouse pointer position on plane.

            UnityEngine.Plane plane = new UnityEngine.Plane(Vector3.up, transform.position);
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            float enter = 0.0f;
        
            if (plane.Raycast(ray, out enter))
            {
                cursorWorldPosition = ray.GetPoint(enter);
            }
            
            // Plane rotation.
            
            Vector3 cursorDirection = cursorWorldPosition - transform.position;
            cursorDirection.Normalize();
            float angle = Vector3.SignedAngle(transform.forward, cursorDirection, Vector3.up);

            startRotation = transform.rotation;
            targetRotation = startRotation * Quaternion.Euler(Vector3.up * angle);
            rotationT = 0.0f;
        }
        
        // Update plane rotation.

        if (rotationT < 1.0f)
        {
            rotationT = Mathf.Clamp01(rotationT + (rotationSpeed * Time.deltaTime));
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, rotationT);
        }
        
        // Shoot bullets.
        
        if (Input.GetButton("Fire1") && Time.time > nextShootTime)
        {
            nextShootTime = Time.time + shootRate;
            Shoot();
        }
        
        // Helix animation.
        
        helixTransform.RotateAround(helixTransform.position, helixTransform.forward, helixRotationSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Camera follow plane.
        camera.transform.position = new Vector3(transform.position.x, camera.transform.position.y, transform.position.z);
        
        dirty = false;
    }

    private void OnDrawGizmos()
    {
        UnityEngine.Plane plane = new UnityEngine.Plane(transform.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        if (plane.Raycast(ray, out enter))
        {
            Vector3 point = ray.GetPoint(enter);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, 0.2f);
        }
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, shootPointTransform.position, shootPointTransform.rotation);
    }
}
