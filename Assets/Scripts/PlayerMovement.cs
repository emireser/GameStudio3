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

    public SpriteRenderer spriteRend;

    // Harpoon Settings
    public GameObject harpoonPrefab;
    public float harpoonCooldown = 1.0f;
    private float nextHarpoonTime = 0f;
    private Vector3 lastAimDirection = Vector3.up;

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

        if (introDropTimer > 0)
        {
            introDropTimer -= Time.deltaTime;
            movement = new Vector3(0f, -1f, 0f);
            lastAimDirection = Vector3.down;
        }
        else
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");
            movement = new Vector3(moveX, moveY, 0f);

            if (movement.magnitude > 0.1f)
            {
                lastAimDirection = movement.normalized;
            }

            if (spriteRend != null)
            {
                if (moveX > 0) spriteRend.flipX = false;
                else if (moveX < 0) spriteRend.flipX = true;
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
            Vector3 spawnPosition = transform.position + (lastAimDirection * 1.5f);
            GameObject newHarpoon = Instantiate(harpoonPrefab, spawnPosition, Quaternion.identity);

            newHarpoon.transform.up = lastAimDirection;

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
        else if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Boss"))
        {
            Die();
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

        if (other.CompareTag("Enemy") || other.CompareTag("Boss")) Die();

        if (other.CompareTag("Pearl"))
        {
            // 1. Disable collision immediately so it isn't collected twice
            Collider col = other.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // 2. Start the flying animation to the UI slider
            StartCoroutine(FlyPearlToUI(other.gameObject));
        }
    }

    // NEW: Pearl Flying Coroutine
    private IEnumerator FlyPearlToUI(GameObject pearl)
    {
        // Fallback if UI components are missing
        if (pearlProgressBar == null || mainCam == null)
        {
            CollectPearlLogic();
            Destroy(pearl);
            yield break;
        }

        Vector3 startPos = pearl.transform.position;
        Vector3 startScale = pearl.transform.localScale;
        float duration = 0.5f; // Flight time (half a second)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            if (pearl == null) yield break;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Smooth ease-in effect
            float smoothT = t * t * (3f - 2f * t);

            // Convert UI screen position to a World position
            Vector3 screenPos = pearlProgressBar.transform.position;
            screenPos.z = Mathf.Abs(mainCam.transform.position.z);
            Vector3 targetWorldPos = mainCam.ScreenToWorldPoint(screenPos);
            targetWorldPos.z = 0f;

            // Move and shrink the pearl towards the slider
            pearl.transform.position = Vector3.Lerp(startPos, targetWorldPos, smoothT);
            pearl.transform.localScale = Vector3.Lerp(startScale, startScale * 0.3f, smoothT);

            yield return null;
        }

        Destroy(pearl);
        CollectPearlLogic(); // Add to UI counter AFTER the flight is done
    }

    private void CollectPearlLogic()
    {
        collectedPearls++;
        if (pearlProgressBar != null) pearlProgressBar.value = collectedPearls;

        if (collectedPearls >= 10 && !bossSequenceStarted)
        {
            StartBossSequence();
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