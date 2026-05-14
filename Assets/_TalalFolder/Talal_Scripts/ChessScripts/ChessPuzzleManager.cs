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

    // <-- NEW: Visual Highlight Settings -->
    [Header("Visuals / Highlights")]
    [Tooltip("Drag the 4 specific square GameObjects here")]
    public GameObject[] highlightBlocks; 
    public Color highlightColor = Color.green;
    
    // We use this to remember the original color of the blocks so we can change them back
    private Color[] originalColors; 
    private Renderer[] blockRenderers;

    [Header("Audio")]
    public AudioSource audioSpeaker;
    public AudioClip startPuzzleSound; 
    public AudioClip wrongPieceSound;
    public AudioClip wrongSquareSound;
    public AudioClip checkmateSound;

    [Header("Animations")]
    public Animator firstAnimator;
    public string firstTriggerName = "CheckmateTrigger1";

    public Animator secondAnimator;
    public string secondTriggerName = "CheckmateTrigger2";

    public Animator thirdAnimator;
    public string thirdTriggerName = "CheckmateTrigger3";

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

        // <-- NEW: Prepare the blocks for highlighting -->
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

    // <-- NEW: Grabs the renderers and saves the original white/black colors -->
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

    // <-- NEW: Turns the blocks green -->
    private void TurnBlocksGreen()
    {
        for (int i = 0; i < blockRenderers.Length; i++)
        {
            if (blockRenderers[i] != null)
            {
                blockRenderers[i].material.color = highlightColor;
            }
        }
    }

    // <-- NEW: Reverts the blocks back to normal -->
    private void ResetBlockColors()
    {
        for (int i = 0; i < blockRenderers.Length; i++)
        {
            if (blockRenderers[i] != null)
            {
                blockRenderers[i].material.color = originalColors[i];
            }
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
                Debug.Log("Good piece selected! Now click the winning square.");
                
                // <-- NEW: Make the squares turn green! -->
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
                    Debug.Log("Wrong square!");
                    if (wrongSquareSound != null) audioSpeaker.PlayOneShot(wrongSquareSound);
                }
                else
                {
                    Debug.Log("Wrong piece!");
                    if (wrongPieceSound != null) audioSpeaker.PlayOneShot(wrongPieceSound);
                }
                
                isPieceSelected = false; 
                // <-- NEW: If they mess up, turn the blocks back to normal -->
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

        Debug.Log("CHECKMATE! You win!");
        
        if (checkmateSound != null) audioSpeaker.PlayOneShot(checkmateSound);

        if (firstAnimator != null) firstAnimator.SetTrigger(firstTriggerName);
        if (secondAnimator != null) secondAnimator.SetTrigger(secondTriggerName);
        if (thirdAnimator != null) thirdAnimator.SetTrigger(thirdTriggerName); 

        // <-- NEW: Turn the blocks back to normal now that the move is done -->
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