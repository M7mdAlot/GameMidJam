using UnityEngine;
using System.Collections.Generic;

public class SniperPuzzleManager : MonoBehaviour
{
    [Header("Player & Cameras")]
    public GameObject playerCharacter;
    public Camera mainPlayerCamera;
    public Camera sniperCamera; 

    [Header("Sniper Settings")]
    public Transform firePoint; 
    public LineRenderer laserRenderer; 
    public float rotationAngle = 45f; 

    [Header("Wave & Spawning Settings")]
    public int totalWaves = 7;
    public GameObject whiteFrisbeePrefab;
    public GameObject redFrisbeePrefab;
    
    [Tooltip("Drag your 3 empty GameObjects here (Left, Center, Right)")]
    public Transform[] spawnPoints; 

    private int currentWave = 0;
    private List<GameObject> spawnedFrisbees = new List<GameObject>();

    private bool isPlayerNear = false;
    private bool isInPuzzleMode = false;
    private Quaternion centerRotation;

    void Start()
    {
        mainPlayerCamera.gameObject.SetActive(true);
        sniperCamera.gameObject.SetActive(false);
        laserRenderer.enabled = false; 
        centerRotation = transform.rotation;
    }

    void Update()
    {
        if (isPlayerNear && !isInPuzzleMode && Input.GetKeyDown(KeyCode.E))
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
        }
    }

    private void StartPuzzle()
    {
        isInPuzzleMode = true;
        currentWave = 0; 
        
        playerCharacter.SetActive(false);
        mainPlayerCamera.gameObject.SetActive(false);
        sniperCamera.gameObject.SetActive(true);
        laserRenderer.enabled = true; 

        SpawnNextWave();
    }

    private void HandleAiming()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = centerRotation * Quaternion.Euler(0, rotationAngle, 0);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = centerRotation * Quaternion.Euler(0, -rotationAngle, 0);
        }
        else
        {
            transform.rotation = centerRotation;
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
                    Debug.Log("YOU WIN! All 7 waves complete.");
                    EndPuzzle();
                }
                else
                {
                    SpawnNextWave(); 
                }
            }
            else if (hit.collider.CompareTag("RedFrisbee"))
            {
                Debug.Log("WRONG Target! Penalty!");
                Destroy(hit.collider.gameObject); 
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
        transform.rotation = centerRotation; 

        sniperCamera.gameObject.SetActive(false);
        mainPlayerCamera.gameObject.SetActive(true);
        playerCharacter.SetActive(true);
        isPlayerNear = false;

        foreach (GameObject frisbee in spawnedFrisbees)
        {
            if (frisbee != null) Destroy(frisbee);
        }
        spawnedFrisbees.Clear(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}