using System;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class GridController : MonoBehaviour
{
    [SerializeField] private int gridWidth, gridHeight;
    [SerializeField] private float cellSize;
    [SerializeField] private float offset;
    private Vector3 gridOffset;
    
    public Transform cursor;
    private Vector2Int _cursorPos = Vector2Int.zero;
    [SerializeField] private Transform highlight;
    
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private int spawnRow;

    private Item _heldItem;
    private Item[,] _grid;
    
    // Start is called before the first frame update
    void Start()
    {
        _grid = new Item[gridWidth, gridHeight];
        UpdateCursorPosition();
        SpawnItems();
        ScoreManager.Instance.AddScore(0);
    }

    // Update is called once per frame
    void Update()
    {
        // LogGrid();
        bool validPos = (_cursorPos.y < 4 | _cursorPos.y == 5) & 
                        !((_cursorPos.x % 2 == 1 | _cursorPos.x == 6) 
                          & _cursorPos.y == 5);
        HandleMovement();
        if (Input.GetKeyDown(KeyCode.K) & validPos) HandleItemPickupPlacement();
        
        if (IsRowEmpty(spawnRow)) SpawnItems();

        if (validPos) highlight.gameObject.SetActive(_heldItem);

        if (IsGridFull(0, 3))
        {
            ScoreManager.Instance.AddScore(1000);
            ClearEntireGrid();
        }
    }

    private bool IsGridFull(int startRow, int endRow)
    {
        for (int row = startRow; row <= endRow; row++)
            for (int x = 0; x < gridWidth; x++)
                if (!_grid[x, row]) return false; // If any cell is empty, rows are not full
        
        return true; // All rows in the range are full
    }

    private void ClearEntireGrid()
    {
        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (_grid[x, y])
                {
                    Destroy(_grid[x, y].gameObject);
                    _grid[x, y] = null;
                }
    }
    
    private bool IsRowEmpty(int row)
    {
        for (int x = 0; x < gridWidth; x++)
            if (_grid[x, row]) return false;
        return true;
    }
    
    private void HandleMovement()
    {
        Vector2Int move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) move.y = 1;
        if (Input.GetKeyDown(KeyCode.A)) move.x = -1;
        if (Input.GetKeyDown(KeyCode.S)) move.y = -1;
        if (Input.GetKeyDown(KeyCode.D)) move.x = 1;
        
        Vector2Int newPos = _cursorPos + move;
        // Check if the item would fit in the new position
        if (_heldItem)
        {
            // Check if the new position can accommodate the size of the held item
            if (move.x != 0) newPos.x = Mathf.Clamp(newPos.x, 0, gridWidth - _heldItem.size.x);
            if (move.y != 0) newPos.y = Mathf.Clamp(newPos.y, 0, gridHeight - _heldItem.size.y);
        }

        if ((IsInsideGrid(newPos) && (!_heldItem || CanPlaceItem(_heldItem, newPos))) || _cursorPos.y >= spawnRow - 1)
        {
            _cursorPos = newPos;
            UpdateCursorPosition();
        }
    }

    private void UpdateCursorPosition()
    {
        gridOffset = new Vector3(-3 * cellSize, -3 * cellSize, 0);
        cursor.position = new Vector3(_cursorPos.x * cellSize, _cursorPos.y * cellSize, 0) + gridOffset;

        if (highlight)
        {
            // If we have a held item, we need to calculate the bounding box
            if (_heldItem)
            {
                // Offset to align the item to the top-left (subtracting half the width/height if necessary)
                float offsetX = (_heldItem.size.x != 1) ? (cellSize / _heldItem.size.x) : 0;
                float offsetY = (_heldItem.size.y != 1) ? (cellSize / _heldItem.size.y) : 0;

                // Update the position of the highlight to align with the top-left of the item
                highlight.position = cursor.position + new Vector3(offsetX, 0-offsetY, 0);
            }
            else
            {
                // If no item is held, reset the highlight size to zero (hidden)
                highlight.localScale = Vector3.zero;
            }
        }
    }
    
    private bool CanPlaceItem(Item item, Vector2Int newPos)
    {
        // Get the new occupied cells based on the new position
        var newOccupiedCells = item.shape.Select(cell => new Vector2Int(cell.x + newPos.x, cell.y + newPos.y)).ToArray();

        // Check if any of the new occupied cells are out of bounds or already occupied
        foreach (var cell in newOccupiedCells)
            // Check if occupied or out of bounds
            if (!IsInsideGrid(cell) || _grid[cell.x, cell.y]) return false;

        return true; // The position is valid
    }

   private void HandleItemPickupPlacement()
    {
        if (_heldItem)  // Currently holding an item
        {
            // Check if the item can be placed at the current position
            if (CanPlaceItem(_heldItem, _cursorPos))
            {
                // Place the item on the grid
                foreach (var cell in _heldItem.occupiedCells)
                {
                    Vector2Int targetCell = _cursorPos + cell;
                    _grid[targetCell.x, targetCell.y] = _heldItem;  // Mark the grid with the held item
                }
                
                // Set the scale based on occupied cells
                Vector3 scale = GetItemScale(_heldItem);

                float offsetX = (_heldItem.size.x != 1) ? (cellSize / _heldItem.size.x) : 0;
                float offsetY = (_heldItem.size.y != 1) ? (cellSize / _heldItem.size.y) : 0;
                
                // Set the position of the held item (cursor position)
                _heldItem.transform.position = cursor.position + new Vector3(offsetX, 0-offsetY, 0);
                _heldItem.transform.localScale = scale;
                _heldItem.gameObject.SetActive(true);
                _heldItem.topLeftPosition = _cursorPos;
                _heldItem.beenPlaced = true;

                // Reset the held item to null
                _heldItem = null;

                // Update the highlight scale if no item is held
                if (!_heldItem && highlight) highlight.localScale = Vector3.zero;
            }
        }
        else  // Not holding an item
        {
            if (_grid[_cursorPos.x, _cursorPos.y])  // Hovering over an item
            {
                _heldItem = _grid[_cursorPos.x, _cursorPos.y];

                Vector2Int startingPos = _heldItem.beenPlaced ? _heldItem.topLeftPosition : _cursorPos;
                
                // Free the cells the item previously occupied (so they can be reused)
                foreach (var cell in _heldItem.occupiedCells)
                {
                    Vector2Int targetCell = startingPos + cell;
                    _grid[targetCell.x, targetCell.y] = null; // Free the space in the grid
                }

                // Deactivate the item temporarily
                _heldItem.gameObject.SetActive(false);

                // Clear the grid cell where the item was originally placed
                _grid[_cursorPos.x, _cursorPos.y] = null;

                // Update the highlight scale based on the held item's shape
                if (_heldItem)
                {
                    highlight.transform.localScale = GetItemScale(_heldItem);
                }
            }
        }
    }

    private Vector3 GetItemScale(Item item)
    {
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        // Find the bounding box of the occupied cells
        foreach (var cell in item.occupiedCells)
        {
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxY = Mathf.Max(maxY, cell.y);
        }

        // Calculate width and height from the bounding box
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        // Return the scale based on the bounding box size
        return new Vector3(width * cellSize, height * cellSize, 1);
    }
    
    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && 
               pos.y >= 0 && pos.y < gridHeight;
    }

    private void SpawnItems()
    {
        for (int i = 0; i < 6; i += 2)
        {
            int randomIdx = Random.Range(0, itemPrefabs.Length);
            GameObject itemPrefab = itemPrefabs[randomIdx];

            Vector3 worldPos = GridToWorldPosition(i, spawnRow);
            GameObject newItem = Instantiate(itemPrefab, worldPos, Quaternion.identity);
            newItem.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            _grid[i, spawnRow] = newItem.GetComponent<Item>();
        }
    }

    public void RerollItems()
    {
        for (int x = 0; x < gridWidth; x++)
            if (_grid[x, spawnRow])
            {
                Destroy(_grid[x, spawnRow].gameObject);
                _grid[x, spawnRow] = null;
            }
        ScoreManager.Instance.AddScore(-300);
    }
    
    private Vector3 GridToWorldPosition(int x, int y)
    {
        return gridOffset + new Vector3(x * cellSize, y * cellSize, 0);
    }
}
