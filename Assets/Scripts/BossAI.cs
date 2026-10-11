using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossAI : MonoBehaviour
{
    public int health = 3;
    public float moveSpeed = 3f;

    // Projectile Settings
    public GameObject projectilePrefab;
    public float shootCooldown = 2.0f; // Fires a projectile every 2 seconds

    private Transform player;
    private Vector3 startPos;
    private float shootTimer = 0f;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        startPos = transform.position;
        shootTimer = shootCooldown;

        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    void Update()
    {
        // Lock Z axis
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if (player == null) return;

        // Hover side to side
        float newX = Mathf.Sin(Time.time * moveSpeed) * 6f;
        transform.position = new Vector3(newX, startPos.y, 0f);

        // Shooting Logic
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            ShootProjectile();
            shootTimer = shootCooldown;
        }
    }

    void ShootProjectile()
    {
        if (projectilePrefab != null)
        {
            // Spawn the projectile slightly below the boss's center
            Vector3 spawnPos = transform.position + new Vector3(0f, -1.5f, 0f);
            Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Harpoon"))
        {
            health--;
            Destroy(other.gameObject);

            if (rend != null) StartCoroutine(FlashRed());

            if (health <= 0)
            {
                LevelComplete();
            }
        }
        else if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator FlashRed()
    {
        rend.material.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        rend.material.color = originalColor;
    }

    void LevelComplete()
    {
        Debug.Log("BOSS DEFEATED! TRANSITIONING TO LEVEL 2...");
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}