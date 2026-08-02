using Unity.Netcode;
using UnityEngine;

public class RVStorageInteraction : MonoBehaviour
{
    [SerializeField] private Inventory storageInventory;
    [SerializeField] private ItemData testItem;

    private Inventory nearbyPlayerInventory;

    private void Update()
    {
        if (nearbyPlayerInventory == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Opened RV storage");

            Debug.Log(
                $"Player slots: {nearbyPlayerInventory.Slots.Count}, " +
                $"RV slots: {storageInventory.Slots.Count}"
            );
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TransferItem(
                nearbyPlayerInventory,
                storageInventory,
                testItem,
                1
            );
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            TransferItem(
                storageInventory,
                nearbyPlayerInventory,
                testItem,
                1
            );
        }
    }

    private void TransferItem(
        Inventory source,
        Inventory destination,
        ItemData item,
        int amount
        )
    {
        if (source == null || destination == null || item == null)
        {
            Debug.LogWarning("Transfer information is missing.");
            return;
        }

        if (source.GetItemCount(item) < amount)
        {
            Debug.Log($"The source inventory has no {item.ItemName}.");
            return;
        }

        bool addedToDestination =
            destination.AddItem(item, amount);

        if (!addedToDestination)
        {
            Debug.Log(
                $"There is not enough room for {item.ItemName}."
            );

            return;
        }

        bool removedFromSource =
            source.RemoveItem(item, amount);

        if (!removedFromSource)
        {
            // Safety rollback in case something unexpected happened.
            destination.RemoveItem(item, amount);

            Debug.LogError("The inventory transfer failed.");
            return;
        }

        Debug.Log($"Transferred {amount} {item.ItemName}.");
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject player =
            other.GetComponentInParent<NetworkObject>();

        if (player == null)
        {
            return;
        }

        if (!player.IsPlayerObject || !player.IsOwner)
        {
            return;
        }

        Inventory playerInventory =
            player.GetComponent<Inventory>();

        if (playerInventory == null)
        {
            Debug.LogWarning(
                "The local player does not have an Inventory component."
            );

            return;
        }

        nearbyPlayerInventory = playerInventory;

        Debug.Log("Local player is near RV storage");
    }

    private void OnTriggerExit(Collider other)
    {
        NetworkObject player =
            other.GetComponentInParent<NetworkObject>();

        if (player == null || !player.IsOwner)
        {
            return;
        }

        Inventory playerInventory =
            player.GetComponent<Inventory>();

        if (playerInventory != nearbyPlayerInventory)
        {
            return;
        }

        nearbyPlayerInventory = null;

        Debug.Log("Local player moved away from RV storage");
    }
}