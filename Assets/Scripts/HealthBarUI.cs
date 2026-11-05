using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Health bar juice: a white trail that shows how much health was lost,
// plus a shake/punch/flash when the player takes damage.
public class HealthBarUI : MonoBehaviour
{
    public Slider healthSlider;
    public Image fillImage;
    public RectTransform damageTrail; // White bar behind the fill

    public float trailDelay = 0.4f;   // Wait before the white trail starts shrinking
    public float trailSpeed = 0.6f;   // How much of the bar the trail drains per second

    public float shakeAmount = 8f;
    public float shakeDuration = 0.3f;
    public float punchScale = 1.15f;

    private RectTransform rectTransform;
    private Vector2 basePosition;
    private Color fillColor;
    private float trailValue = 1f;
    private float trailWaitTimer = 0f;
    private Coroutine hitRoutine;

    void Awake()
    {
        rectTransform = (RectTransform)transform;
        basePosition = rectTransform.anchoredPosition;
        if (fillImage != null) fillColor = fillImage.color;
        if (healthSlider != null) trailValue = healthSlider.normalizedValue;
    }

    void Update()
    {
        if (healthSlider == null || damageTrail == null) return;

        float current = healthSlider.normalizedValue;
        if (current >= trailValue)
        {
            // Healing: trail snaps to the new value
            trailValue = current;
        }
        else if (trailWaitTimer > 0f)
        {
            trailWaitTimer -= Time.deltaTime;
        }
        else
        {
            trailValue = Mathf.MoveTowards(trailValue, current, trailSpeed * Time.deltaTime);
        }

        Vector2 max = damageTrail.anchorMax;
        max.x = trailValue;
        damageTrail.anchorMax = max;
    }

    public void PlayHit(float strength)
    {
        trailWaitTimer = trailDelay;
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitRoutine(strength));
    }

    IEnumerator HitRoutine(float strength)
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.Clamp01(t / shakeDuration);

            rectTransform.anchoredPosition = basePosition + Random.insideUnitCircle * shakeAmount * strength * k;
            rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, punchScale, k);
            if (fillImage != null) fillImage.color = Color.Lerp(fillColor, Color.white, k);

            yield return null;
        }

        rectTransform.anchoredPosition = basePosition;
        rectTransform.localScale = Vector3.one;
        if (fillImage != null) fillImage.color = fillColor;
        hitRoutine = null;
    }
}
