using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float dashSpeed = 15.0f;
    public float dashDuration = 0.2f;

    public CameraScroll mainCameraScript;
    public ObjectSpawner spawnerScript;

    public float screenMinX = -8.5f;
    public float screenMaxX = 8.5f;

    // Harpoon Settings
    public GameObject harpoonPrefab;
    public float harpoonCooldown = 1.0f;
    private float nextHarpoonTime = 0f;
    private Vector3 lastAimDirection = Vector3.up; // Default aim direction

    public GameObject bossPrefab;

    // UI Settings
    public Slider oxygenSlider;
    public Image oxygenFillImage;
    public float maxOxygen = 100f;
    public float oxygenDrainRate = 5f;
    private float currentOxygen;

    public Slider pearlProgressBar;
    public TextMeshProUGUI bossWarningText;
    private int collectedPearls = 0;
    private bool bossSequenceStarted = false;

    private Rigidbody rb;
    private float dashTimer = 0.0f;
    private bool isDashing = false;
    private Vector3 movement;
    private bool hasJumped = false;
    private float introDropTimer = 0f;
    private Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        currentOxygen = maxOxygen;

        if (pearlProgressBar != null)
        {
            pearlProgressBar.maxValue = 10;
            pearlProgressBar.value = 0;
        }

        if (oxygenSlider != null)
        {
            oxygenSlider.maxValue = maxOxygen;
            oxygenSlider.value = currentOxygen;
        }

        if (bossWarningText != null) bossWarningText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1. GAME START / DROP FROM BOAT (ONLY WITH "JUMP" BUTTON)
        if (!hasJumped)
        {
            if (Input.GetButtonDown("Jump"))
            {
                hasJumped = true;
                introDropTimer = 0.5f;
                if (mainCameraScript != null) mainCameraScript.isDiving = true;
            }
            return;
        }

        if (!bossSequenceStarted) currentOxygen -= oxygenDrainRate * Time.deltaTime;

        if (oxygenSlider != null)
        {
            oxygenSlider.value = currentOxygen;
            if (currentOxygen < 25f && currentOxygen > 0)
            {
                if (oxygenFillImage != null) oxygenFillImage.color = Color.red;
                if (mainCameraScript != null) mainCameraScript.isShaking = true;
            }
            else
            {
                if (oxygenFillImage != null) oxygenFillImage.color = Color.cyan;
                if (mainCameraScript != null) mainCameraScript.isShaking = false;
            }
        }

        if (currentOxygen <= 0) Die();

        // Movement and Aiming Logic
        if (introDropTimer > 0)
        {
            introDropTimer -= Time.deltaTime;
            movement = new Vector3(0f, -1f, 0f);
            lastAimDirection = Vector3.down; // Aim down during the initial drop
        }
        else
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
            movement = new Vector3(moveX, moveY, 0f);

            // Update last aim direction if the player is actively moving the joystick
            if (movement.magnitude > 0.1f)
            {
                lastAimDirection = movement.normalized;
            }
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) isDashing = false;
        }

        if (introDropTimer <= 0 && Input.GetButtonDown("Jump"))
        {
            if (!isDashing)
            {
                isDashing = true;
                dashTimer = dashDuration;
            }
        }

        // Harpoon Shooting Logic
        if (introDropTimer <= 0 && Input.GetButtonDown("Fire1") && Time.time >= nextHarpoonTime)
        {
            ShootHarpoon();
            nextHarpoonTime = Time.time + harpoonCooldown;
        }
    }

    void FixedUpdate()
    {
        if (!hasJumped) return;
        if (isDashing) rb.linearVelocity = movement.normalized * dashSpeed;
        else rb.linearVelocity = movement.normalized * moveSpeed;
    }

    void LateUpdate()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, screenMinX, screenMaxX);
        transform.position = clampedPosition;
    }

    void ShootHarpoon()
    {
        if (harpoonPrefab != null)
        {
            // Spawn harpoon slightly in front of the aiming direction
            Vector3 spawnPosition = transform.position + (lastAimDirection * 1.5f);
            GameObject newHarpoon = Instantiate(harpoonPrefab, spawnPosition, Quaternion.identity);

            // Rotate harpoon tip to face the aim direction
            newHarpoon.transform.up = lastAimDirection;

            // Assign direction to the Harpoon script
            Harpoon harpoonScript = newHarpoon.GetComponent<Harpoon>();
            if (harpoonScript != null)
            {
                harpoonScript.direction = lastAimDirection;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DeathWall"))
        {
            if (!bossSequenceStarted) Die();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oxygen"))
        {
            currentOxygen += 40f;
            if (currentOxygen > maxOxygen) currentOxygen = maxOxygen;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Enemy")) Die();

        if (other.CompareTag("Pearl"))
        {
            collectedPearls++;
            if (pearlProgressBar != null) pearlProgressBar.value = collectedPearls;
            Destroy(other.gameObject);

            if (collectedPearls >= 10 && !bossSequenceStarted)
            {
                StartBossSequence();
            }
        }
    }

    void StartBossSequence()
    {
        bossSequenceStarted = true;
        if (spawnerScript != null) spawnerScript.isSpawningActive = false;
        if (mainCameraScript != null) mainCameraScript.isDiving = false;
        StartCoroutine(BossCountdownRoutine());
    }

    private IEnumerator BossCountdownRoutine()
    {
        if (bossWarningText != null)
        {
            bossWarningText.gameObject.SetActive(true);
            bossWarningText.text = "BOSS COMING!";
            yield return new WaitForSeconds(1.5f);
            bossWarningText.text = "3";
            yield return new WaitForSeconds(1f);
            bossWarningText.text = "2";
            yield return new WaitForSeconds(1f);
            bossWarningText.text = "1";
            yield return new WaitForSeconds(1f);
            bossWarningText.text = "FIGHT!";
            yield return new WaitForSeconds(1f);
            bossWarningText.gameObject.SetActive(false);
        }

        // Spawn the Boss at the very top edge of the CAMERA
        if (bossPrefab != null && mainCam != null)
        {
            Vector3 spawnPos = new Vector3(0f, mainCam.transform.position.y + 4.5f, 0f);
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }
    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}