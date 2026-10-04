using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject oxygenPrefab;
    public GameObject enemyPrefab;
    public GameObject pearlPrefab; // Added pearl prefab

    public float oxygenSpawnInterval = 3f;
    public float enemySpawnInterval = 5f;
    public float pearlSpawnInterval = 4f; // Pearls spawn every 4 seconds

    public float minX = -8f;
    public float maxX = 8f;

    private float oxygenTimer = 0f;
    private float enemyTimer = 0f;
    private float pearlTimer = 0f;

    public CameraScroll mainCameraScript;

    // Stop spawning when boss is triggered
    public bool isSpawningActive = true;

    void Update()
    {
        if (mainCameraScript != null && mainCameraScript.isDiving && isSpawningActive)
        {
            // 1. Oxygen
            oxygenTimer -= Time.deltaTime;
            if (oxygenTimer <= 0f)
            {
                SpawnObject(oxygenPrefab);
                oxygenTimer = oxygenSpawnInterval;
            }

            // 2. Enemy
            if (enemyPrefab != null)
            {
                enemyTimer -= Time.deltaTime;
                if (enemyTimer <= 0f)
                {
                    SpawnObject(enemyPrefab);
                    enemyTimer = enemySpawnInterval;
                }
            }

            // 3. Pearl
            if (pearlPrefab != null)
            {
                pearlTimer -= Time.deltaTime;
                if (pearlTimer <= 0f)
                {
                    SpawnObject(pearlPrefab);
                    pearlTimer = pearlSpawnInterval;
                }
            }
        }
    }

    void SpawnObject(GameObject prefabToSpawn)
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, 0f);
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }
}