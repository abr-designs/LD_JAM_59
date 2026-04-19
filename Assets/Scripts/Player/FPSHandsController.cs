using UnityEngine;
using System.Collections;
using GameInput;

public class FPSHandsController : MonoBehaviour
{
    public enum HandState
    {
        Walking,
        Grabbing,
        Holding,
        Dropping
    }

    [Header("References")]
    public SpriteRenderer leftHand;
    public SpriteRenderer rightHand;

    [Header("Sprites")]
    public Sprite leftWalkingSprite;
    public Sprite rightWalkingSprite;

    public Sprite rightGrabbingSprite;

    public Sprite leftHoldingSprite;
    public Sprite rightHoldingSprite;

    [Header("Bobbing")]
    public float bobIdleSpeed = 3f;
    public float bobWalkingSpeed = 6f;
    public float bobAmount = 0.05f;

    public bool isWalking;

    [Header("Grab Animation")]
    public float grabDuration = 0.25f;
    public float grabForwardAmount = 0.2f;

    private Vector3 leftStartPos;
    private Vector3 rightStartPos;

    private float bobTimer;
    private HandState currentState;

    private Coroutine grabRoutine;

    private void OnEnable()
    {
        GameInputDelegator.OnMovementChanged += OnMovementChanged;
    }

    private void OnDisable()
    {
        GameInputDelegator.OnMovementChanged -= OnMovementChanged;
    }
    
    void Start()
    {
        leftStartPos = leftHand.transform.localPosition;
        rightStartPos = rightHand.transform.localPosition;

        SetState(HandState.Walking);
    }

    void Update()
    {
        HandleBobbing();
    }

    public void SetState(HandState newState)
    {
        // Prevent interrupting grab halfway unless you want that behavior
        /*if (currentState == HandState.Grabbing)
            return;*/

        currentState = newState;

        switch (currentState)
        {
            case HandState.Walking:
                leftHand.sprite = leftWalkingSprite;
                rightHand.sprite = rightWalkingSprite;
                break;

            case HandState.Grabbing:
                if (grabRoutine != null)
                    StopCoroutine(grabRoutine);

                grabRoutine = StartCoroutine(GrabAnimation());
                break;

            case HandState.Holding:
                leftHand.sprite = leftHoldingSprite;
                rightHand.sprite = rightHoldingSprite;
                break;
            
            case HandState.Dropping:
                if (grabRoutine != null)
                    StopCoroutine(grabRoutine);

                grabRoutine = StartCoroutine(GrabAnimation(HandState.Walking));
                break;
        }
    }

    private IEnumerator GrabAnimation(HandState nextState = HandState.Holding)
    {
        currentState = HandState.Grabbing;

        // Set grabbing sprite (right hand only)
        rightHand.sprite = rightGrabbingSprite;

        float timer = 0f;

        Vector3 startPos = rightStartPos;
        Vector3 forwardPos = rightStartPos + new Vector3(0f, 0f, grabForwardAmount);

        while (timer < grabDuration)
        {
            timer += Time.deltaTime;
            float t = timer / grabDuration;

            // Smooth forward motion (ease out)
            float curve = Mathf.Sin(t * Mathf.PI * 0.5f);

            rightHand.transform.localPosition = Vector3.Lerp(startPos, forwardPos, curve);

            yield return null;
        }

        // Snap back to base position
        rightHand.transform.localPosition = rightStartPos;

        // Transition to holding
        SetState(nextState);
    }

    void HandleBobbing()
    {
        if (currentState != HandState.Walking)
        {
            leftHand.transform.localPosition = leftStartPos;
            if (currentState != HandState.Grabbing)
                rightHand.transform.localPosition = rightStartPos;
            return;
        }

        bobTimer += Time.deltaTime * (isWalking ? bobWalkingSpeed : bobIdleSpeed);

        float bobY = Mathf.Sin(bobTimer) * bobAmount;
        float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmount;

        leftHand.transform.localPosition = leftStartPos + new Vector3(-bobX, bobY, 0f);
        rightHand.transform.localPosition = rightStartPos + new Vector3(bobX, bobY, 0f);
    }
    
    private void OnMovementChanged(Vector2 input)
    {
        isWalking = input.magnitude > 0.1f;
    }
}