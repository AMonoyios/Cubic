using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class Toolbar : MonoBehaviour
{
    [SerializeField]
    private Slot[] slots;

    [SerializeField]
    private Image hand;

    private int selectedSlotIndex;
    private const int tempOffset = 4;

    private void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].PopulateSlot(i + tempOffset);
        }

        SelectSlot(selectedSlotIndex);
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                selectedSlotIndex--;
            }
            else
            {
                selectedSlotIndex++;
            }

            if (selectedSlotIndex > slots.Length - 1)
            {
                selectedSlotIndex = 0;
            }
            if (selectedSlotIndex < 0)
            {
                selectedSlotIndex = slots.Length - 1;
            }

            World.Instance.GetPlayer.SelectedBlockID = (byte)(selectedSlotIndex + tempOffset);

            SelectSlot(selectedSlotIndex);

            EventsManager.Instance.UpdateDebugScreenUI(playerGUIArea: true);
            EventsManager.Instance.UpdateSelectedBlockUI();
        }
    }

    private void SelectSlot(int index)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == index)
            {
                slots[i].Select();
            }
            else
            {
                slots[i].UnSelect();
            }
        }

        if (World.Instance.GetBlockTypes[selectedSlotIndex + tempOffset].icon != null)
        {
            hand.enabled = true;
            hand.sprite = World.Instance.GetBlockTypes[selectedSlotIndex + tempOffset].icon;
        }
        else
        {
            hand.enabled = false;
            hand.sprite = null;
        }
    }
}
