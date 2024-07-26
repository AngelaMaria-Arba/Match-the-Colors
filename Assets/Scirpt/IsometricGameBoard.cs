using UnityEngine;
using System.Collections.Generic;

public class IsometricGameBoard : MonoBehaviour
{
    public GameObject cellPrefab;
    public List<GameObject> characterPrefabs;
    public int rows = 9;
    public int columns = 6;
    public float cellWidth = 1.0f;
    public float cellHeight = 0.5f;
    public int numberOfCharactersToSpawn = 9;

    private readonly Color[] charactersColors = { Color.blue, Color.yellow, Color.magenta };
    private readonly Color[] cellsColors = { Color.red, Color.green };
    private static int[,] characterMatrix;
    private static HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private static Dictionary<Vector3, List<Color>> positionColors = new Dictionary<Vector3, List<Color>>(); // New
    private Dictionary<Color, int> movedCharacterColors = new Dictionary<Color, int>(); // Changed to dictionary

    private CharacterManager characterManager;

    void Start()
    {
        characterManager = FindObjectOfType<CharacterManager>();
        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found in the scene.");
        }
        CreateBoard();
        SpawnCharactersOnTiles();
    }

    void CreateBoard()
    {
        Vector3[,] tilePositions = new Vector3[rows, columns];
        Color[,] cellColors = new Color[rows, columns];

        if (characterMatrix == null)
        {
            characterMatrix = new int[rows, columns];
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                float x = col * cellWidth;
                float z = row * cellHeight;
                cell.transform.position = new Vector3(x, -0.8f, z);

                tilePositions[row, col] = cell.transform.position;

                Color color = GetCellColor(row, col, cellColors);
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = color;
                }
                cellColors[row, col] = color;
            }
        }

        SetRowToSpecialValue(rows - 1);
    }

    void SpawnCharactersOnTiles()
    {
        if (characterPrefabs.Count == 0) return;

        int spawnedCharacters = 0;
        int totalColors = charactersColors.Length;
        int charactersPerColor = 3;
        List<Color> colorPool = new List<Color>();

        foreach (Color color in charactersColors)
        {
            for (int i = 0; i < charactersPerColor; i++)
            {
                colorPool.Add(color);
            }
        }

        for (int i = colorPool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Color temp = colorPool[i];
            colorPool[i] = colorPool[j];
            colorPool[j] = temp;
        }

        for (int row = 0; row < rows && spawnedCharacters < numberOfCharactersToSpawn; row++)
        {
            for (int col = 0; col < columns && spawnedCharacters < numberOfCharactersToSpawn; col++)
            {
                Vector3 cellPosition = new Vector3(col * cellWidth, 0, row * cellHeight);
                int randomCharacterIndex = Random.Range(0, characterPrefabs.Count);
                GameObject characterPrefab = characterPrefabs[randomCharacterIndex];

                Vector3 spawnPosition = new Vector3(cellPosition.x, 0, cellPosition.z);
                GameObject character = Instantiate(characterPrefab, spawnPosition, Quaternion.identity, transform);

                Color characterColor = colorPool[spawnedCharacters];
                Renderer characterRenderer = character.GetComponent<Renderer>();
                if (characterRenderer != null)
                {
                    characterRenderer.material.color = characterColor;
                }

                characterMatrix[row, col] = 1;

                IsometricCharacterController controller = character.GetComponent<IsometricCharacterController>();
                if (controller == null)
                {
                    controller = character.AddComponent<IsometricCharacterController>();
                }
                controller.Initialize(this, new Vector2Int(row, col));

                characterManager.RegisterCharacter(controller);

                if (!positionColors.ContainsKey(spawnPosition))
                {
                    positionColors[spawnPosition] = new List<Color>();
                }
                positionColors[spawnPosition].Add(characterColor);

                spawnedCharacters++;
            }
        }
    }

    public void UpdateCharacterMatrix(Vector2Int oldPosition, Vector2Int newPosition)
    {
        // Set the previous position to 0 if it's within the bounds of the board
        if (oldPosition.x >= 0 && oldPosition.x < characterMatrix.GetLength(0) &&
            oldPosition.y >= 0 && oldPosition.y < characterMatrix.GetLength(1))
        {
            characterMatrix[oldPosition.x, oldPosition.y] = 0;

            // Update occupied positions
            Vector3 oldWorldPosition = new Vector3(oldPosition.y * cellWidth, 0, oldPosition.x * cellHeight);
            occupiedPositions.Remove(oldWorldPosition);
        }

        // Set the new position to 1 if it's within the bounds of the board
        if (newPosition.x >= 0 && newPosition.x < characterMatrix.GetLength(0) &&
            newPosition.y >= 0 && newPosition.y < characterMatrix.GetLength(1))
        {
            characterMatrix[newPosition.x, newPosition.y] = 1;

            // Update occupied positions
            Vector3 newWorldPosition = new Vector3(newPosition.y * cellWidth, 0, newPosition.x * cellHeight);
            occupiedPositions.Add(newWorldPosition);
        }

        // Print the character matrix to the console
        PrintCharacterMatrix();
    }

    public Vector2Int GetBoardPositionFromWorldPosition(Vector3 worldPosition)
    {
        int row = Mathf.RoundToInt(worldPosition.z / cellHeight);
        int col = Mathf.RoundToInt(worldPosition.x / cellWidth);
        return new Vector2Int(row, col);
    }

    private void PrintCharacterMatrix()
    {
        string matrixString = "";
        for (int row = 0; row < characterMatrix.GetLength(0); row++)
        {
            for (int col = 0; col < characterMatrix.GetLength(1); col++)
            {
                matrixString += characterMatrix[row, col] + " ";
            }
            matrixString += "\n";
        }
        Debug.Log(matrixString);
    }

    Color GetCellColor(int row, int col, Color[,] cellColors)
    {
        // Get colors of adjacent cells
        Color left = (col > 0) ? cellColors[row, col - 1] : Color.clear;
        Color below = (row > 0) ? cellColors[row - 1, col] : Color.clear;

        // Choose a color different from adjacent cells
        foreach (Color color in cellsColors)
        {
            if (color != left && color != below)
            {
                return color;
            }
        }

        // Default to first color if no other options
        return cellsColors[0];
    }

    // Static method to access characterMatrix from anywhere in the game
    public static int[,] GetCharacterMatrix()
    {
        return characterMatrix;
    }

    // Method to set the nth row of characterMatrix to a special value (e.g., 2)
    public void SetRowToSpecialValue(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rows)
        {
            Debug.LogError("Invalid row index.");
            return;
        }

        for (int col = 0; col < columns; col++)
        {
            characterMatrix[rowIndex, col] = 2;

            // Update the visual appearance (color) of the corresponding cell
            GameObject cell = GetCellAtPosition(new Vector2Int(rowIndex, col));
            if (cell != null)
            {
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white; // Set color to white
                }
            }
        }
    }

    // Static method to check if a position is occupied
    public static bool IsPositionOccupied(Vector3 position)
    {
        return occupiedPositions.Contains(position);
    }

    // Static method to mark a position as occupied
    public static void MarkPositionOccupied(Vector3 position)
    {
        occupiedPositions.Add(position);
    }

    // Helper method to get the cell GameObject at a specific board position
    private GameObject GetCellAtPosition(Vector2Int position)
    {
        Transform boardTransform = transform;
        Vector3 cellPosition = new Vector3(position.y * cellWidth, -0.8f, position.x * cellHeight);
        foreach (Transform child in boardTransform)
        {
            if (child.position == cellPosition)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    public bool CheckCharactersInRow(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= rows)
        {
            Debug.LogError("Invalid row index.");
            return false;
        }

        for (int col = 0; col < columns; col++)
        {
            if (characterMatrix[rowIndex, col] == 1)
            {
                // Do something if needed
            }

            // Update the visual appearance (color) of the corresponding cell
            GameObject cell = GetCellAtPosition(new Vector2Int(rowIndex, col));
            if (cell != null)
            {
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.white; // Set color to white
                }
            }
        }

        return false;
    }

    public Dictionary<Vector3, List<Color>> GetPositionColors()
    {
        return positionColors;
    }

    public void AddMovedCharacterColor(Color color)
    {
        if (movedCharacterColors.ContainsKey(color))
        {
            movedCharacterColors[color]++;
        }
        else
        {
            movedCharacterColors[color] = 1;
        }

        // Check if any color has reached the count of 3
        if (movedCharacterColors[color] == 3)
        {
            DestroyCharactersByColor(color);
        }
    }

    private void DestroyCharactersByColor(Color color)
    {
        // Destroy characters with the specified color
        foreach (Transform child in transform)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null && renderer.material.color == color)
            {
                Destroy(child.gameObject);
            }
        }

        // Reset the character matrix and movedCharacterColors for the destroyed color
        ResetCharacterMatrixAndColorCount(color);
    }

    private void ResetCharacterMatrixAndColorCount(Color color)
    {
        // Reset character matrix
        for (int row = 0; row < characterMatrix.GetLength(0); row++)
        {
            for (int col = 0; col < characterMatrix.GetLength(1); col++)
            {
                if (characterMatrix[row, col] == 1)
                {
                    Vector3 cellPosition = new Vector3(col * cellWidth, 0, row * cellHeight);
                    if (IsPositionOccupied(cellPosition))
                    {
                        characterMatrix[row, col] = 0;
                        occupiedPositions.Remove(cellPosition);
                    }
                }
            }
        }

        // Reset color count in movedCharacterColors
        if (movedCharacterColors.ContainsKey(color))
        {
            movedCharacterColors[color] = 0;
        }
    }

    public Dictionary<Color, int> GetMovedCharacterColors()
    {
        return movedCharacterColors;
    }

    public void PrintMovedCharacterColors()
    {
        string colorsString = "Moved Character Colors: ";
        foreach (var entry in movedCharacterColors)
        {
            colorsString += $"{entry.Key} (Count: {entry.Value}), ";
        }
        Debug.Log(colorsString.TrimEnd(',', ' '));
    }
}
