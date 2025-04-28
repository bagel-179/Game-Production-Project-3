using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform dashOrientation;
    [SerializeField] private TimeShiftManager timeShiftManager;
    [HideInInspector, SerializeField] public Transform playerModel;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float groundDrag = 5f;
    private float currentSpeed;
    private float originalSpeed;
    private bool isSlowed = false;
    private Coroutine currentSlowRoutine;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 30f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Jump Settings")]
    [SerializeField] public int maxJumps = 2;
    [HideInInspector] public int jumpCount = 0;
    [SerializeField] private float jumpForce = 18f;
    private float airMultiplier = 1f;
    private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;
    [HideInInspector] public bool isGrounded;

    [Header("Spin Settings")]
    [SerializeField] private float spinDuration = 0.3f;
    [SerializeField] private float spinSpeed = 3060f;
    private bool isSpinning = false;

    [Header("Ground Slam")]
    [SerializeField] private GroundSlam groundSlam;
    private float jumpStartHeight;
    private bool canSlam = false;
    private bool movementLocked = false;

    [Header("Glide Settings")]
    [SerializeField] bool enableGlide = true;
    [SerializeField] private float glideGravityScale = 0.2f;
    private float normalGravityScale = 2f;
    private float glideFallSpeedLimit = 2f;
    [SerializeField] private float swaySpeed = 2f;
    [SerializeField] private float swayAngle = 10f;
    [SerializeField] private float maxGlideDuration = 3f;
    private float currentSwayAngle = 0f;
    private float swayTime = 0f;
    private bool isGliding;
    private float glideTimer = 0f; 
    [HideInInspector] public bool canGlide = true;

    [Header("Particles")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem glideParticles;

    [Header("Sounds")]
    [SerializeField] private AudioClip shiftSound;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private AudioSource audioSource;

    public Camera activeCamera;

    public static event System.Action OnJump;
    public bool IsGrinding { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        rb.freezeRotation = true;
        rb.linearVelocity = Vector3.zero;

        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        HandleInput();
        ApplyDrag();
        CheckGroundStatus();

        if (Input.GetKeyDown(KeyCode.R) || Input.GetButtonDown("Shift"))
        {
            timeShiftManager.TimeShift();
            audioSource.clip = shiftSound;
            audioSource.Play();
        }

        if (isGrounded)
        {
            canGlide = true;
            glideTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
            MovePlayer();
        if (isGliding) ApplyGlideSway();
        ApplyGravity();
    }

    private void HandleInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;

        Vector3 forward = activeCamera.transform.forward;
        Vector3 right = activeCamera.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        if (!isSpinning && moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            dashOrientation.rotation = Quaternion.Slerp(dashOrientation.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrinding) return; 

            if (jumpCount < maxJumps && !movementLocked)
            {
                Jump();
                jumpParticles.Play();
            }
        }

        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetButtonDown("Slam")) && canSlam)
        {
            groundSlam.ActivateGroundSlam();
            canGlide = false;
            canSlam = false;
        }

        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Dash")) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private void MovePlayer()
    {
        if (movementLocked) return;

        float speedToUse = isSlowed ? currentSpeed : moveSpeed;
        float multiplier = isGrounded ? 1f : airMultiplier;
        Vector3 targetVelocity = moveDirection * speedToUse * multiplier;

        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
    }

    public bool IsJumping()
    {
        return Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps && !movementLocked;
    }

    public void Jump()
    {
        if (jumpCount >= maxJumps) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpCount++;

        OnJump?.Invoke();

        jumpParticles.Play();
        jumpStartHeight = transform.position.y;
        canSlam = true;

        if (jumpCount >= 1) StartCoroutine(SpinEffect());
    }

    private void ApplyGravity()
    {
        if (isGrounded && jumpCount > 0) jumpCount = 0;

        bool tryingToGlide = enableGlide && Input.GetKey(KeyCode.Space) && rb.linearVelocity.y < 0;
        float gravityScale = (tryingToGlide && canGlide) ? glideGravityScale : normalGravityScale;

        rb.AddForce(Vector3.down * gravityScale * Mathf.Abs(Physics.gravity.y), ForceMode.Acceleration);

        if (tryingToGlide && canGlide && rb.linearVelocity.y < -glideFallSpeedLimit)
        {
            isGliding = true;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -glideFallSpeedLimit, rb.linearVelocity.z);
            glideParticles.Play();

            glideTimer += Time.deltaTime;
            if (glideTimer >= maxGlideDuration)
            {
                CancelGlide();
            }
        }
        else if (isGliding)
        {
            CancelGlide();
        }
    }

    private void ApplyDrag()
    {
        rb.linearDamping = isGrounded ? groundDrag : 0f;
    }

    public void CheckGroundStatus()
    {
        isGrounded = Physics.SphereCast(transform.position, 0.3f, Vector3.down, out _, playerHeight * 0.5f + 0.2f, groundLayer);
    }

    public void SetMovementLock(bool locked)
    {
        movementLocked = locked;
    }

    private void ApplyGlideSway()
    {
        swayTime += Time.deltaTime * swaySpeed;
        float targetSway = Mathf.Sin(swayTime) * swayAngle;

        currentSwayAngle = Mathf.Lerp(currentSwayAngle, targetSway, Time.deltaTime * 5f);

        playerModel.localRotation = Quaternion.Euler(playerModel.localRotation.eulerAngles.x, playerModel.localRotation.eulerAngles.y, currentSwayAngle);
    }

    private IEnumerator SpinEffect()
    {
        isSpinning = true;
        float elapsedTime = 0f;

        while (elapsedTime < spinDuration)
        {
            float rotationAmount = spinSpeed * Time.deltaTime;
            playerModel.Rotate(0f, rotationAmount, 0f, Space.Self);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerModel.rotation = Quaternion.Euler(playerModel.eulerAngles.x, Mathf.Round(playerModel.eulerAngles.y), playerModel.eulerAngles.z);

        isSpinning = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float savedYVelocity = rb.linearVelocity.y;

        float originalDrag = rb.linearDamping;
        rb.linearDamping = 0f;

        Vector3 dashDirection = dashOrientation.forward;
        dashDirection.y = 0f;
        dashDirection.Normalize();
        rb.linearVelocity = new Vector3(dashDirection.x * dashForce, savedYVelocity, dashDirection.z * dashForce);

        yield return new WaitForSeconds(dashDuration);

        rb.linearDamping = originalDrag;

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void ApplySlow(float slowAmount, float slowDuration)
    {
        if (currentSlowRoutine != null)
            StopCoroutine(currentSlowRoutine);

        originalSpeed = moveSpeed;
        currentSpeed = moveSpeed * slowAmount;
        isSlowed = true;
        Debug.Log("Slow!");

        currentSlowRoutine = StartCoroutine(RemoveSlowAfterDelay(slowDuration));
    }

    private IEnumerator RemoveSlowAfterDelay(float slowDuration)
    {
        yield return new WaitForSeconds(slowDuration);
        currentSpeed = originalSpeed;
        isSlowed = false;
        Debug.Log("Not Slow!");
    }

    public void CancelGlide()
    {
        isGliding = false;
        canGlide = false;
        glideParticles.Stop();
    }
}