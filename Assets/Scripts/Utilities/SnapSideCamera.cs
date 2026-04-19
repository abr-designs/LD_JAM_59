using GameInput;
using UnityEngine;

public class SnapSideCamera : MonoBehaviour
{
    [Header("Positions")]
    public float leftYaw = -60f;
    public float rightYaw = 60f;

    [Header("Input")]
    public float inputThreshold = 0.2f; // how hard you push before switching sides
    
    [Header("Switch Resistance")]
    public float switchPoint = 0f;       // usually center
    public float switchDeadZone = 10f;   // how far past center before switching

    [Header("Smoothing")]
    public float smoothTime = 0.15f; // lower = snappier
    public float maxSpeed = 1000f;   // cap for fast snapping

    private float currentYaw;
    private float targetYaw;
    private float velocity;

    private enum Side
    {
        Left,
        Right
    }

    private Side currentSide = Side.Left;

    private float m_mouseXDelta;

    private void OnEnable()
    {
        GameInputDelegator.OnMouseMoved += OnMouseMoved;
    }
    
    private void OnDisable()
    {
        GameInputDelegator.OnMouseMoved -= OnMouseMoved;
    }


    private void Start()
    {
        currentYaw = transform.localEulerAngles.y;
        targetYaw = rightYaw;
    }

    private void Update()
    {
        // Only allow switching if BOTH input and position agree
        if (currentSide == Side.Right)
        {
            if (m_mouseXDelta < -inputThreshold && currentYaw < (switchPoint - switchDeadZone))
            {
                currentSide = Side.Left;
            }
        }
        else // currently Left
        {
            if (m_mouseXDelta > inputThreshold && currentYaw > (switchPoint + switchDeadZone))
            {
                currentSide = Side.Right;
            }
        }

        targetYaw = (currentSide == Side.Left) ? leftYaw : rightYaw;

        currentYaw = Mathf.SmoothDampAngle(
            currentYaw,
            targetYaw,
            ref velocity,
            smoothTime,
            maxSpeed
        );

        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    private void OnMouseMoved(Vector2 currentDelta)
    {
        m_mouseXDelta =  currentDelta.x;
    }
}