using UnityEngine;
using UnityEngine.UI;

public sealed class Slot : MonoBehaviour
{
    private int itemID;
    [SerializeField]
    private Image slot;
    [SerializeField]
    private Image icon;

    public void PopulateSlot(int blockID)
    {
        itemID = blockID;

        if (World.Instance.GetBlockTypes[blockID].icon != null)
        {
            icon.enabled = true;
            icon.sprite = World.Instance.GetBlockTypes[blockID].icon;
        }
    }

    public void ClearSlot()
    {
        itemID = 0;

        icon.enabled = false;
        icon.sprite = null;
    }

    public void Select()
    {
        slot.rectTransform.sizeDelta = SlotProperties.SelectedSlotSize;
        slot.color = SlotProperties.SelectedSlotColor;
    }

    public void UnSelect()
    {
        slot.rectTransform.sizeDelta = SlotProperties.UnselectedSlotSize;
        slot.color = SlotProperties.UnselectedSlotColor;
    }
}
