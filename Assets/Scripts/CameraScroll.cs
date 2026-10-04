using UnityEngine;

public class CameraScroll : MonoBehaviour
{
    public float speed = 2.0f;
    public bool isDiving = false;

    // Camera shake variables
    public bool isShaking = false;
    public float shakeAmount = 0.05f;

    void Update()
    {
        if (isDiving)
        {
            // Calculate normal downward movement
            Vector3 move = Vector3.down * speed * Time.deltaTime;

            // Add random shake offset if oxygen is low
            if (isShaking)
            {
                move.x += Random.Range(-shakeAmount, shakeAmount);
                move.y += Random.Range(-shakeAmount, shakeAmount);
            }

            transform.Translate(move);
        }
    }
}