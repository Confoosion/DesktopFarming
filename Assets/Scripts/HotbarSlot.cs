using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class HotbarSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Image slotImage;
    public Image iconImage;
    public TextMeshProUGUI labelText;
    public Image selectionHighlight;
    
    [Header("Slot Data")]
    public int slotIndex;
    public HotbarSystem.ToolType toolType = HotbarSystem.ToolType.None;
    public CropData cropData; // If this is a seed slot, store the crop type
    public HotbarSystem hotbarSystem;
    
    [Header("Visual Settings")]
    public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color selectedColor = new Color(1f, 1f, 0.3f, 1f);
    
    private bool isSelected = false;
    
    private void Start()
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(false);
        }
        
        if (slotImage != null)
        {
            slotImage.color = normalColor;
        }
    }
    
    public void SetTool(HotbarSystem.ToolType tool, string label, CropData crop = null)
    {
        toolType = tool;
        cropData = crop;
        
        if (labelText != null)
        {
            labelText.text = label;
        }
        
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(true);
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (selectionHighlight != null)
        {
            selectionHighlight.gameObject.SetActive(selected);
        }
        
        if (slotImage != null)
        {
            slotImage.color = selected ? selectedColor : normalColor;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (toolType == HotbarSystem.ToolType.None) return;
        
        if (isSelected)
        {
            // Clicking again deselects
            hotbarSystem.DeselectAll();
        }
        else
        {
            hotbarSystem.SelectSlot(slotIndex);
        }
    }
}