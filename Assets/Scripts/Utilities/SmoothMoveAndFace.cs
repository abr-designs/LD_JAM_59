using UnityEngine;

public class SmoothMoveAndFace : MonoBehaviour
{
    [Header("Movement")]
    public float maxMoveSpeed = 10f;
    public float positionSmoothTime = 0.2f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;

    private Vector3 targetPosition;
    private Vector3 targetDirection = Vector3.forward;

    private Vector3 velocity;
    
    [Header("Rotation Offset")]
    public Vector3 rotationOffsetEuler; // e.g. (0, -90, 0)

    // --- PUBLIC API ---
    public void SetTarget(Vector3 position, Vector3 forwardDirection)
    {
        targetPosition = position;

        // Avoid zero direction issues
        if (forwardDirection.sqrMagnitude > 0.0001f)
            targetDirection = forwardDirection.normalized;
    }

    void Update()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            positionSmoothTime,
            maxMoveSpeed
        );

        Vector3 dir = targetDirection;

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();

            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
            targetRotation *= Quaternion.Euler(rotationOffsetEuler);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}
