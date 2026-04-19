using UnityEngine;

public class UIWindDrift : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float maxAngle = 10f;          // Maximum tilt angle (degrees)
    public float baseSpeed = 1f;          // Base oscillation speed
    public float gustStrength = 0.5f;     // How strong random gusts are
    public float gustSpeed = 0.5f;        // How fast gust randomness changes

    [Header("Smoothing")]
    public float smoothTime = 0.2f;

    private RectTransform rectTransform;
    private float velocity;
    private float noiseOffset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        noiseOffset = Random.Range(0f, 100f); // Unique motion per instance
    }

    void Update()
    {
        float time = Time.time;

        // Base smooth oscillation (like gentle wind sway)
        float baseWave = Mathf.Sin(time * baseSpeed);

        // Perlin noise for natural gust variation
        float gust = Mathf.PerlinNoise(noiseOffset, time * gustSpeed) * 2f - 1f;

        // Combine both
        float targetAngle = (baseWave + gust * gustStrength) * maxAngle;

        // Smooth rotation
        float currentZ = rectTransform.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        float smoothed = Mathf.SmoothDamp(currentZ, targetAngle, ref velocity, smoothTime);

        rectTransform.localRotation = Quaternion.Euler(0f, 0f, smoothed);
    }
}