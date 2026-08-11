using Unity.Netcode;
using UnityEngine;

public class InventoryTest : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private ItemData testItem;
    [SerializeField] private int amountToAdd = 1;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TestAddItem();
        }
    }

    private void TestAddItem()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory has not been assigned.");
            return;
        }

        if (testItem == null)
        {
            Debug.LogError("Test Item has not been assigned.");
            return;
        }

        bool addedEverything =
            inventory.AddItem(testItem, amountToAdd);

        Debug.Log(
            addedEverything
                ? $"Added {amountToAdd} {testItem.ItemName}"
                : $"Could not fit all {amountToAdd} {testItem.ItemName}"
        );
    }
}