using System;
using UnityEngine;

using GameInput;

namespace Prototypes.Alex
{
    [RequireComponent(typeof(CharacterController))]
    public class QuakeController : MonoBehaviour
    {
        public float mouseSensitivity = 2.5f;
        public Transform playerCamera;

        [Header("Movement")] public float maxSpeed = 10f;
        public float groundAcceleration = 50f;
        public float airAcceleration = 20f;
        public float friction = 8f;
        public float jumpForce = 8f;
        public float gravity = 20f;

        private CharacterController controller;
        private Vector3 velocity;
        private float yVelocity;
        private float xRotation = 0f;

        private Vector2 m_currentInput;
        private Vector2 m_currentMouseDelta;
        private bool m_jumpPressed;

        private void OnEnable()
        {
            GameInputDelegator.OnJumpPressed += OnJumpPressed;
            GameInputDelegator.OnMovementChanged += OnMovementChanged;
            GameInputDelegator.OnMouseMoved += OnMouseMoved;
        }

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }

        private void OnDisable()
        {
            GameInputDelegator.OnMovementChanged -= OnMovementChanged;
            GameInputDelegator.OnMouseMoved -= OnMouseMoved;
        }

        private void Update()
        {
            Look();
            Move();
        }

        private void Look()
        {
            float mouseX = m_currentMouseDelta.x * mouseSensitivity * 100f * Time.deltaTime;
            float mouseY = m_currentMouseDelta.y * mouseSensitivity * 100f * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -89f, 89f);

            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void Move()
        {
            Vector3 wishDir = transform.right * m_currentInput.x + transform.forward * m_currentInput.y;
            wishDir.Normalize();

            if (controller.isGrounded)
            {
                ApplyFriction();

                if (m_jumpPressed)
                    yVelocity = jumpForce;

                Accelerate(wishDir, maxSpeed, groundAcceleration);
            }
            else
            {
                Accelerate(wishDir, maxSpeed, airAcceleration);
            }

            yVelocity -= gravity * Time.deltaTime;

            Vector3 move = velocity + Vector3.up * yVelocity;
            controller.Move(move * Time.deltaTime);

            if (controller.isGrounded && yVelocity < 0)
                yVelocity = -1f;
        }

        private void ApplyFriction()
        {
            Vector3 horizontal = new Vector3(velocity.x, 0, velocity.z);
            float speed = horizontal.magnitude;

            if (speed < 0.1f)
            {
                velocity.x = 0;
                velocity.z = 0;
                return;
            }

            float drop = speed * friction * Time.deltaTime;
            float newSpeed = Mathf.Max(speed - drop, 0);
            newSpeed /= speed;

            velocity.x *= newSpeed;
            velocity.z *= newSpeed;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;

            if (addSpeed <= 0)
                return;

            float accelSpeed = accel * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            velocity += wishDir * accelSpeed;
        }
        //Input Callbacks
        //================================================================================================================//

        private void OnJumpPressed(bool pressed)
        {
            m_jumpPressed = pressed;
        }

        private void OnMouseMoved(Vector2 mouseDelta)
        {
            m_currentMouseDelta = mouseDelta;
        }

        private void OnMovementChanged(Vector2 input)
        {
            m_currentInput = input;
        }
    }
}