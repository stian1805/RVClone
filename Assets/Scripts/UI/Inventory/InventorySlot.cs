using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    [SerializeField] private ItemData item;
    [SerializeField] private int quantity;

    public ItemData Item => item;
    public int Quantity => quantity;

    public bool IsEmpty => item == null || quantity <= 0;

    public InventorySlot()
    {
        Clear();
    }

    public bool CanAdd(ItemData itemToAdd)
    {
        if (itemToAdd == null)
        {
            return false;
        }

        if (IsEmpty)
        {
            return true;
        }

        return item == itemToAdd &&
               quantity < item.MaximumStackSize;
    }

    public int Add(ItemData itemToAdd, int amount)
    {
        if (!CanAdd(itemToAdd) || amount <= 0)
        {
            return amount;
        }

        if (IsEmpty)
        {
            item = itemToAdd;
            quantity = 0;
        }

        int availableSpace =
            item.MaximumStackSize - quantity;

        int amountToAdd =
            Mathf.Min(amount, availableSpace);

        quantity += amountToAdd;

        return amount - amountToAdd;
    }

    public void Clear()
    {
        item = null;
        quantity = 0;
    }

    public int Remove(int amount)
    {
        if (IsEmpty || amount <= 0)
        {
            return 0;
        }

        int amountToRemove = Mathf.Min(amount, quantity);

        quantity -= amountToRemove;

        if (quantity <= 0)
        {
            Clear();
        }

        return amountToRemove;
    }
}