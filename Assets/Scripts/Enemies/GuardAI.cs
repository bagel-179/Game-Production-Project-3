using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    public Transform player;
    public Transform resetPosition;
    public float detectionRange = 15f;

    private NavMeshAgent agent;
    private bool hasResetPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange && distance > agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            hasResetPlayer = false; 
        }
        else if (distance <= agent.stoppingDistance)
        {
            agent.isStopped = true;

            if (!hasResetPlayer)
            {
                ResetPlayerPosition();
                hasResetPlayer = true;
            }
        }
        else
        {
            agent.isStopped = true;
            hasResetPlayer = false;
        }
    }

    private void ResetPlayerPosition()
    {
        if (player == null || resetPosition == null) return;

        Transform root = player.root;
        root.position = resetPosition.position;
        root.rotation = resetPosition.rotation;
    }
}