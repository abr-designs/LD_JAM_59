using UnityEngine;

public class UIWindFlap : MonoBehaviour
{
    [Header("Base Sway (slow wind)")]
    public float swayAngle = 10f;
    public float swaySpeed = 0.5f;

    [Header("Flapping (fast oscillation)")]
    public float flapAngle = 15f;
    public float flapSpeed = 8f;

    [Header("Gust Noise")]
    public float gustStrength = 1.5f;
    public float gustSpeed = 1.2f;

    [Header("Axis Influence")]
    public Vector3 axisStrength = new Vector3(0.5f, 1f, 0f);
    // X = tilt forward/back
    // Y = twist left/right
    // Z = spin (classic UI rotation)

    [Header("Smoothing")]
    public float smoothTime = 0.08f;

    private RectTransform rectTransform;

    private Vector3 velocity;
    private Vector3 currentRotation;
    private float noiseOffset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float time = Time.time;

        // --- BASE SWAY (slow, stable motion)
        float sway = Mathf.Sin(time * swaySpeed) * swayAngle;

        // --- FLAPPING (fast oscillation, like fabric snapping)
        float flap = Mathf.Sin(time * flapSpeed) * flapAngle;

        // --- GUST NOISE (adds chaotic variation)
        float gust = (Mathf.PerlinNoise(noiseOffset, time * gustSpeed) * 2f - 1f) * gustStrength;

        // Combine motion
        float combined = sway + flap + gust;

        // Apply across axes with different weights
        Vector3 targetRotation = new Vector3(
            combined * axisStrength.x,
            combined * axisStrength.y,
            combined * axisStrength.z
        );

        // Smooth toward target
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref velocity, smoothTime);

        rectTransform.localRotation = Quaternion.Euler(currentRotation);
    }
}