using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    private Inventory inventory;
    private Inventory transferTarget;

    private readonly List<InventorySlotUI> createdSlots = new();

    private void Update()
    {
        if (inventory == null)
        {
            TryFindLocalPlayerInventory();
            return;
        }

        RefreshUI();
    }

    private void TryFindLocalPlayerInventory()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (NetworkManager.Singleton.LocalClient == null)
        {
            return;
        }

        NetworkObject localPlayer =
            NetworkManager.Singleton.LocalClient.PlayerObject;

        if (localPlayer == null)
        {
            return;
        }

        inventory = localPlayer.GetComponent<Inventory>();

        if (inventory == null)
        {
            return;
        }

        BuildUI();
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

    public void SetTransferTarget(Inventory target)
    {
        transferTarget = target;
    }

    public void ClearTransferTarget()
    {
        transferTarget = null;
    }

    public Inventory GetInventory()
    {
        return inventory;
    }
}