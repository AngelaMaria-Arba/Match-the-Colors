using UnityEngine;

public class IsometricCharacterController : MonoBehaviour
{
    private Vector3 targetPosition;
    private bool movingToFixedPoint = false;
    private IsometricGameBoard gameBoard;
    private Vector2Int boardPosition; // Position of the character on the board

    public float moveSpeed = 5f; // Speed of movement

    public delegate void MovementCompleteHandler(IsometricCharacterController character);
    public event MovementCompleteHandler OnMovementComplete;

    // Initialize the controller with the game board reference and board position
    public void Initialize(IsometricGameBoard board, Vector2Int position)
    {
        gameBoard = board;
        boardPosition = position;
    }

    void Update()
    {
        if (movingToFixedPoint)
        {
            MoveTowardsFixedPoint();
        }
    }

    public void SetTargetPosition(Vector3 position)
    {
        if (movingToFixedPoint)
        {
            // Cancel the current movement
            CancelMovement();
        }

        targetPosition = position;
        movingToFixedPoint = position != Vector3.zero; // Only start moving if the target is not the default position
    }

    private void CancelMovement()
    {
        movingToFixedPoint = false;
        transform.position = targetPosition;
        gameBoard.UpdateCharacterMatrix(boardPosition, new Vector2Int(-1, -1)); // Indicate cancellation
        OnMovementComplete?.Invoke(this);
    }

    void MoveTowardsFixedPoint()
    {
        Vector3 currentPosition = transform.position;

        // Determine if movement should be along X or Z axis
        bool moveAlongX = Mathf.Abs(targetPosition.x - currentPosition.x) > Mathf.Abs(targetPosition.z - currentPosition.z);
        bool moveAlongZ = !moveAlongX;

        if (moveAlongX)
        {
            if (Mathf.Abs(targetPosition.x - currentPosition.x) > 0.1f)
            {
                // Move along the X axis
                transform.position = Vector3.MoveTowards(currentPosition, new Vector3(targetPosition.x, currentPosition.y, currentPosition.z), Time.deltaTime * moveSpeed);
            }
            else
            {
                // Switch to Z axis movement
                if (Mathf.Abs(targetPosition.z - currentPosition.z) > 0.1f)
                {
                    // Move along the Z axis
                    transform.position = Vector3.MoveTowards(currentPosition, new Vector3(currentPosition.x, currentPosition.y, targetPosition.z), Time.deltaTime * moveSpeed);
                }
                else
                {
                    // Stop moving when the target position is reached
                    FinalizeMovement();
                }
            }
        }
        else if (moveAlongZ)
        {
            if (Mathf.Abs(targetPosition.z - currentPosition.z) > 0.1f)
            {
                // Move along the Z axis
                transform.position = Vector3.MoveTowards(currentPosition, new Vector3(currentPosition.x, currentPosition.y, targetPosition.z), Time.deltaTime * moveSpeed);
            }
            else
            {
                // Switch to X axis movement
                if (Mathf.Abs(targetPosition.x - currentPosition.x) > 0.1f)
                {
                    // Move along the X axis
                    transform.position = Vector3.MoveTowards(currentPosition, new Vector3(targetPosition.x, currentPosition.y, currentPosition.z), Time.deltaTime * moveSpeed);
                }
                else
                {
                    // Stop moving when the target position is reached
                    FinalizeMovement();
                }
            }
        }
    }

    private void FinalizeMovement()
    {
        // Stop moving and finalize position
        transform.position = targetPosition;
        movingToFixedPoint = false;

        // Calculate new board position
        Vector2Int newBoardPosition = gameBoard.GetBoardPositionFromWorldPosition(targetPosition);

        // Update the character matrix in IsometricGameBoard
        gameBoard.UpdateCharacterMatrix(boardPosition, newBoardPosition);


        // Add the color of the moved character to the movedCharacterColors list
        Renderer characterRenderer = GetComponent<Renderer>();
        if (characterRenderer != null)
        {
            Color characterColor = characterRenderer.material.color;
            gameBoard.AddMovedCharacterColor(characterColor);
        }

        // Update the current board position
        boardPosition = newBoardPosition;

        // Notify that movement is complete
        OnMovementComplete?.Invoke(this);
        
        // Print moved character colors
        gameBoard.PrintMovedCharacterColors();
    }

    void OnMouseDown()
    {
        // Notify the CharacterManager that this character was clicked
        CharacterManager characterManager = FindObjectOfType<CharacterManager>();
        if (characterManager != null)
        {
            characterManager.HandleCharacterClick(this);
        }
        else
        {
            Debug.LogError("CharacterManager not found in the scene.");
        }
    }

    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }
}
