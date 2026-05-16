using UnityEngine;
using System.Collections.Generic;

public class SniperPuzzleManager : MonoBehaviour
{
    [Header("Player & Cameras")]
    public GameObject playerCharacter;
    public GameObject mainPlayerCamera;
    public GameObject sniperCamera; 

    [Header("Reward Item")]
    [Tooltip("Drag the item you want to appear here")]
    public GameObject rewardItem;

    [Header("Sniper Settings")]
    public Transform firePoint; 
    public LineRenderer laserRenderer; 
    public float laserWidth = 0.02f;

    [Header("Aiming Angles (X, Y, Z)")]
    public Vector3 centerAimAngle = new Vector3(0, 0, 0);
    public Vector3 leftAimAngle = new Vector3(0, -45, 0);
    public Vector3 rightAimAngle = new Vector3(0, 45, 0);

    [Header("Wave & Spawning Settings")]
    public int totalWaves = 7;
    public GameObject whiteFrisbeePrefab;
    public GameObject redFrisbeePrefab;
    public Transform[] spawnPoints; 

    private int currentWave = 0;
    private List<GameObject> spawnedFrisbees = new List<GameObject>();

    private bool isPlayerNear = false;
    private bool isInPuzzleMode = false;
    private bool isPuzzleSolved = false; 

    void Start()
    {
        mainPlayerCamera.gameObject.SetActive(true);
        sniperCamera.gameObject.SetActive(false);
        laserRenderer.enabled = false; 

        // <-- NEW: Hide the reward item at the start of the game -->
        if (rewardItem != null) rewardItem.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNear && !isInPuzzleMode && !isPuzzleSolved && Input.GetKeyDown(KeyCode.E))
        {
            StartPuzzle();
        }

        if (isInPuzzleMode)
        {
            HandleAiming();
            UpdateLaser();

            if (Input.GetMouseButtonDown(0)) 
            {
                Shoot();
            }
            
            if (Input.GetKeyDown(KeyCode.Q))
            {
                EndPuzzle();
            }
        }
    }

    private void StartPuzzle()
    {
        isInPuzzleMode = true;
        currentWave = 0; 
        
        playerCharacter.SetActive(false);
        mainPlayerCamera.gameObject.SetActive(false);
        sniperCamera.gameObject.SetActive(true);
        
        laserRenderer.startWidth = laserWidth;
        laserRenderer.endWidth = laserWidth;
        laserRenderer.enabled = true; 

        SpawnNextWave();
    }

    private void HandleAiming()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.localRotation = Quaternion.Euler(rightAimAngle);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.localRotation = Quaternion.Euler(leftAimAngle);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(centerAimAngle);
        }
    }

    private void UpdateLaser()
    {
        laserRenderer.SetPosition(0, firePoint.position);

        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit))
        {
            laserRenderer.SetPosition(1, hit.point);
        }
        else
        {
            laserRenderer.SetPosition(1, firePoint.position + firePoint.forward * 100f);
        }
    }

    private void Shoot()
    {
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("WhiteFrisbee"))
            {
                Debug.Log("Hit the Correct Target!");
                currentWave++; 

                if (currentWave >= totalWaves)
                {
                    Debug.Log("YOU WIN! 7 in a row complete.");
                    isPuzzleSolved = true; 

                    // <-- NEW: Reveal the reward item immediately upon winning! -->
                    if (rewardItem != null)
                    {
                        rewardItem.SetActive(true);
                    }

                    EndPuzzle();
                }
                else
                {
                    SpawnNextWave(); 
                }
            }
            else if (hit.collider.CompareTag("RedFrisbee"))
            {
                Debug.Log("WRONG Target! Streak broken. Resetting to Wave 1!");
                currentWave = 0; 
                SpawnNextWave(); 
            }
        }
    }

    private void SpawnNextWave()
    {
        foreach (GameObject frisbee in spawnedFrisbees)
        {
            if (frisbee != null) Destroy(frisbee);
        }
        spawnedFrisbees.Clear();

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        int randomWhiteIndex = Random.Range(0, availablePoints.Count);
        Transform whiteSpot = availablePoints[randomWhiteIndex];
        
        GameObject white = Instantiate(whiteFrisbeePrefab, whiteSpot.position, whiteSpot.rotation);
        spawnedFrisbees.Add(white);

        availablePoints.RemoveAt(randomWhiteIndex);

        foreach (Transform redSpot in availablePoints)
        {
            GameObject red = Instantiate(redFrisbeePrefab, redSpot.position, redSpot.rotation);
            spawnedFrisbees.Add(red);
        }
    }

    private void EndPuzzle()
    {
        isInPuzzleMode = false;
        laserRenderer.enabled = false;
        
        transform.localRotation = Quaternion.Euler(centerAimAngle); 

        sniperCamera.gameObject.SetActive(false);
        mainPlayerCamera.gameObject.SetActive(true);
        playerCharacter.SetActive(true);

        foreach (GameObject frisbee in spawnedFrisbees)
        {
            if (frisbee != null) Destroy(frisbee);
        }
        spawnedFrisbees.Clear(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPuzzleSolved) 
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}