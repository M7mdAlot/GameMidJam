using UnityEngine;
using System.Collections; 

public class ChessPuzzleManager : MonoBehaviour
{
    [Header("Player & Cameras")]
    public GameObject playerCharacter; 
    public UnityEngine.Camera mainPlayerCamera;
    public UnityEngine.Camera topDownCamera;

    [Header("Puzzle Settings")]
    public Collider boardTriggerZone;

    // <-- NEW: The item that appears when you win -->
    [Header("Reward Item")]
    [Tooltip("Drag the item you want to appear here")]
    public GameObject rewardItem; 

    [Header("Visuals / Highlights")]
    public GameObject[] highlightBlocks; 
    public Color highlightColor = Color.green;
    
    private Color[] originalColors; 
    private Renderer[] blockRenderers;

    [Header("Audio")]
    public AudioSource audioSpeaker;
    public AudioClip startPuzzleSound; 
    public AudioClip wrongPieceSound;
    public AudioClip wrongSquareSound;
    public AudioClip checkmateSound;

    private bool isPlayerNearBoard = false;
    private bool isInPuzzleMode = false;
    private bool isPieceSelected = false;
    private bool isPuzzleSolved = false; 

    private GameObject selectedPiece;

    void Start()
    {
        if (playerCharacter != null) playerCharacter.SetActive(true);
        mainPlayerCamera.gameObject.SetActive(true);
        topDownCamera.gameObject.SetActive(false);

        // Hide the reward item at the very beginning of the game
        if (rewardItem != null) rewardItem.SetActive(false);

        SetupHighlightBlocks();
    }

    void Update()
    {
        if (isPlayerNearBoard && !isInPuzzleMode && !isPuzzleSolved && Input.GetKeyDown(KeyCode.E))
        {
            StartPuzzleMode();
        }

        if (isInPuzzleMode && Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void SetupHighlightBlocks()
    {
        blockRenderers = new Renderer[highlightBlocks.Length];
        originalColors = new Color[highlightBlocks.Length];

        for (int i = 0; i < highlightBlocks.Length; i++)
        {
            if (highlightBlocks[i] != null)
            {
                blockRenderers[i] = highlightBlocks[i].GetComponent<Renderer>();
                originalColors[i] = blockRenderers[i].material.color;
            }
        }
    }

    private void TurnBlocksGreen()
    {
        for (int i = 0; i < blockRenderers.Length; i++)
        {
            if (blockRenderers[i] != null) blockRenderers[i].material.color = highlightColor;
        }
    }

    private void ResetBlockColors()
    {
        for (int i = 0; i < blockRenderers.Length; i++)
        {
            if (blockRenderers[i] != null) blockRenderers[i].material.color = originalColors[i];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPuzzleSolved)
        {
            isPlayerNearBoard = true;
            Debug.Log("Press E to play chess.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearBoard = false;
        }
    }

    private void StartPuzzleMode()
    {
        isInPuzzleMode = true;
        
        if (boardTriggerZone != null) boardTriggerZone.enabled = false;
        if (playerCharacter != null) playerCharacter.SetActive(false);

        mainPlayerCamera.gameObject.SetActive(false);
        topDownCamera.gameObject.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (startPuzzleSound != null) audioSpeaker.PlayOneShot(startPuzzleSound);
    }

    private void HandleMouseClick()
    {
        Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("WinningPiece"))
            {
                selectedPiece = clickedObject;
                isPieceSelected = true;
                TurnBlocksGreen();
            }
            else if (clickedObject.CompareTag("WinningSquare") && isPieceSelected)
            {
                ExecuteWinningMove(clickedObject.transform);
            }
            else
            {
                if (isPieceSelected)
                {
                    if (wrongSquareSound != null) audioSpeaker.PlayOneShot(wrongSquareSound);
                }
                else
                {
                    if (wrongPieceSound != null) audioSpeaker.PlayOneShot(wrongPieceSound);
                }
                
                isPieceSelected = false; 
                ResetBlockColors();
            }
        }
    }

    private void ExecuteWinningMove(Transform targetSquare)
    {
        Collider squareCollider = targetSquare.GetComponent<Collider>();
        Vector3 newPos = squareCollider.bounds.center;
        newPos.y = selectedPiece.transform.position.y; 
        
        selectedPiece.transform.position = newPos;

        if (checkmateSound != null) audioSpeaker.PlayOneShot(checkmateSound);

        // <-- NEW: Reveal the item immediately upon winning! -->
        if (rewardItem != null)
        {
            rewardItem.SetActive(true);
        }

        ResetBlockColors();
        isPuzzleSolved = true; 

        StartCoroutine(EndPuzzleRoutine());
    }

    private IEnumerator EndPuzzleRoutine()
    {
        yield return new WaitForSeconds(5f);

        if (playerCharacter != null) playerCharacter.SetActive(true);
        
        topDownCamera.gameObject.SetActive(false);
        mainPlayerCamera.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isInPuzzleMode = false; 
        isPlayerNearBoard = false; 
    }
}