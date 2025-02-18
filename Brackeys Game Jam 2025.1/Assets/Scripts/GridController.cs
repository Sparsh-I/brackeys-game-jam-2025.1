using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public int gridWidth = 5, gridHeight = 5;
    public float cellSize = 1f;
    public Transform cursor;
    private Vector2Int _cursorPos = Vector2Int.zero;

    private Item _heldItem = null;
    private Item[,] _grid;
    
    // Start is called before the first frame update
    void Start()
    {
        _grid = new Item[gridWidth, gridHeight];
        UpdateCursorPosition();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        if (Input.GetKeyDown(KeyCode.Return)) HandleItemPickupPlacement();
    }

    private void HandleMovement()
    {
        Vector2Int move = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) move.y = 1;
        if (Input.GetKeyDown(KeyCode.A)) move.x = -1;
        if (Input.GetKeyDown(KeyCode.S)) move.y = -1;
        if (Input.GetKeyDown(KeyCode.D)) move.x = 1;
        
        Vector2Int newPos = _cursorPos + move;
        UpdateCursorPosition();
    }

    private void UpdateCursorPosition()
    {
        cursor.position = new Vector3(_cursorPos.x * cellSize, _cursorPos.y * cellSize, 0);
    }
    
    private void HandleItemPickupPlacement()
}
