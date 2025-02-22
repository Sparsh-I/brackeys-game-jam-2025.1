using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    public List<Vector2Int> shape; // List of coordinates occupied by the item
    public Vector2Int size; // The bounding box size (width, height)
    public Vector2Int[] occupiedCells;
    public Vector2Int topLeftPosition;
    public bool beenPlaced = false;

    // Constructor for custom shapes, allowing manual setup of the shape
    public Item(List<Vector2Int> shape)
    {
        this.shape = shape;
        this.size = new Vector2Int(
            shape.Count > 0 ? shape.Max(coord => coord.x) + 1 : 0,
            shape.Count > 0 ? shape.Max(coord => coord.y) + 1 : 0
        );
    }
    
    public void SetPosition(Vector2Int newPos)
    {
        // Set the position of the item to the new position
        this.transform.position = new Vector3(newPos.x, newPos.y, 0);
    }
}