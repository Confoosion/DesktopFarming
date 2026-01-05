using UnityEngine;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Farm/Crop Data")]
public class CropData : ScriptableObject
{
    [Header("Crop Info")]
    public string cropName;
    
    [Header("Growth Settings")]
    public float growthTime = 10f; // Total time to grow (in seconds)
    public float waterInterval = 5f; // How often it needs water (in seconds)
    
    [Header("Growth Sprites")]
    public Sprite[] growthStages; // Array of sprites for each growth phase
    // Index 0 = just planted, Index 1 = phase 2, etc., Last index = fully grown
    
    [Header("Rewards")]
    public int harvestValue = 10; // Currency earned on harvest
    public int seedCost = 5; // Cost to plant
    
    // Helper method to get the correct sprite based on growth progress
    public Sprite GetSpriteForGrowth(float growthProgress)
    {
        if (growthStages == null || growthStages.Length == 0)
            return null;
        
        // If fully grown (100%), always show the last sprite
        if (growthProgress >= 1f)
        {
            return growthStages[growthStages.Length - 1];
        }
        
        // Calculate which growth stage we're in (0 to Length-2)
        // This ensures the last sprite is reserved for 100% growth
        int maxStageBeforeFinal = growthStages.Length - 1;
        int stageIndex = Mathf.FloorToInt(growthProgress * maxStageBeforeFinal);
        stageIndex = Mathf.Clamp(stageIndex, 0, maxStageBeforeFinal - 1);
        
        return growthStages[stageIndex];
    }
    
    // Get number of growth phases
    public int GetPhaseCount()
    {
        return growthStages != null ? growthStages.Length : 0;
    }
}