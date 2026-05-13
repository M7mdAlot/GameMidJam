using UnityEngine;
using UnityEngine.AI;

public class GuardAi : MonoBehaviour
{
    public Transform RoomPosition;
    public Transform[] PatrolPoints;
    public GameObject Player;

    private NavMeshAgent agent;
    private Animator animator;
    private enum GuardState { Resting, Patrolling, Alerted }
    private GuardState currentState = GuardState.Resting;

    private float restTime = 180f;
    private float patrolRestTime = 120f;
    private float timer = 0f;
    private float patrolTimer = 0f;
    private float patrolDuration;
    private bool returningToRoom = false;
    private int lastPatrolIndex = -1;
    private bool hasEnteredSit = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerSpotted += HandlePlayerSpotted;
        agent.SetDestination(RoomPosition.position);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerSpotted -= HandlePlayerSpotted;
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

        // once sitEnter finishes switch to sitIdle and stay there
        if (hasEnteredSit)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("SitEnter") && state.normalizedTime >= 1f)
            {
                animator.SetBool("sitEnter", false);
                animator.SetBool("sitIdle", true);
            }
        }
    }

    private void UpdateResting()
    {
        if (agent.remainingDistance <= agent.stoppingDistance && !hasEnteredSit)
        {
            hasEnteredSit = true;
            animator.SetBool("sitEnter", true);
        }

        timer += Time.deltaTime;
        if (timer >= restTime)
        {
            StandUp();
            timer = 0f;
            patrolTimer = 0f;
            patrolDuration = Random.Range(120f, 180f);
            returningToRoom = false;
            currentState = GuardState.Patrolling;
            GoToNextPatrolPoint();
        }
    }

    private void StandUp()
    {
        hasEnteredSit = false;
        animator.SetBool("sitIdle", false);
        animator.SetBool("sitEnter", false);
        animator.SetBool("sitExit", true);
    }

    private void UpdatePatrolling()
    {
        // reset sitExit once animation finishes
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("SitExit") && state.normalizedTime >= 1f)
        {
            animator.SetBool("sitExit", false);
        }

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
        StandUp();
        currentState = GuardState.Alerted;
        timer = 0f;
        agent.SetDestination(Player.transform.position);
    }
}