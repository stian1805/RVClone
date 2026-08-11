using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [SerializeField] private List<ItemData> items = new();

    private Dictionary<string, ItemData> itemLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildLookup();
    }

    private void BuildLookup()
    {
        itemLookup = new Dictionary<string, ItemData>();

        foreach (ItemData item in items)
        {
            if (item == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.ItemId))
            {
                Debug.LogWarning(
                    $"Item {item.name} has no Item ID."
                );

                continue;
            }

            if (itemLookup.ContainsKey(item.ItemId))
            {
                Debug.LogError(
                    $"Duplicate Item ID found: {item.ItemId}"
                );

                continue;
            }

            itemLookup.Add(item.ItemId, item);
        }
    }

    public ItemData GetItem(string itemId)
    {
        if (itemLookup.TryGetValue(itemId, out ItemData item))
        {
            return item;
        }

        Debug.LogWarning(
            $"Could not find item with ID: {itemId}"
        );

        return null;
    }
}