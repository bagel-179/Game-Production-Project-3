using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 15f;
    public float alertRadius = 10f;
    public float fieldOfView = 90f;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Transform player;
    private NavMeshAgent agent;
    private Vector3 lastKnownPosition;
    private bool playerDetected = false;

    private static List<EnemyAI> allEnemies = new List<EnemyAI>(); 

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        player = GameObject.FindGameObjectWithTag("Player").transform; 
        allEnemies.Add(this);
    }

    private void OnDestroy()
    {
        allEnemies.Remove(this); 
    }

    private void Update()
    {
        if (CanSeePlayer())
        {
            AlertNearbyEnemies();
            playerDetected = true;
            lastKnownPosition = player.position;
        }

        if (playerDetected)
        {
            MoveToTarget();
        }
    }

    private bool CanSeePlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);

        foreach (Collider col in colliders)
        {
            Transform potentialPlayer = col.transform;
            Vector3 directionToPlayer = (potentialPlayer.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, potentialPlayer.position);

            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if (angleToPlayer > fieldOfView * 0.5f) continue;

            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer))
            {
                player = potentialPlayer;
                return true;
            }
        }

        return false;
    }

    private void MoveToTarget()
    {
        if (agent != null)
        {
            agent.SetDestination(lastKnownPosition);

            if (!CanSeePlayer() && Vector3.Distance(transform.position, lastKnownPosition) < 1f)
            {
                playerDetected = false;
            }
        }
    }

    private void AlertNearbyEnemies()
    {
        foreach (EnemyAI enemy in allEnemies)
        {
            if (enemy != this && Vector3.Distance(transform.position, enemy.transform.position) <= alertRadius)
            {
                enemy.playerDetected = true;
                enemy.lastKnownPosition = player.position;
            }
        }
    }
}