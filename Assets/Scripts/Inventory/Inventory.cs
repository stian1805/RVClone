using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int numberOfSlots = 12;
    [SerializeField] private List<InventorySlot> slots = new();

    public IReadOnlyList<InventorySlot> Slots => slots;

    private void Awake()
    {
        CreateEmptySlots();
    }

    private void CreateEmptySlots()
    {
        slots.Clear();

        for (int i = 0; i < numberOfSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
    }

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        int amountRemaining = amount;

        // First, fill existing stacks of the same item.
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty)
            {
                continue;
            }

            if (slot.Item != item)
            {
                continue;
            }

            amountRemaining = slot.Add(item, amountRemaining);

            if (amountRemaining == 0)
            {
                return true;
            }
        }

        // Then use empty slots.
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty)
            {
                continue;
            }

            amountRemaining = slot.Add(item, amountRemaining);

            if (amountRemaining == 0)
            {
                return true;
            }
        }

        Debug.Log(
            $"Inventory was full. Could not add {amountRemaining} {item.ItemName}"
        );

        return false;
    }
    public bool RemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
        {
            return false;
        }

        int totalAvailable = GetItemCount(item);

        if (totalAvailable < amount)
        {
            return false;
        }

        int amountRemaining = amount;

        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty || slot.Item != item)
            {
                continue;
            }

            int removed = slot.Remove(amountRemaining);
            amountRemaining -= removed;

            if (amountRemaining == 0)
            {
                return true;
            }
        }

        return false;
    }

    public int GetItemCount(ItemData item)
    {
        if (item == null)
        {
            return 0;
        }

        int total = 0;

        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.Item == item)
            {
                total += slot.Quantity;
            }
        }

        return total;
    }
}