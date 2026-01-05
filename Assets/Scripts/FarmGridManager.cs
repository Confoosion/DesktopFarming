using UnityEngine;
using System.Collections.Generic;

public class FarmGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int initialWidth = 3;
    public int initialHeight = 3;
    public float tileSize = 1.1f;
    public GameObject tilePrefab;
    
    [Header("Grid State")]
    public int currentWidth;
    public int currentHeight;
    
    private Dictionary<Vector2Int, FarmTile> gridTiles = new Dictionary<Vector2Int, FarmTile>();
    
    private void Start()
    {
        InitializeGrid(initialWidth, initialHeight);
    }
    
    private void InitializeGrid(int width, int height)
    {
        currentWidth = width;
        currentHeight = height;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateTile(x, y, true); // Start with unlocked tiles
            }
        }
        
        CenterCamera();
    }
    
    private void CreateTile(int x, int y, bool unlocked = false)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        
        // Don't create if already exists
        if (gridTiles.ContainsKey(gridPos))
            return;
        
        Vector3 worldPos = GridToWorldPosition(x, y);
        GameObject tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
        tileObj.name = $"Tile_{x}_{y}";
        
        FarmTile tile = tileObj.GetComponent<FarmTile>();
        if (tile != null)
        {
            tile.isUnlocked = unlocked;
        }
        
        gridTiles[gridPos] = tile;
    }
    
    public void ExpandGrid(int direction)
    {
        // direction: 0=right, 1=top, 2=left, 3=bottom
        switch (direction)
        {
            case 0: // Expand right
                for (int y = 0; y < currentHeight; y++)
                {
                    CreateTile(currentWidth, y, false);
                }
                currentWidth++;
                break;
                
            case 1: // Expand top
                for (int x = 0; x < currentWidth; x++)
                {
                    CreateTile(x, currentHeight, false);
                }
                currentHeight++;
                break;
                
            case 2: // Expand left
                // Shift existing tiles right in dictionary
                Dictionary<Vector2Int, FarmTile> newDict = new Dictionary<Vector2Int, FarmTile>();
                foreach (var kvp in gridTiles)
                {
                    Vector2Int newPos = new Vector2Int(kvp.Key.x + 1, kvp.Key.y);
                    kvp.Value.transform.position = GridToWorldPosition(newPos.x, newPos.y);
                    kvp.Value.name = $"Tile_{newPos.x}_{newPos.y}";
                    newDict[newPos] = kvp.Value;
                }
                gridTiles = newDict;
                
                // Create new column at x=0
                for (int y = 0; y < currentHeight; y++)
                {
                    CreateTile(0, y, false);
                }
                currentWidth++;
                break;
                
            case 3: // Expand bottom
                // Shift existing tiles up in dictionary
                Dictionary<Vector2Int, FarmTile> newDict2 = new Dictionary<Vector2Int, FarmTile>();
                foreach (var kvp in gridTiles)
                {
                    Vector2Int newPos = new Vector2Int(kvp.Key.x, kvp.Key.y + 1);
                    kvp.Value.transform.position = GridToWorldPosition(newPos.x, newPos.y);
                    kvp.Value.name = $"Tile_{newPos.x}_{newPos.y}";
                    newDict2[newPos] = kvp.Value;
                }
                gridTiles = newDict2;
                
                // Create new row at y=0
                for (int x = 0; x < currentWidth; x++)
                {
                    CreateTile(x, 0, false);
                }
                currentHeight++;
                break;
        }
        
        CenterCamera();
    }
    
    public void UnlockTile(int x, int y)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        if (gridTiles.ContainsKey(gridPos))
        {
            gridTiles[gridPos].UnlockTile();
        }
    }
    
    private Vector3 GridToWorldPosition(int x, int y)
    {
        return new Vector3(x * tileSize, y * tileSize, 0);
    }
    
    private void CenterCamera()
    {
        if (Camera.main != null)
        {
            float centerX = (currentWidth - 1) * tileSize / 2f;
            float centerY = (currentHeight - 1) * tileSize / 2f;
            Camera.main.transform.position = new Vector3(centerX, centerY, -10);
        }
    }
    
    // Helper method to get tile at position
    public FarmTile GetTile(int x, int y)
    {
        Vector2Int gridPos = new Vector2Int(x, y);
        return gridTiles.ContainsKey(gridPos) ? gridTiles[gridPos] : null;
    }
}