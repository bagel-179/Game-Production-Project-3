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
    public float dashForce = 30f;      
    public float dashDuration = 0.2f; 
    public float dashCooldown = 1f;    
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
    public float glideGravityScale = 0.2f;
    public float normalGravityScale = 2f;
    public float glideFallSpeedLimit = 2f; 

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
        rb.linearVelocity = Vector3.zero; 
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

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        jumpCount++;
    }

    private void ApplyGravity()
    {
        if (isGrounded)
        {
            if (jumpCount > 0) jumpCount = 0; 
        }

        float gravityScale = (enableGlide && Input.GetKey(KeyCode.Space) && rb.linearVelocity.y < 0)
            ? glideGravityScale
            : normalGravityScale;

        rb.AddForce(Vector3.down * gravityScale * Mathf.Abs(Physics.gravity.y), ForceMode.Acceleration);

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

        float savedYVelocity = rb.linearVelocity.y;

        float originalDrag = rb.linearDamping;
        rb.linearDamping = 0f;

        Vector3 dashDirection = playerModel.forward;
        rb.linearVelocity = new Vector3(dashDirection.x * dashForce, savedYVelocity, dashDirection.z * dashForce);

        yield return new WaitForSeconds(dashDuration);

        rb.linearDamping = originalDrag;

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}