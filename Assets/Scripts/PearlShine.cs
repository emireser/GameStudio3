using UnityEngine;

public class PearlShine : MonoBehaviour
{
    public float shineSpeed = 4f;       // Speed of the twinkling glow
    public float scalePulseAmount = 0.08f; // Tiny scaling pulse for sparkling effect
    public Color shineColor = new Color(1.5f, 1.5f, 2.0f, 1f); // Glowing light-blue overlay (overdrive brightness)

    private SpriteRenderer spriteRend;
    private Vector3 startScale;
    private Color baseColor;

    void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;

        if (spriteRend != null)
        {
            baseColor = spriteRend.color;
        }
    }

    void Update()
    {
        if (spriteRend == null) return;

        // Sine wave for smooth pulsing rhythm (Values fluctuate between 0 and 1)
        float t = (Mathf.Sin(Time.time * shineSpeed) + 1f) / 2f;

        // 1. Glimmering Highlight transition (Blinks/Twinkles to overlay bright blue-white glow)
        spriteRend.color = Color.Lerp(baseColor, shineColor, t);

        // 2. Pulse the scale slightly to make it feel alive and shimmering
        float pulse = Mathf.Sin(Time.time * shineSpeed) * scalePulseAmount;
        transform.localScale = startScale + new Vector3(pulse, pulse, 0f);
    }
}