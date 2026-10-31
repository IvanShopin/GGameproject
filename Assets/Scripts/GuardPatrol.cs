using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTime = 2f;

    private NavMeshAgent agent;
    private int currentIndex;
    private float waitTimer;
    private bool isWaiting;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
    }

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        agent.SetDestination(waypoints[currentIndex].position);
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        // Дошли до точки? Начинаем ждать
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }

        // Ждём, потом идём к следующей точке
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                currentIndex = (currentIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentIndex].position);
            }
        }
    }

    // Рисуем маршрут в окне Scene, когда охранник выделен
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Transform next = waypoints[(i + 1) % waypoints.Length];
            if (next == null) continue;
            Gizmos.DrawLine(waypoints[i].position, next.position);
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
        }
    }
}