using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform playerModel;

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 10f;
    public float groundDrag = 5f;

    [Header("Dash Settings")]
    public float dashForce = 20f;      // How strong the dash is
    public float dashDuration = 0.2f;  // How long the dash lasts
    public float dashCooldown = 1f;    // Time before player can dash again
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Jump Settings")]
    public int maxJumps = 3;
    private int jumpCount = 0;
    public float jumpForce = 8f;
    public float airMultiplier = 1f;
    public float jumpCooldown = 0.2f;
    public float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Glide Settings")]
    public bool enableGlide = true;
    public float glideGravityScale = 0.2f; // Reduced gravity while gliding
    public float normalGravityScale = 2f; // Normal gravity
    public float glideFallSpeedLimit = 2f; // Max downward speed while gliding

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Start()
    {
        rb.linearVelocity = Vector3.zero; // Ensure no initial motion
    }

    private void Update()
    {
        HandleInput();
        ApplyDrag();
        CheckGroundStatus();
    }

    private void FixedUpdate()
    {
        if (!isDashing) MovePlayer();
        ApplyGravity();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Get movement direction relative to camera
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private void MovePlayer()
    {
        float multiplier = isGrounded ? 1f : airMultiplier;
        Vector3 targetVelocity = moveDirection * moveSpeed * multiplier;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    private void Jump()
    {
        if (jumpCount >= maxJumps) return;

        // Reset Y velocity to prevent stacking forces
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Apply the jump force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Increment jump count
        jumpCount++;
    }

    private void ApplyGravity()
    {
        if (isGrounded)
        {
            if (jumpCount > 0) jumpCount = 0; // Reset jumps when grounded
        }

        float gravityScale = (enableGlide && Input.GetKey(KeyCode.Space) && rb.linearVelocity.y < 0)
            ? glideGravityScale
            : normalGravityScale;

        // Ensure proper downward force (negative gravity)
        rb.AddForce(Vector3.down * gravityScale * Mathf.Abs(Physics.gravity.y), ForceMode.Acceleration);

        // Limit fall speed when gliding
        if (enableGlide && Input.GetKey(KeyCode.Space) && rb.linearVelocity.y < -glideFallSpeedLimit)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -glideFallSpeedLimit, rb.linearVelocity.z);
        }
    }

    private void ApplyDrag()
    {
        rb.linearDamping = isGrounded ? groundDrag : 0f;
    }

    private void CheckGroundStatus()
    {
        isGrounded = Physics.SphereCast(transform.position, 0.3f, Vector3.down, out _, playerHeight * 0.5f + 0.2f, groundLayer);
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Store current vertical velocity
        float savedYVelocity = rb.linearVelocity.y;

        // Disable drag temporarily to avoid slowdown
        float originalDrag = rb.linearDamping;
        rb.linearDamping = 0f;

        // Apply dash force while preserving vertical velocity
        Vector3 dashDirection = playerModel.forward;
        rb.linearVelocity = new Vector3(dashDirection.x * dashForce, savedYVelocity, dashDirection.z * dashForce);

        yield return new WaitForSeconds(dashDuration);

        // Restore drag
        rb.linearDamping = originalDrag;

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}