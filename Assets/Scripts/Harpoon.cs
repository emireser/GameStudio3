using UnityEngine;

public class Harpoon : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 3f;
    public Vector3 direction = Vector3.up;

    void Start()
    {
        // Destroy after lifetime expires if it misses the target
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Definitive movement command
        transform.position += direction * speed * Time.deltaTime;

        // Lock Z axis to 0 to stay on the same 2D plane
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        // Destroy if it hits an Enemy or Boss
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            Destroy(gameObject);
        }
    }
}