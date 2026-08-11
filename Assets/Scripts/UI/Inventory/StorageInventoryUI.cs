using System.Collections.Generic;
using UnityEngine;

public class StorageInventoryUI : MonoBehaviour
{
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    private Inventory inventory;
    private Inventory transferTarget;

    private readonly List<InventorySlotUI> createdSlots = new();

    public void Open(
        Inventory storageInventory,
        Inventory targetInventory
    )
    {
        inventory = storageInventory;
        transferTarget = targetInventory;

        gameObject.SetActive(true);

        BuildUI();
    }

    public void Close()
    {
        inventory = null;
        transferTarget = null;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        RefreshUI();
    }

    private void BuildUI()
    {
        foreach (InventorySlotUI slot in createdSlots)
        {
            Destroy(slot.gameObject);
        }

        createdSlots.Clear();

        for (int i = 0; i < inventory.Slots.Count; i++)
        {
            int slotIndex = i;

            GameObject newSlotObject =
                Instantiate(slotPrefab, slotParent);

            InventorySlotUI slotUI =
                newSlotObject.GetComponent<InventorySlotUI>();

            slotUI.Setup(
                inventory.Slots[slotIndex],
                () => OnSlotClicked(slotIndex)
            );

            createdSlots.Add(slotUI);
        }
    }

    private void RefreshUI()
    {
        if (inventory == null)
        {
            return;
        }

        foreach (InventorySlotUI slot in createdSlots)
        {
            slot.Refresh();
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if (transferTarget == null)
        {
            return;
        }

        InventorySlot slot =
            inventory.Slots[slotIndex];

        if (slot.IsEmpty)
        {
            return;
        }

        ItemData item = slot.Item;

        bool added =
            transferTarget.AddItem(item, 1);

        if (!added)
        {
            return;
        }

        inventory.RemoveItem(item, 1);
    }
}