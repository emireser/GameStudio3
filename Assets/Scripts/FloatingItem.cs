using UnityEngine;

public class FloatingItem : MonoBehaviour
{
    public float floatSpeed = 3f; // Dalgalanma hizi
    public float floatAmount = 0.15f; // Ne kadar buyuyup kuculecegi
    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        // Sinus dalgasi ile puruzsuz buyuyup kuculme efekti
        float scalePulse = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localScale = startScale + new Vector3(scalePulse, scalePulse, 0f);
    }
}