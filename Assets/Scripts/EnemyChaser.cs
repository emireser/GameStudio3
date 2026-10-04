using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    public float speed = 2.5f;
    public float detectionDistance = 10f;
    public float chaseTime = 3f; // How many seconds to chase

    private Transform player;
    private bool isChasing = false;
    private bool isExhausted = false;
    private float chaseTimer = 0f;
    private Transform mainCamera;

    // Direction to swim away when exhausted
    private Vector3 exitDirection;

    void Start()
    {
        // Find player automatically
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 1. Start chasing if player is near and enemy is not exhausted
        if (!isExhausted && !isChasing && distanceToPlayer <= detectionDistance)
        {
            isChasing = true;
            chaseTimer = chaseTime; // Start the 3-second timer
        }

        // 2. Chasing Logic
        if (isChasing)
        {
            chaseTimer -= Time.deltaTime;

            if (chaseTimer <= 0)
            {
                // Time is up, enemy gives up
                isChasing = false;
                isExhausted = true;

                // CRITICAL FIX: Remove the "Enemy" tag so it can't kill the player anymore
                gameObject.tag = "Untagged";

                // Decide which way to swim away (left or right) based on its current position
                if (transform.position.x > 0)
                {
                    exitDirection = Vector3.right; // Swim right
                }
                else
                {
                    exitDirection = Vector3.left; // Swim left
                }
            }
            else
            {
                // Keep moving towards the player
                Vector3 targetPosition = new Vector3(player.position.x, player.position.y, 0f);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            }
        }

        // 3. Exhausted Logic (Swimming away)
        if (isExhausted)
        {
            // Swim away quickly to the side
            transform.Translate(exitDirection * (speed * 1.5f) * Time.deltaTime);
        }

        // 4. Optimization: Destroy if it goes too far off-screen (vertically or horizontally)
        if (mainCamera != null && Mathf.Abs(transform.position.y - mainCamera.position.y) > 15f)
        {
            Destroy(gameObject);
        }
        else if (Mathf.Abs(transform.position.x) > 15f)
        {
            Destroy(gameObject); // Destroy if it swims past the horizontal screen limits
        }
    }
}