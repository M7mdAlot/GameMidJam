using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GuardAi : MonoBehaviour
{
    public Transform RoomPosition;
    public Transform[] PatrolPoints;
    public GameObject Player;

    private NavMeshAgent agent;
    private Animator animator;
    private enum GuardState { Resting, Patrolling, Alerted, Investigating }
    private GuardState currentState = GuardState.Resting;

    private float restTime = 180f;
    private float patrolRestTime = 120f;
    private float timer = 0f;
    private float patrolTimer = 0f;
    private float patrolDuration;
    private bool returningToRoom = false;
    private int lastPatrolIndex = -1;
    private bool hasEnteredSit = false;
    private float lostPlayerTimer = 0f;
    private float lostPlayerTime = 3f;
    private Vector3 lastKnownPlayerPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.Instance.OnPlayerSpotted += HandlePlayerSpotted;
        GameManager.Instance.OnCameraSpotted += HandleCameraSpotted;
        agent.SetDestination(RoomPosition.position);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerSpotted -= HandlePlayerSpotted;
            GameManager.Instance.OnCameraSpotted -= HandleCameraSpotted;
        }
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
            case GuardState.Investigating:
                UpdateInvestigating();
                break;
        }

        if (hasEnteredSit)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("SitEnter") && state.normalizedTime >= 1f)
            {
                animator.SetBool("sitEnter", false);
                animator.SetBool("sitIdle", true);
            }
        }

        float currentSpeed = agent.velocity.magnitude;
        animator.SetFloat("guardSpeed", currentSpeed);
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
            timer = 0f;
            patrolTimer = 0f;
            patrolDuration = Random.Range(120f, 180f);
            returningToRoom = false;
            hasEnteredSit = false;
            animator.SetBool("sitIdle", false);
            animator.SetBool("sitEnter", false);
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
        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);

        // 1. If the guard is close enough, instantly catch the player!
        if (distanceToPlayer <= 2.5f) // Increased to 2.5f so colliders don't block the catch
        {
            GameManager.Instance.PlayerCaught();
            return; // Stop doing anything else
        }

        // 2. Otherwise, keep chasing the player
        if (distanceToPlayer <= 20f)
        {
            lastKnownPlayerPosition = Player.transform.position;
            lostPlayerTimer = 0f;
            agent.SetDestination(Player.transform.position);
        }
        else
        {
            lostPlayerTimer += Time.deltaTime;

            if (lostPlayerTimer >= lostPlayerTime)
            {
                lostPlayerTimer = 0f;
                currentState = GuardState.Investigating;
                agent.SetDestination(lastKnownPlayerPosition);
                return;
            }
        }

        // 3. If we reached the last known spot and the player is gone, investigate
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = GuardState.Investigating;
            agent.SetDestination(lastKnownPlayerPosition);
        }
    }
    private void UpdateInvestigating()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            timer += Time.deltaTime;
            if (timer >= 5f)
            {
                timer = 0f;
                currentState = GuardState.Patrolling;
                GoToNextPatrolPoint();
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
        StopAllCoroutines();
        hasEnteredSit = false;
        animator.SetBool("sitIdle", false);
        animator.SetBool("sitEnter", false);
        animator.SetBool("sitExit", false);
        animator.Play("Locomotion");
        lastKnownPlayerPosition = Player.transform.position;
        lostPlayerTimer = 0f;
        currentState = GuardState.Alerted;
        timer = 0f;
        agent.SetDestination(Player.transform.position);
    }
    private void HandleCameraSpotted(Vector3 spottedPosition)
    {
        StopAllCoroutines();
        hasEnteredSit = false;
        animator.SetBool("sitIdle", false);
        animator.SetBool("sitEnter", false);
        animator.SetBool("sitExit", false);
        animator.Play("Locomotion");
        currentState = GuardState.Investigating;
        timer = 0f;
        agent.SetDestination(spottedPosition);
    }

    // This catches the player if they physically bump into each other
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Player)
        {
            Debug.Log("Guard physically touched the player!");
            GameManager.Instance.PlayerCaught();
        }
    }

    // This catches the player if one of them is using a Trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // 1. This will print the name of literally ANYTHING that touches the guard
        Debug.Log("The Guard just touched: " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Busted! Triggering GameManager...");
            GameManager.Instance.PlayerCaught();
        }
    }
}