using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HotbarSystem : MonoBehaviour
{
    public static HotbarSystem Instance { get; private set; }
    
    [Header("Hotbar Settings")]
    public int hotbarSlots = 6;
    public GameObject slotPrefab;
    public Transform hotbarParent;
    
    [Header("Available Crops")]
    public CropData[] availableCrops; // Assign your crop ScriptableObjects here
    
    [Header("Tool Settings")]
    public ToolType selectedTool = ToolType.None;
    public CropData selectedCrop; // Currently selected crop type
    
    private List<HotbarSlot> slots = new List<HotbarSlot>();
    private int selectedSlotIndex = -1;
    
    public enum ToolType
    {
        None,
        Seed,
        WateringCan,
        Scythe
    }
    
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
    
    private void Start()
    {
        CreateHotbarSlots();
        SetupDefaultTools();
    }
    
    private void Update()
    {
        // Number key selection (1-6)
        for (int i = 0; i < Mathf.Min(hotbarSlots, 9); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
        
        // Deselect with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectAll();
        }
    }
    
    private void CreateHotbarSlots()
    {
        for (int i = 0; i < hotbarSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, hotbarParent);
            HotbarSlot slot = slotObj.GetComponent<HotbarSlot>();
            
            if (slot != null)
            {
                slot.slotIndex = i;
                slot.hotbarSystem = this;
                slots.Add(slot);
            }
        }
    }
    
    private void SetupDefaultTools()
    {
        // Slot 0: Watering Can
        if (slots.Count > 0) 
            slots[0].SetTool(ToolType.WateringCan, "Water", null);
        
        // Slot 1: Scythe
        if (slots.Count > 1) 
            slots[1].SetTool(ToolType.Scythe, "Scythe", null);
        
        // Slots 2+: Different crop types
        for (int i = 0; i < availableCrops.Length && (i + 2) < slots.Count; i++)
        {
            if (availableCrops[i] != null)
            {
                slots[i + 2].SetTool(ToolType.Seed, availableCrops[i].cropName, availableCrops[i]);
            }
        }
    }
    
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return;
        
        // Deselect previous slot
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
        {
            slots[selectedSlotIndex].SetSelected(false);
        }
        
        // Select new slot
        selectedSlotIndex = index;
        slots[selectedSlotIndex].SetSelected(true);
        selectedTool = slots[selectedSlotIndex].toolType;
        selectedCrop = slots[selectedSlotIndex].cropData;
        
        if (selectedCrop != null)
        {
            Debug.Log($"Selected {selectedCrop.cropName} seeds");
        }
        else
        {
            Debug.Log($"Selected tool: {selectedTool}");
        }
    }
    
    public void DeselectAll()
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slots.Count)
        {
            slots[selectedSlotIndex].SetSelected(false);
        }
        
        selectedSlotIndex = -1;
        selectedTool = ToolType.None;
        selectedCrop = null;
        Debug.Log("No tool selected");
    }
    
    public ToolType GetSelectedTool()
    {
        return selectedTool;
    }
    
    public CropData GetSelectedCrop()
    {
        return selectedCrop;
    }
    
    private Color GetToolColor(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.Seed: return new Color(0.8f, 0.6f, 0.3f); // Brown
            case ToolType.WateringCan: return new Color(0.3f, 0.6f, 0.9f); // Blue
            case ToolType.Scythe: return new Color(0.7f, 0.7f, 0.7f); // Gray
            default: return Color.white;
        }
    }
}