using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public List<Vector3> fixedPositions = new List<Vector3>();
    private List<IsometricCharacterController> characters = new List<IsometricCharacterController>();
    private IsometricCharacterController selectedCharacter;

    private IsometricGameBoard gameBoard;

    void Start()
    {
        gameBoard = FindObjectOfType<IsometricGameBoard>(); // Get the instance of IsometricGameBoard
        if (gameBoard == null)
        {
            Debug.LogError("IsometricGameBoard not found in the scene.");
            return; // Exit if gameBoard is not found
        }

        InitializeFixedPositions();
    }

    void InitializeFixedPositions()
    {
        fixedPositions.Add(new Vector3(1, 0, 8));
        fixedPositions.Add(new Vector3(2, 0, 8));
        fixedPositions.Add(new Vector3(3, 0, 8));
        fixedPositions.Add(new Vector3(4, 0, 8));
        fixedPositions.Add(new Vector3(5, 0, 8));
    }

    public void RegisterCharacter(IsometricCharacterController character)
    {
        if (character == null) return;

        characters.Add(character);
        AssignTargetPosition(character);

        // Subscribe to movement completion event
        character.OnMovementComplete += HandleMovementComplete;
    }

    public void HandleCharacterClick(IsometricCharacterController clickedCharacter)
    {
        if (clickedCharacter == null) return;

        // Deselect previous character if any
        if (selectedCharacter != null)
        {
            selectedCharacter.SetTargetPosition(Vector3.zero); // or any default position
        }

        selectedCharacter = clickedCharacter;
        AssignTargetPosition(selectedCharacter);
    }

    void AssignTargetPosition(IsometricCharacterController character)
    {
        if (selectedCharacter == character)
        {
            foreach (Vector3 position in fixedPositions)
            {
                if (!IsometricGameBoard.IsPositionOccupied(position))
                {
                    character.SetTargetPosition(position);
                    IsometricGameBoard.MarkPositionOccupied(position);
                    break;
                }
            }
        }
    }

    private void HandleMovementComplete(IsometricCharacterController character)
    {
        if (character == null) return;

        // Determine the row index of the character
        Vector2Int position = character.GetBoardPosition();
        int rowIndex = position.x;

        // Implement any additional logic based on the row index if needed
        // For example, you could update some game state or UI element here
    }
}