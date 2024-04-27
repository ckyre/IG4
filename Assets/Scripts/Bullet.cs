using UnityEngine;

public class Bullet : MonoBehaviour
{
    private static float lifetime = 10.0f;
    private static float moveSpeed = 30.0f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * (moveSpeed * Time.deltaTime);
    }
}
