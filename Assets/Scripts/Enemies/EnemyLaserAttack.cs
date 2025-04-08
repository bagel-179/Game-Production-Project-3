using UnityEngine;

public class EnemyLaserAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackRange = 10f;
    public Transform attackPoint; 

    [Header("Slow Effect")]
    public float slowAmount = 0.5f;
    public float slowDuration = 0.5f;

    private EnemyAI enemyAI;
    private PlayerMovement player;
    private LineRenderer lineRenderer;

    private bool isAttacking = false;
    private bool hasSlowedPlayer = false;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        player = FindObjectOfType<PlayerMovement>();
        lineRenderer = GetComponentInChildren<LineRenderer>();

        lineRenderer.enabled = false;

        // Debug check for attackPoint
        if (attackPoint == null)
        {
            Debug.LogError("Attack Point not assigned in EnemyLaserAttack!", this);
        }
    }

    private void Update()
    {
        bool shouldAttack = enemyAI.IsPlayerDetected() &&
                            Vector3.Distance(transform.position, enemyAI.GetPlayerTransform().position) <= attackRange;

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
        lineRenderer.enabled = true;
    }

    private void StopAttack()
    {
        isAttacking = false;
        lineRenderer.enabled = false;
        hasSlowedPlayer = false;
    }

    private void PerformLaserAttack()
    {
        if (attackPoint == null) return;

        Vector3 origin = attackPoint.position;
        Vector3 direction = (enemyAI.GetPlayerTransform().position - origin).normalized;

        Ray ray = new Ray(origin, direction);
        Vector3 endPoint = origin + direction * attackRange;

        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Player") && !hasSlowedPlayer)
            {
                player.ApplySlow(slowAmount, slowDuration);
                hasSlowedPlayer = true;
            }
        }

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, endPoint);
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