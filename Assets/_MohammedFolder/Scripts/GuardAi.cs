using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GuardAi : MonoBehaviour
{
    [Header("Guard Settings")]
    public Transform RoomPosition;
    public Transform[] PatrolPoints;
    public GameObject Player;

    // <-- NEW: Aggression Settings you can tweak in Unity! -->
    [Header("Aggression & Chase Settings")]
    [Tooltip("How close the guard needs to be to catch you")]
    public float catchDistance = 2.5f; 
    [Tooltip("How far you have to run to break his line of sight")]
    public float giveUpDistance = 40f; 
    [Tooltip("How many seconds he keeps searching AFTER he loses sight of you")]
    public float memoryTime = 8f; 
    
    [Header("Speed Settings")]
    public float patrolSpeed = 3.5f;
    public float chaseSpeed = 7.0f; // He runs faster when alerted!

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
    private Vector3 lastKnownPlayerPosition;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Start at walking speed
        if (agent != null) agent.speed = patrolSpeed;
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
        agent.speed = patrolSpeed; // Ensure he is walking

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
        agent.speed = chaseSpeed; // Ensure he is sprinting!

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);

        // 1. Math-based Catch (Backup in case colliders fail)
        if (distanceToPlayer <= catchDistance) 
        {
            GameManager.Instance.PlayerCaught();
            return; 
        }

        // 2. Chasing the Player
        if (distanceToPlayer <= giveUpDistance)
        {
            lastKnownPlayerPosition = Player.transform.position;
            lostPlayerTimer = 0f; // Keep resetting the timer as long as he sees you
            agent.SetDestination(Player.transform.position);
        }
        else
        {
            // 3. Player ran out of range, start counting down his memory
            lostPlayerTimer += Time.deltaTime;

            if (lostPlayerTimer >= memoryTime)
            {
                lostPlayerTimer = 0f;
                currentState = GuardState.Investigating;
                agent.SetDestination(lastKnownPlayerPosition);
                return;
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = GuardState.Investigating;
            agent.SetDestination(lastKnownPlayerPosition);
        }
    }

    private void UpdateInvestigating()
    {
        agent.speed = patrolSpeed; // Slow back down while looking around

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
        WakeUpGuard();
        lastKnownPlayerPosition = Player.transform.position;
        lostPlayerTimer = 0f;
        currentState = GuardState.Alerted;
        timer = 0f;
        agent.SetDestination(Player.transform.position);
    }

    private void HandleCameraSpotted(Vector3 spottedPosition)
    {
        WakeUpGuard();
        currentState = GuardState.Investigating;
        timer = 0f;
        agent.SetDestination(spottedPosition);
    }

    private void WakeUpGuard()
    {
        StopAllCoroutines();
        hasEnteredSit = false;
        animator.SetBool("sitIdle", false);
        animator.SetBool("sitEnter", false);
        animator.SetBool("sitExit", false);
        animator.Play("Locomotion");
    }

    // --- YOUR COLLISION FIXES REMAIN INTACT HERE ---
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Player)
        {
            Debug.Log("Guard physically touched the player!");
            GameManager.Instance.PlayerCaught();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Player)
        {
            Debug.Log("Guard's trigger touched the player!");
            GameManager.Instance.PlayerCaught();
        }
    }
}