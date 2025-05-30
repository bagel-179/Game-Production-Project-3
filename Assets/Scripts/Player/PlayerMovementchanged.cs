/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    //FIRST ZACH ANIM EDIT
    private Animator animator;

    
    //END OF ZACH ANIM EDIT



    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform dashOrientation;
    [SerializeField] private TimeShiftManager timeShiftManager;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 30f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    private bool isDashing = false;
    private bool canDash = true;

    [Header("Jump Settings")]
    [SerializeField] private int maxJumps = 2;
    private int jumpCount = 0;
    [SerializeField] private float jumpForce = 18f;
    private float airMultiplier = 1f;
    private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    [Header("Spin Settings")]
    [SerializeField] private float spinDuration = 0.3f;
    [SerializeField] private float spinSpeed = 3060f;
    private bool isSpinning = false;

    [Header("Glide Settings")]
    [SerializeField] bool enableGlide = true;
    [SerializeField] private float glideGravityScale = 0.2f;
    private float normalGravityScale = 2f;
    private float glideFallSpeedLimit = 2f;
    [SerializeField] private float swaySpeed = 2f;
    [SerializeField] private float swayAngle = 10f;
    private float currentSwayAngle = 0f;
    private float swayTime = 0f;
    private bool isGliding;

    [Header("Particles")]
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem glideParticles;

    private Rigidbody rb;
    private Vector3 moveDirection;

    public Camera activeCamera;

    private void Awake()
    {
        //zachedit
        animator = GetComponent<Animator>();

        //endzachedit
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.linearVelocity = Vector3.zero;
    }

    private void Update()
    {
        HandleInput();
        ApplyDrag();
        CheckGroundStatus();

        if (Input.GetKeyDown(KeyCode.R))
        {
            timeShiftManager.TimeShift();
        }

        //ZACHS ANIMATION EDIT
        if (moveDirection == Vector3.zero)
        {
            //idle
            animator.SetFloat("Speed", 0);
        }
        else
        {
            animator.SetFloat("Speed", 1);
        }

        //END OF CURRENT ANIMATION EDIT
    }

    private void FixedUpdate()
    {
        if (!isDashing) MovePlayer();
        if (isGliding) ApplyGlideSway();
        ApplyGravity();
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

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

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            Jump();
            jumpParticles.Play();
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

        jumpParticles.Play();

        if (jumpCount >= 1) StartCoroutine(SpinEffect());
    }

    private void ApplyGravity()
    {
        if (isGrounded && jumpCount > 0) jumpCount = 0;

        float gravityScale = (enableGlide && Input.GetKey(KeyCode.Space) && rb.linearVelocity.y < 0) ? glideGravityScale : normalGravityScale;

        rb.AddForce(Vector3.down * gravityScale * Mathf.Abs(Physics.gravity.y), ForceMode.Acceleration);

        if (enableGlide && Input.GetKey(KeyCode.Space) && rb.linearVelocity.y < -glideFallSpeedLimit)
        {
            isGliding = true;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -glideFallSpeedLimit, rb.linearVelocity.z);

            glideParticles.Play();
        }
        else
        {
            isGliding = false;
            glideParticles.Stop();
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
}*/