using UnityEngine;

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
    }

    // Update is called once per frame
    void Update()
    {
        bool validPos = (_cursorPos.y < 4 | _cursorPos.y == 5) & 
                        !((_cursorPos.x % 2 == 1 | _cursorPos.x == 6) 
                          & _cursorPos.y == 5);
        HandleMovement();
        if (Input.GetKeyDown(KeyCode.Return) & validPos) HandleItemPickupPlacement();
        
        if (IsRowEmpty(spawnRow)) SpawnItems();

        if (validPos) highlight.gameObject.SetActive(_heldItem);
        // UpdateHighlight();
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
        if (IsInsideGrid(newPos)) 
        {
            _cursorPos = newPos;
            UpdateCursorPosition();
        }
    }

    private void UpdateCursorPosition()
    {
        gridOffset = new Vector3(-3 * cellSize, -3 * cellSize, 0);
        cursor.position = new Vector3(_cursorPos.x * cellSize, _cursorPos.y * cellSize, 0) + gridOffset;

        if (highlight) highlight.position = cursor.position;
    }

    /* private bool CanPlaceItem(Item item, Vector2Int cursorPos)
    {
        foreach (var cell in item.occupiedCells)
        {
            Vector2Int targetCell = cell + cursorPos;
            if (targetCell.x < 0 
                || targetCell.x >= gridWidth
                || targetCell.y < 0 
                || targetCell.y > spawnRow - 2 
                || _grid[targetCell.x, targetCell.y])
                return false;
        }

        return true;
    } */

    private void HandleItemPickupPlacement()
    {
        if (_heldItem)                                  // currently holding an item
        {
            if (!_grid[_cursorPos.x, _cursorPos.y])     // hovering over an empty space
            {
                /* if (CanPlaceItem(_heldItem, _cursorPos))
                {
                    foreach (var cell in _heldItem.occupiedCells)
                    {
                        Vector2Int targetCell = cell + _cursorPos;
                        _grid[targetCell.x, targetCell.y] = _heldItem;
                    }
                } */
                _heldItem.transform.position = cursor.position;
                _grid[_cursorPos.x, _cursorPos.y] = _heldItem;
                if (_cursorPos.y == spawnRow) 
                    _heldItem.transform.localScale = new Vector3(0.5f, 0.5f, 1);
                else
                    _heldItem.transform.localScale = new Vector3(cellSize, cellSize, 1);
                _heldItem.gameObject.SetActive(true);
                _heldItem = null;
                
                if (_heldItem) highlight.transform.localScale = new Vector3(_heldItem.width, _heldItem.height, 1);
            }
        }
        else                                            // NOT holding an item
        {
            if (_grid[_cursorPos.x, _cursorPos.y])      // hovering over an item
            {
                _heldItem = _grid[_cursorPos.x, _cursorPos.y];
                /* foreach (var cell in _heldItem.occupiedCells)
                {
                    Vector2Int targetCell = _cursorPos + cell;
                    _grid[targetCell.x, targetCell.y] = null;
                } */
                _heldItem.gameObject.SetActive(false);
                _grid[_cursorPos.x, _cursorPos.y] = null;
                
                if (_heldItem) highlight.transform.localScale = new Vector3(_heldItem.width, _heldItem.height, 1);
            }
        }
    }

    /* private void UpdateHighlight()
    {
        if (_heldItem)
        {
            int minWidth = 1, minHeight = 1;
            foreach (var cell in _heldItem.occupiedCells)
            {
                maxWidth = Mathf.Max(minWidth, cell.x + 1);
                maxHeight = Mathf.Max(minHeight, cell.y + 1);
            }

            highlight.localScale = new Vector3(minWidth * cellSize, minHeight * cellSize, 1);

            highlight.position = GridToWorldPosition(_cursorPos.x, _cursorPos.y);
        }
        else
        {
            highlight.localScale = Vector3.zero; // no highlight if no item is held
        }
    } */
    
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
    
    private Vector3 GridToWorldPosition(int x, int y)
    {
        return gridOffset + new Vector3(x * cellSize, y * cellSize, 0);
    }
}
