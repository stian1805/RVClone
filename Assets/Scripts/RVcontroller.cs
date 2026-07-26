using Unity.Netcode;
using UnityEngine;

public class Seat : NetworkBehaviour
{
    [SerializeField] private Transform sitPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private bool isDriverSeat;
    [SerializeField] private GameObject rvObject;

    private TemporaryRVMovement rvMovement;
    private NetworkObject nearbyPlayer;
    private NetworkObject seatedPlayer;

    private CharacterController playerController;
    private PlayerMovement playerMovement;

    private bool isSitting;

    private readonly NetworkVariable<ulong> occupantClientId =
        new NetworkVariable<ulong>(ulong.MaxValue);

    private bool IsOccupied
    {
        get
        {
            return occupantClientId.Value != ulong.MaxValue;
        }
    }

    public override void OnNetworkSpawn()
    {
        occupantClientId.OnValueChanged += OnOccupantChanged;
    }

    public override void OnNetworkDespawn()
    {
        occupantClientId.OnValueChanged -= OnOccupantChanged;
    }

    private void Update()
    {
        NetworkObject playerToControl;

        if (isSitting)
        {
            playerToControl = seatedPlayer;
        }
        else
        {
            playerToControl = nearbyPlayer;
        }

        if (playerToControl == null)
        {
            return;
        }

        if (!playerToControl.IsOwner)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isSitting)
            {
                StandUp();
            }
            else
            {
                SitDown();
            }
        }
    }

    private void LateUpdate()
    {
        if (!isSitting || seatedPlayer == null)
        {
            return;
        }

        seatedPlayer.transform.position = sitPoint.position;
        seatedPlayer.transform.rotation = sitPoint.rotation;
    }

private void OnTriggerEnter(Collider other)
{
    NetworkObject playerNetworkObject =
        other.GetComponentInParent<NetworkObject>();

    if (playerNetworkObject == null)
    {
        return;
    }

    if (!playerNetworkObject.IsPlayerObject)
    {
        return;
    }

    // Ignore players controlled by other clients.
    if (!playerNetworkObject.IsOwner)
    {
        return;
    }

    nearbyPlayer = playerNetworkObject;

    Debug.Log(
        $"My local player {playerNetworkObject.OwnerClientId} is close to the seat"
    );
}

private void OnTriggerExit(Collider other)
{
    NetworkObject playerNetworkObject =
        other.GetComponentInParent<NetworkObject>();

    if (playerNetworkObject == null)
    {
        return;
    }

    if (!playerNetworkObject.IsOwner)
    {
        return;
    }

    if (playerNetworkObject != nearbyPlayer)
    {
        return;
    }

    nearbyPlayer = null;

    Debug.Log("My local player moved away from the seat");
}
    private void SitDown()
    {
        if (nearbyPlayer == null)
        {
            return;
        }

        if (IsOccupied)
        {
            Debug.Log("This seat is already occupied");
            return;
        }

        RequestSitServerRpc(nearbyPlayer.OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestSitServerRpc(ulong playerClientId)
    {
        if (IsOccupied)
        {
            return;
        }

        occupantClientId.Value = playerClientId;
    }

    private void OnOccupantChanged(
        ulong previousClientId,
        ulong newClientId
    )
    {
        if (newClientId == ulong.MaxValue)
        {
            return;
        }

        if (NetworkManager.Singleton.LocalClientId != newClientId)
        {
            return;
        }

        CompleteSitDown();
    }

    private void CompleteSitDown()
    {
        if (nearbyPlayer == null)
        {
            Debug.LogWarning(
                "The player left the trigger before the seat was claimed."
            );

            ReleaseSeatServerRpc();
            return;
        }

        seatedPlayer = nearbyPlayer;

        playerController =
            seatedPlayer.GetComponent<CharacterController>();

        playerMovement =
            seatedPlayer.GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        seatedPlayer.transform.position = sitPoint.position;
        seatedPlayer.transform.rotation = sitPoint.rotation;

        isSitting = true;
        if (isDriverSeat && rvMovement != null)
        {
            rvMovement.EnableDriving(seatedPlayer.OwnerClientId);
        }
        Debug.Log("Player successfully claimed and entered the seat");
    }

    private void StandUp()
    {
        if (seatedPlayer == null)
        {
            return;
        }

        seatedPlayer.transform.position = exitPoint.position;
        seatedPlayer.transform.rotation = exitPoint.rotation;

        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        if (isDriverSeat && rvMovement != null)
        {
            rvMovement.DisableDriving();
        }
        
        isSitting = false;
        seatedPlayer = null;
        playerController = null;
        playerMovement = null;

        ReleaseSeatServerRpc();

        Debug.Log("Player stood up");
    }
    private void Awake()
    {
        if (rvObject != null)
        {
            rvMovement = rvObject.GetComponent<TemporaryRVMovement>();
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void ReleaseSeatServerRpc()
    {
        occupantClientId.Value = ulong.MaxValue;
    }
}