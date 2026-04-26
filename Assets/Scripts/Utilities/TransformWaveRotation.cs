using UnityEngine;

public class TransformWaveRotation : MonoBehaviour
{
    [Header("Wave A")]
    public Vector2 directionA = new Vector2(1f, 0f);
    public float amplitudeA = 5f;
    public float frequencyA = 1f;

    [Header("Wave B")]
    public Vector2 directionB = new Vector2(0f, 1f);
    public float amplitudeB = 3f;
    public float frequencyB = 1.5f;

    [Header("General Settings")]
    public float rotationStrength = 10f;
    public float smoothSpeed = 5f;

    private Quaternion m_targetLocalRotation;

    private void Update()
    {
        float time = Time.time;

        Vector2 dirA = directionA.normalized;
        Vector2 dirB = directionB.normalized;

        float waveA = Mathf.Sin(time * frequencyA);
        float waveB = Mathf.Sin(time * frequencyB);

        // Build local-space "normal"
        Vector3 localNormal = new Vector3(
            dirA.x * waveA * amplitudeA + dirB.x * waveB * amplitudeB,
            1f,
            dirA.y * waveA * amplitudeA + dirB.y * waveB * amplitudeB
        ).normalized;

        // Convert to LOCAL rotation
        m_targetLocalRotation = Quaternion.FromToRotation(Vector3.up, localNormal);

        // Scale intensity properly (0–1 range is sane)
        m_targetLocalRotation = Quaternion.Slerp(
            Quaternion.identity,
            m_targetLocalRotation,
            rotationStrength
        );

        // Smooth toward target in LOCAL space
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            m_targetLocalRotation,
            Time.deltaTime * smoothSpeed
        );
    }
}
