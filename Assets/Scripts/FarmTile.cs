using UnityEngine;

public class FarmTile : MonoBehaviour
{
    [Header("Tile State")]
    public bool isUnlocked = false;
    public bool isPlanted = false;
    public float growthProgress = 0f;
    
    [Header("Water System - Independent of Crop")]
    public bool isWatered = false;
    public float waterLevel = 0f; // 0 = dry, 1 = fully watered
    public float waterDrainRate = 0.1f; // Water drains at this rate per second (0.1 = 10 seconds to dry)
    
    [Header("Current Crop")]
    public CropData currentCrop;
    
    [Header("Visual References")]
    public SpriteRenderer soilRenderer; // Background layer for soil
    public SpriteRenderer cropRenderer; // Foreground layer for crops
    
    [Header("Soil Sprites")]
    public Sprite lockedSprite; // Sprite for locked tiles
    public Sprite drySprite; // Sprite for dry/unwatered soil
    public Sprite wateredSprite; // Sprite for watered soil
    
    private void Start()
    {
        if (soilRenderer == null)
            soilRenderer = GetComponent<SpriteRenderer>();
        
        UpdateVisual();
    }
    
    private void Update()
    {
        // Handle water drainage - independent of crops
        if (waterLevel > 0f)
        {
            waterLevel -= waterDrainRate * Time.deltaTime;
            waterLevel = Mathf.Clamp01(waterLevel);
            
            // Update watered status based on water level
            bool wasWatered = isWatered;
            isWatered = waterLevel > 0f;
            
            // Visual update if water status changed
            if (wasWatered != isWatered)
            {
                if (!isWatered)
                {
                    Debug.Log("Tile has dried out.");
                }
                UpdateVisual();
            }
        }
        
        // Handle crop growth - only if tile is watered
        if (isPlanted && currentCrop != null && growthProgress < 1f)
        {
            // Only grow if tile is watered
            if (isWatered)
            {
                // Grow the crop
                growthProgress += Time.deltaTime / currentCrop.growthTime;
                growthProgress = Mathf.Clamp01(growthProgress);
                UpdateVisual();
                
                if (growthProgress >= 1f)
                {
                    OnCropGrown();
                }
            }
            // If tile is dry, growth pauses
        }
    }
    
    public void UnlockTile()
    {
        isUnlocked = true;
        UpdateVisual();
    }
    
    public void PlantCrop(CropData crop)
    {
        if (!isUnlocked || crop == null) return;
        
        // Check if player has enough currency
        if (GameManager.Instance != null && GameManager.Instance.currency < crop.seedCost)
        {
            Debug.Log("Not enough currency for seeds!");
            return;
        }
        
        // Deduct seed cost
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(-crop.seedCost);
        }
        
        currentCrop = crop;
        isPlanted = true;
        growthProgress = 0f;
        
        // Water level is independent - if tile is watered, crop will grow
        if (isWatered)
        {
            Debug.Log($"Planted {crop.cropName} on watered soil! It will grow while soil is wet.");
        }
        else
        {
            Debug.Log($"Planted {crop.cropName} on dry soil. Water the tile to start growth.");
        }
        
        UpdateVisual();
    }
    
    public void WaterCrop()
    {
        if (!isUnlocked) return;
        
        // Can water the tile regardless of crop status
        if (waterLevel > 0f)
        {
            Debug.Log("Tile is already watered!");
            return;
        }
        
        // Fill water level to maximum
        waterLevel = 1f;
        isWatered = true;
        UpdateVisual();
        
        if (isPlanted && currentCrop != null)
        {
            Debug.Log($"Tile watered! {currentCrop.cropName} will grow while soil is wet.");
        }
        else
        {
            Debug.Log("Tile watered! Ready for planting.");
        }
    }
    
    public void HarvestCrop()
    {
        if (!isPlanted || growthProgress < 1f || currentCrop == null) return;
        
        // Add harvesting rewards
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(currentCrop.harvestValue);
        }
        
        Debug.Log($"Harvested {currentCrop.cropName}! +{currentCrop.harvestValue} currency");
        
        // Reset crop state but keep water level
        isPlanted = false;
        growthProgress = 0f;
        currentCrop = null;
        UpdateVisual();
        
        // Note: Water level persists after harvest
    }
    
    private void OnCropGrown()
    {
        Debug.Log($"{currentCrop.cropName} fully grown! Ready to harvest!");
    }
    
    private void UpdateVisual()
    {
        // Update soil sprite based on tile state
        if (!isUnlocked)
        {
            // Locked tile
            if (soilRenderer != null)
            {
                soilRenderer.sprite = lockedSprite;
                soilRenderer.color = Color.white;
            }
            
            // No crop visible
            if (cropRenderer != null)
            {
                cropRenderer.sprite = null;
            }
            return;
        }
        else if (!isPlanted)
        {
            // No crop visible
            if (cropRenderer != null)
            {
                cropRenderer.sprite = null;
            }
        }
        else if (currentCrop != null)
        {   
            // Show crop sprite based on growth progress
            if (cropRenderer != null)
            {
                Sprite currentSprite = currentCrop.GetSpriteForGrowth(growthProgress);
                if (currentSprite != null)
                {
                    cropRenderer.sprite = currentSprite;
                    cropRenderer.color = Color.white;
                }
            }
        }

        soilRenderer.sprite = isWatered ? wateredSprite : drySprite;
    }
    
    private void OnMouseDown()
    {
        if (!isUnlocked)
        {
            Debug.Log("Tile is locked!");
            return;
        }
        
        // Get selected tool from hotbar
        if (HotbarSystem.Instance == null) return;
        
        HotbarSystem.ToolType selectedTool = HotbarSystem.Instance.GetSelectedTool();
        
        switch (selectedTool)
        {
            case HotbarSystem.ToolType.Seed:
                if (!isPlanted)
                {
                    // Get the selected crop from hotbar
                    CropData selectedCrop = HotbarSystem.Instance.GetSelectedCrop();
                    if (selectedCrop != null)
                    {
                        PlantCrop(selectedCrop);
                    }
                    else
                    {
                        Debug.Log("No crop selected!");
                    }
                }
                else
                {
                    Debug.Log("Already planted!");
                }
                break;
                
            case HotbarSystem.ToolType.WateringCan:
                if (waterLevel < 1f)
                {
                    WaterCrop();
                }
                else
                {
                    Debug.Log("Tile is already fully watered!");
                }
                break;
                
            case HotbarSystem.ToolType.Scythe:
                if (isPlanted && growthProgress >= 1f)
                {
                    HarvestCrop();
                }
                else if (!isPlanted)
                {
                    Debug.Log("No crop to harvest!");
                }
                else
                {
                    Debug.Log("Crop not ready to harvest!");
                }
                break;
                
            case HotbarSystem.ToolType.None:
                Debug.Log("No tool selected! Press 1, 2, or 3 to select a tool.");
                break;
        }
    }
    
    private void OnMouseOver()
    {
        // Allow dragging with any tool
        if (Input.GetMouseButton(0) && HotbarSystem.Instance != null)
        {
            if (!isUnlocked)
            {
                return; // Can't do anything on locked tiles
            }
            
            HotbarSystem.ToolType selectedTool = HotbarSystem.Instance.GetSelectedTool();
            
            switch (selectedTool)
            {
                case HotbarSystem.ToolType.Seed:
                    if (!isPlanted)
                    {
                        CropData selectedCrop = HotbarSystem.Instance.GetSelectedCrop();
                        if (selectedCrop != null)
                        {
                            PlantCrop(selectedCrop);
                        }
                    }
                    break;
                    
                case HotbarSystem.ToolType.WateringCan:
                    if (waterLevel < 1f)
                    {
                        WaterCrop();
                    }
                    break;
                    
                case HotbarSystem.ToolType.Scythe:
                    if (isPlanted && growthProgress >= 1f)
                    {
                        HarvestCrop();
                    }
                    break;
            }
        }
    }
}