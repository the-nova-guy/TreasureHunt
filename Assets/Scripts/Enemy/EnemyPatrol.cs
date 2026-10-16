using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Walks between waypoints using a NavMeshAgent, pausing briefly at each one.
/// Waypoints are filled in by the Maze Generator window when the enemy is spawned.
/// NOTE: the NavMeshAgent must be DISABLED on the prefab; this script enables it
/// in Start(), after the maze NavMesh has been baked.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrol : MonoBehaviour
{
    public Vector3[] waypoints;

    [SerializeField] float waitTime = 1f;       // pause at each waypoint
    [SerializeField] float arriveDistance = 0.6f;

    NavMeshAgent agent;
    int index;
    float waitTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = true; // NavMesh is ready by now

        if (agent.isOnNavMesh && waypoints != null && waypoints.Length > 0)
            agent.SetDestination(waypoints[index]);
    }

    void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;
        if (waypoints == null || waypoints.Length == 0) return;
        if (agent.pathPending) return;

        // Reached the current waypoint: wait, then head to the next one
        if (agent.remainingDistance <= arriveDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                waitTimer = 0f;
                index = (index + 1) % waypoints.Length;
                agent.SetDestination(waypoints[index]);
            }
        }
    }

    // Shows the patrol route in the Scene view when the enemy is selected
    void OnDrawGizmosSelected()
    {
        if (waypoints == null) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawSphere(waypoints[i] + Vector3.up * 0.3f, 0.3f);
            Gizmos.DrawLine(waypoints[i] + Vector3.up * 0.3f,
                            waypoints[(i + 1) % waypoints.Length] + Vector3.up * 0.3f);
        }
    }
}
