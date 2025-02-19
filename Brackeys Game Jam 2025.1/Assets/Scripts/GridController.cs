using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.VFX;

public class GridController : MonoBehaviour
{
    [SerializeField] private int gridWidth, gridHeight;
    [SerializeField] private float cellSize;
    [SerializeField] private float offset;
    public Transform cursor;
    private Vector2Int _cursorPos = Vector2Int.zero;
    
    [SerializeField] private GameObject[] itemPrefabs;

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
        HandleMovement();
        if (Input.GetKeyDown(KeyCode.Return)) HandleItemPickupPlacement();
        
        if (IsRowEmpty(6)) SpawnItems();
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
        Vector3 gridOffset = new Vector3(-3 * cellSize, -3 * cellSize, 0);
        cursor.position = new Vector3(_cursorPos.x * cellSize, _cursorPos.y * cellSize, 0) + gridOffset;
    }

    private void HandleItemPickupPlacement()
    {
        if (_heldItem)
        {
            if (!_grid[_cursorPos.x, _cursorPos.y])
            {
                _grid[_cursorPos.x, _cursorPos.y] = _heldItem;
                _heldItem.transform.position = cursor.position;
                _heldItem.gameObject.SetActive(true);
                _heldItem = null;
            }
        }
        else
        {
            if (_grid[_cursorPos.x, _cursorPos.y])
            {
                _heldItem = _grid[_cursorPos.x, _cursorPos.y];
                _grid[_cursorPos.x, _cursorPos.y] = null;
                _heldItem.gameObject.SetActive(false);
            }
        }
    }
    
    private bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && 
               pos.y >= 0 && pos.y < gridHeight;
    }

    private void SpawnItems()
    {
        for (int i = 1; i < 6; i += 2)
        {
            if (!_grid[i, 6])
            {
                int randomIdx = Random.Range(0, itemPrefabs.Length);
                GameObject itemPrefab = itemPrefabs[randomIdx];

                Vector3 worldPos = GridToWorldPosition(i, 6);
                GameObject newItem = Instantiate(itemPrefab, worldPos, Quaternion.identity);
                _grid[i, 6] = newItem.GetComponent<Item>();
            }
        }
    }
    
    private Vector3 GridToWorldPosition(int x, int y)
    {
        return transform.position + new Vector3(x * cellSize, y * cellSize, 0);
    }
}
