using UnityEngine;

public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private GameObject playerInventoryPanel;

    private bool isPlayerInventoryOpen;
    private bool storageIsOpen;
    private PlayerMovement playerMovement;
    private bool gameplayReady;
    private void Start()
    {
        playerInventoryPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void StartGameplay()
    {
        gameplayReady = true;

        FindLocalPlayerMovement();

        LockMouse();
        EnablePlayerControl();
    }
    private void DisablePlayerControl()
    {
        if (playerMovement == null)
        {
            FindLocalPlayerMovement();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
    }

    private void EnablePlayerControl()
    {
        if (playerMovement == null)
        {
            FindLocalPlayerMovement();
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePlayerInventory();
        }
    }

    private void TogglePlayerInventory()
    {
        if (!gameplayReady)
        {
            return;
        }

        if (storageIsOpen)
        {
            return;
        }

        isPlayerInventoryOpen = !isPlayerInventoryOpen;

        playerInventoryPanel.SetActive(isPlayerInventoryOpen);

        if (isPlayerInventoryOpen)
        {
            UnlockMouse();
            DisablePlayerControl();
        }
        else
        {
            LockMouse();
            EnablePlayerControl();
        }
    }

    public void OpenForStorage()
    {
        storageIsOpen = true;
        isPlayerInventoryOpen = true;

        playerInventoryPanel.SetActive(true);

        UnlockMouse();
        DisablePlayerControl();
    }

    public void CloseForStorage()
    {
        storageIsOpen = false;
        isPlayerInventoryOpen = false;

        playerInventoryPanel.SetActive(false);

        LockMouse();
        EnablePlayerControl();
    }

    private void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FindLocalPlayerMovement()
    {
        if (Unity.Netcode.NetworkManager.Singleton == null)
        {
            return;
        }

        var localClient =
            Unity.Netcode.NetworkManager.Singleton.LocalClient;

        if (localClient == null)
        {
            return;
        }

        var playerObject = localClient.PlayerObject;

        if (playerObject == null)
        {
            return;
        }

        playerMovement =
            playerObject.GetComponent<PlayerMovement>();
    }
}