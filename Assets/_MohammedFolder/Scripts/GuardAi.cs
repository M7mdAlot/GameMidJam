using UnityEngine;
using UnityEngine.AI;

public class GuardAi : MonoBehaviour
{
    public Transform RoomPosition;
    public Transform[] PatrolPoints;
    public GameObject Player;

    private NavMeshAgent agent;
    private enum GuardState { Resting, Patrolling, Alerted }
    private GuardState currentState = GuardState.Resting;

    private float restTime = 180f;
    private float patrolRestTime = 120f;
    private float timer = 0f;
    private float patrolTimer = 0f;
    private float patrolDuration;
    private bool returningToRoom = false;
    private int lastPatrolIndex = -1;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnPlayerSpotted += HandlePlayerSpotted;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerSpotted -= HandlePlayerSpotted;
    }

    private void Start()
    {
        agent.SetDestination(RoomPosition.position);
    }

    private void Update()
    {
        switch (currentState)
        {
            case GuardState.Resting:
                UpdateResting();
                break;
            case GuardState.Patrolling:
                UpdatePatrolling();
                break;
            case GuardState.Alerted:
                UpdateAlerted();
                break;
        }
    }

    private void UpdateResting()
    {
        timer += Time.deltaTime;
        if (timer >= restTime)
        {
            timer = 0f;
            patrolTimer = 0f;
            patrolDuration = Random.Range(120f, 180f);
            returningToRoom = false;
            currentState = GuardState.Patrolling;
            GoToNextPatrolPoint();
        }
    }

    private void UpdatePatrolling()
    {
        if (returningToRoom)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                timer += Time.deltaTime;
                if (timer >= patrolRestTime)
                {
                    timer = 0f;
                    returningToRoom = false;
                    currentState = GuardState.Resting;
                }
            }
            return;
        }

        patrolTimer += Time.deltaTime;

        if (patrolTimer >= patrolDuration)
        {
            returningToRoom = true;
            timer = 0f;
            agent.SetDestination(RoomPosition.position);
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            GoToNextPatrolPoint();
        }
    }

    private void UpdateAlerted()
    {
        agent.SetDestination(Player.transform.position);

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);

            if (distanceToPlayer <= 1.5f)
            {
                GameManager.Instance.PlayerCaught();
            }
            else
            {
                timer = 0f;
                currentState = GuardState.Resting;
                agent.SetDestination(RoomPosition.position);
            }
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (PatrolPoints.Length == 0) return;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, PatrolPoints.Length);
        }
        while (randomIndex == lastPatrolIndex && PatrolPoints.Length > 1);

        lastPatrolIndex = randomIndex;
        agent.SetDestination(PatrolPoints[randomIndex].position);
    }

    private void HandlePlayerSpotted()
    {
        currentState = GuardState.Alerted;
        timer = 0f;
        agent.SetDestination(Player.transform.position);
    }
}