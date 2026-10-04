using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Added for UI Slider

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float dashSpeed = 15.0f;
    public float dashDuration = 0.2f;

    public CameraScroll mainCameraScript;
    public ObjectSpawner spawnerScript; // Reference to stop spawning

    // Oxygen
    public TextMeshProUGUI oxygenText;
    public float maxOxygen = 100f;
    public float oxygenDrainRate = 5f;
    private float currentOxygen;

    // Pearls and Progression
    public Slider pearlProgressBar;
    private int collectedPearls = 0;
    private bool bossSequenceStarted = false;

    private Rigidbody rb;
    private float dashTimer = 0.0f;
    private bool isDashing = false;
    private Vector3 movement;
    private bool hasJumped = false;
    private float introDropTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentOxygen = maxOxygen;

        // Initialize pearl progress bar
        if (pearlProgressBar != null)
        {
            pearlProgressBar.maxValue = 10;
            pearlProgressBar.value = 0;
        }
    }

    void Update()
    {
        if (!hasJumped)
        {
            if (Input.GetButtonDown("Jump") || Input.GetButtonDown("Fire1"))
            {
                hasJumped = true;
                introDropTimer = 0.5f;
                if (mainCameraScript != null) mainCameraScript.isDiving = true;
            }
            return;
        }

        // Stop oxygen drain if boss sequence is active
        if (!bossSequenceStarted)
        {
            currentOxygen -= oxygenDrainRate * Time.deltaTime;
        }

        if (oxygenText != null)
        {
            oxygenText.text = "Oxygen: " + Mathf.Round(currentOxygen).ToString();

            if (currentOxygen < 25f && currentOxygen > 0)
            {
                oxygenText.color = Color.red;
                if (mainCameraScript != null) mainCameraScript.isShaking = true;
            }
            else
            {
                oxygenText.color = Color.white;
                if (mainCameraScript != null) mainCameraScript.isShaking = false;
            }
        }

        if (currentOxygen <= 0) Die();

        // Movement
        if (introDropTimer > 0)
        {
            introDropTimer -= Time.deltaTime;
            movement = new Vector3(0f, -1f, 0f);
        }
        else
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
            movement = new Vector3(moveX, moveY, 0f);
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) isDashing = false;
        }

        if (introDropTimer <= 0 && (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")))
        {
            if (!isDashing)
            {
                isDashing = true;
                dashTimer = dashDuration;
            }
        }
    }

    void FixedUpdate()
    {
        if (!hasJumped) return;

        if (isDashing) rb.linearVelocity = movement.normalized * dashSpeed;
        else rb.linearVelocity = movement.normalized * moveSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DeathWall")) Die();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oxygen"))
        {
            currentOxygen += 30f;
            if (currentOxygen > maxOxygen) currentOxygen = maxOxygen;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Enemy"))
        {
            Die();
        }

        // Collect Pearls
        if (other.CompareTag("Pearl"))
        {
            collectedPearls++;
            if (pearlProgressBar != null) pearlProgressBar.value = collectedPearls;
            Destroy(other.gameObject);

            // Check for Boss Trigger
            if (collectedPearls >= 10 && !bossSequenceStarted)
            {
                StartBossSequence();
            }
        }
    }

    void StartBossSequence()
    {
        bossSequenceStarted = true;
        Debug.Log("10 Pearls Collected! BOSS AWAKENING COUNTDOWN STARTED!");

        // Stop spawning new items/enemies
        if (spawnerScript != null)
        {
            spawnerScript.isSpawningActive = false;
        }

        // We will add the UI countdown and Boss spawning logic here next
    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}