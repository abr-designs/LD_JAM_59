using GameInput;
using UnityEngine;

public class SnapSideCamera : MonoBehaviour
{
    [Header("Positions")]
    public float leftYaw = -60f;
    public float rightYaw = 60f;

    [Header("Input Resistance")]
    public float switchThreshold = 0.6f; // how much input needed to switch
    public float inputBuildSpeed = 3f;
    public float inputDecaySpeed = 2f;

    [Header("Smoothing")]
    public float smoothTime = 0.15f;
    public float maxSpeed = 1000f;

    private float currentYaw;
    private float targetYaw;
    private float velocity;

    private float inputAccum = 0f;

    private enum Side { Left, Right }
    private Side currentSide = Side.Right;

    private float m_mouseXDelta;

    private void OnEnable()
    {
        GameInputDelegator.OnMouseMoved += OnMouseMoved;
    }
    
    private void OnDisable()
    {
        GameInputDelegator.OnMouseMoved -= OnMouseMoved;
    }
    private void OnMouseMoved(Vector2 currentDelta)
    {
        m_mouseXDelta =  currentDelta.x;
    }

    void Start()
    {
        currentYaw = transform.localEulerAngles.y;
        targetYaw = rightYaw;
    }

    void Update()
    {
        // Build input over time (this is the "resistance")
        inputAccum += m_mouseXDelta * inputBuildSpeed * Time.deltaTime;

        // Decay back toward 0 when not pushing
        inputAccum = Mathf.Lerp(inputAccum, 0f, inputDecaySpeed * Time.deltaTime);

        inputAccum = Mathf.Clamp(inputAccum, -1f, 1f);

        // Switch sides only when input commitment is strong enough
        if (inputAccum > switchThreshold)
        {
            currentSide = Side.Right;
            inputAccum = 0f; // reset so it doesn't instantly flip back
        }
        else if (inputAccum < -switchThreshold)
        {
            currentSide = Side.Left;
            inputAccum = 0f;
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


}