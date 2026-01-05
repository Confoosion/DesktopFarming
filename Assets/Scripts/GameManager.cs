using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    public FarmGridManager gridManager;
    
    [Header("Game State")]
    public int currency = 100;
    public int unlockTileCost = 50;
    public int expandGridCost = 200;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        // Grid expansion controls (using arrow keys to avoid conflict with hotbar)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryExpandGrid(0); // Expand right
        }
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            TryExpandGrid(1); // Expand top
        }
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryExpandGrid(2); // Expand left
        }
        
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TryExpandGrid(3); // Expand bottom
        }
    }
    
    public void AddCurrency(int amount)
    {
        currency += amount;
        Debug.Log($"Currency: {currency}");
    }
    
    public bool TryUnlockTile(int x, int y)
    {
        if (currency >= unlockTileCost)
        {
            currency -= unlockTileCost;
            gridManager.UnlockTile(x, y);
            Debug.Log($"Tile unlocked! Currency: {currency}");
            return true;
        }
        else
        {
            Debug.Log("Not enough currency!");
            return false;
        }
    }
    
    public bool TryExpandGrid(int direction)
    {
        if (currency >= expandGridCost)
        {
            currency -= expandGridCost;
            gridManager.ExpandGrid(direction);
            Debug.Log($"Grid expanded! Currency: {currency}");
            return true;
        }
        else
        {
            Debug.Log("Not enough currency to expand grid!");
            return false;
        }
    }
}