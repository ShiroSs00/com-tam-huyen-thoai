using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float lookSensitivity = 0.08f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip walkSound;
    private AudioSource audioSource;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float yVelocity;
    private float pitch;
    private bool sprintHeld;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInteraction = GetComponent<PlayerInteraction>();
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound for the player's own footsteps
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
        UpdateAnimator();
    }

    private void Look()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        bool grounded = controller.isGrounded;
        if (grounded && yVelocity < 0f) yVelocity = -2f;

        float speed = sprintHeld ? runSpeed : walkSpeed;
        Vector3 dir = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(dir * speed * Time.deltaTime);

        yVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * yVelocity * Time.deltaTime);

        // Handle walking audio
        bool isMoving = moveInput.magnitude > 0.1f && grounded;
        if (isMoving && walkSound != null)
        {
            audioSource.pitch = sprintHeld ? 1.4f : 1f;
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (!isMoving && audioSource.isPlaying && audioSource.clip == walkSound)
        {
            audioSource.Stop();
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        float speedPercent = moveInput.magnitude * (sprintHeld ? 1f : 0.5f);
        animator.SetFloat("Speed", speedPercent, 0.1f, Time.deltaTime);
        animator.SetBool("IsGrounded", controller.isGrounded);
        animator.SetFloat("VerticalVelocity", yVelocity);
    }

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => lookInput = value.Get<Vector2>();
    public void OnSprint(InputValue value) => sprintHeld = value.isPressed;

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;
        if (!controller.isGrounded) return;

        yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        if (animator != null) animator.SetTrigger("Jump");
    }

    /// <summary>
    /// Called by PlayerInput khi nhấn phím Interact (E / Gamepad North).
    /// Forward sang PlayerInteraction nếu có trên cùng GameObject.
    /// </summary>
    public void OnInteract(InputValue value)
    {
        if (playerInteraction != null)
            playerInteraction.OnInteract(value);
    }

    // ── Cached refs ───────────────────────────────────────────────────────────
    private PlayerInteraction playerInteraction;

    // Khởi tạo trong Awake (sau khi controller đã được lấy)
}