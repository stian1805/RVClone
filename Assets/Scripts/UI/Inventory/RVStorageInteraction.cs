using Unity.Netcode;
using UnityEngine;

public class RVStorageInteraction : MonoBehaviour
{
    [SerializeField] private Inventory storageInventory;
    [SerializeField] private StorageInventoryUI storageUI;
    [SerializeField] private InventoryUIController inventoryUIController;
    [SerializeField] private InventoryUI playerInventoryUI;
    private Inventory nearbyPlayerInventory;
    private bool isOpen;

    private void Update()
    {
        if (nearbyPlayerInventory == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isOpen)
            {
                CloseStorage();
            }
            else
            {
                OpenStorage();
            }
        }
    }

private void OpenStorage()
{
    storageUI.Open(
        storageInventory,
        nearbyPlayerInventory
    );

    if (inventoryUIController != null)
    {
        inventoryUIController.OpenForStorage();
    }

    if (playerInventoryUI != null)
    {
        playerInventoryUI.SetTransferTarget(storageInventory);
    }

    isOpen = true;

    Debug.Log("Opened RV storage");
}

private void CloseStorage()
{
    storageUI.Close();

    if (inventoryUIController != null)
    {
        inventoryUIController.CloseForStorage();
    }

    if (playerInventoryUI != null)
    {
        playerInventoryUI.ClearTransferTarget();
    }

    isOpen = false;

    Debug.Log("Closed RV storage");
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

        if (isOpen)
        {
            CloseStorage();
        }

        nearbyPlayerInventory = null;

        Debug.Log("Local player moved away from RV storage");
    }
}