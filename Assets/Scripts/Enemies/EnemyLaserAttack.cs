using UnityEngine;

public class EnemyLaserAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 10f;
    public Transform attackPoint;
    public GameObject laser; 

    [Header("Slow Effect")]
    public float slowAmount = 0.5f;
    public float slowDuration = 0.5f;

    private EnemyAI enemyAI;
    private bool isAttacking = false;
    private bool hasSlowedPlayer = false;

    private PlayerMovement player;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        player = FindObjectOfType<PlayerMovement>();
        laser.SetActive(true);
    }

    private void Update()
    {
        bool shouldAttack = enemyAI.IsPlayerDetected() && Vector3.Distance(transform.position, enemyAI.GetPlayerTransform().position) <= attackRange;

        if (shouldAttack && !isAttacking)
        {
            StartAttack();
        }
        else if (!shouldAttack && isAttacking)
        {
            StopAttack();
        }

        if (isAttacking)
        {
            PerformLaserAttack();
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        laser.SetActive(true);
    }

    private void StopAttack()
    {
        isAttacking = false;
        laser.SetActive(false);
        hasSlowedPlayer = false; 
    }

    private void PerformLaserAttack()
    {
        bool isHittingPlayer = Physics.Raycast(new Ray(attackPoint.position, attackPoint.forward), out RaycastHit hitInfo, attackRange) && hitInfo.collider.CompareTag("Player");

        if (isHittingPlayer)
        {
            player.ApplySlow(slowAmount, slowDuration);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackRange > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(attackPoint.position, attackPoint.forward * attackRange); 
        }
    }
}