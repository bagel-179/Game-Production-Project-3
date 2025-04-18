using UnityEngine;
using System.Collections;

public class GroundSlam : MonoBehaviour
{
    [SerializeField] private Transform playerModel;
    [SerializeField] private GameObject slamCollider;
    //[SerializeField] private ParticleSystem slamParticles;
    //[SerializeField] private AudioSource slamSound;
    private PlayerMovement playerMovement;
    private Rigidbody rb;

    [Header("Settings")]
    [SerializeField] private float spinDuration = 0.5f;
    [SerializeField] private float slamSpeed = 30f;
    [SerializeField] private float postSlamBounce = 2f;
    [SerializeField] public float minJumpSlam = 3f;
    [SerializeField] private float slamColliderDuration = 0.2f;

    private Vector3 origModelRotation;
    private bool isSlamming = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        origModelRotation = playerModel.localEulerAngles;

        slamCollider.SetActive(false);
    }

    public void ActivateGroundSlam()
    {
        if (isSlamming || playerMovement == null) return;

        StartCoroutine(GroundSlamRoutine());
    }

    private IEnumerator GroundSlamRoutine()
    {
        isSlamming = true;
        playerMovement.SetMovementLock(true); 

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float spinTime = 0f;
        while (spinTime < spinDuration)
        {
            float spinProgress = spinTime / spinDuration;
            float spinAmount = Mathf.Lerp(0f, 360f, spinProgress);
            playerModel.localRotation = Quaternion.Euler(spinAmount, origModelRotation.y, origModelRotation.z);

            spinTime += Time.deltaTime;
            yield return null;
        }

        playerModel.localRotation = Quaternion.Euler(origModelRotation);

        while (!playerMovement.isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -slamSpeed, rb.linearVelocity.z);
            yield return null;
        }

        SlamImpact();
        isSlamming = false;
        playerMovement.SetMovementLock(false); 
    }

    private void SlamImpact()
    {
        //slamParticles.Play();
        //slamSound.Play();

        rb.AddForce(Vector3.up * postSlamBounce, ForceMode.Impulse);

        slamCollider.SetActive(true);
        StartCoroutine(DeactivateSlamCollider());
    }

    private IEnumerator DeactivateSlamCollider()
    {
        yield return new WaitForSeconds(slamColliderDuration);
        slamCollider.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
        }
    }
}