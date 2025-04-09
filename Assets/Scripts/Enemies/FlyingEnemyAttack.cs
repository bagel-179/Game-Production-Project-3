using UnityEngine;
using System.Collections;

public class FlyingEnemyAttack : MonoBehaviour
{
    [SerializeField] private float disableMovementDuration = 1f;

    [Header("Bounce Settings")]
    [SerializeField] private float bounceUpForce = 5;
    [SerializeField] private float bounceBackForce = 5f;

    [Header("Slow Effect")]
    [SerializeField] private float slowAmount = 0.5f;
    [SerializeField] private float slowDuration = 2f;

    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 2060;
    private bool isSpinning = false;
    private Transform playerModel;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 bounceDirection = (collision.transform.position - transform.position).normalized;
                Vector3 upwardForce = Vector3.up * bounceUpForce;
                Vector3 backwardForce = bounceDirection * bounceBackForce;
                backwardForce.y = 0f;

                playerRb.linearVelocity = Vector3.zero;
                playerRb.AddForce(upwardForce + backwardForce, ForceMode.Impulse);
            }

            PlayerMovement controller = collision.gameObject.GetComponent<PlayerMovement>();

            if (controller != null)
            {
                if (disableMovementDuration > 0f)
                {
                    StartCoroutine(DisableMovementTemporarily(controller, disableMovementDuration));
                    StartCoroutine(SpinEffect(controller.playerModel));
                }

               // if (slowAmount > 0f && slowDuration > 0f)
                {
                    //controller.ApplySlow(slowAmount, slowDuration);
                }
            }
        }
    }

    private IEnumerator DisableMovementTemporarily(MonoBehaviour controller, float duration)
    {
        controller.enabled = false;
        yield return new WaitForSeconds(duration);
        controller.enabled = true;
    }

    private IEnumerator SpinEffect(Transform playerModel)
    {
        if (playerModel == null || isSpinning) yield break;

        isSpinning = true;
        float elapsedTime = 0f;

        while (elapsedTime < disableMovementDuration)
        {
            float rotationAmount = spinSpeed * Time.deltaTime;
            playerModel.Rotate(0f, rotationAmount, 0f, Space.Self);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerModel.rotation = Quaternion.Euler(
            playerModel.eulerAngles.x,
            Mathf.Round(playerModel.eulerAngles.y),
            playerModel.eulerAngles.z
        );

        isSpinning = false;
    }
}