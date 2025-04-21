using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Audio;

public class GroundSlam : MonoBehaviour
{
    [SerializeField] private Transform playerModel;
    [SerializeField] private GameObject slamCollider;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private ParticleSystem slamParticles;
    [SerializeField] private AudioClip slamSound;
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
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
        origModelRotation = playerModel.localEulerAngles;

        slamCollider.SetActive(false);
    }

    public void ActivateGroundSlam()
    {
        if (isSlamming || playerMovement == null) return;

        playerMovement.CancelGlide();
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

        if(playerMovement.isGrounded)
        {
            SlamImpact();
            isSlamming = false;
            playerMovement.SetMovementLock(false);
        }
    }

    private void SlamImpact()
    {
        slamParticles.Play();
        audioSource.clip = slamSound;
        audioSource.Play();
        StartCoroutine(FadeOutSound());
        impulseSource.GenerateImpulse();


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
            EnemyAI enemyScript = other.GetComponentInParent<EnemyAI>();
            enemyScript.Die();
        }
    }

    private IEnumerator FadeOutSound()
    {
        if (slamSound == null) yield break;

        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / 0.5f);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }


}