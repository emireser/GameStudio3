using UnityEngine;
using UnityEngine.SceneManagement;

public class BossProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 4f;
    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // Find the player at the exact moment the projectile is fired
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Calculate direction pointing directly at the player
            moveDirection = (player.transform.position - transform.position).normalized;
        }
        else
        {
            moveDirection = Vector3.down; // Fallback direction
        }
    }

    void Update()
    {
        // Move towards the targeted direction
        transform.position += moveDirection * speed * Time.deltaTime;

        // Lock Z axis to stay on the 2D plane
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        // If it hits the player, restart the game
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        // Destroy the projectile if it hits the walls or harpoons
        else if (other.CompareTag("DeathWall") || other.CompareTag("Harpoon"))
        {
            Destroy(gameObject);
        }
    }
}