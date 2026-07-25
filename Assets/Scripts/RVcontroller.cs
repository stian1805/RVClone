using Unity.Netcode;
using UnityEngine;

public class Seat : MonoBehaviour
{
    private NetworkObject nearbyPlayer;
    [SerializeField] private Transform sitPoint;

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

        nearbyPlayer = playerNetworkObject;

        Debug.Log(
            $"Player {playerNetworkObject.OwnerClientId} is close to the seat"
        );
    }

    private void OnTriggerExit(Collider other)
    {
        NetworkObject playerNetworkObject =
            other.GetComponentInParent<NetworkObject>();

        if (playerNetworkObject == nearbyPlayer)
        {
            nearbyPlayer = null;
            Debug.Log("Player moved away from the seat");
        }
    }

    private void Update()
{
    if (nearbyPlayer == null)
    {
        return;
    }

    if (!nearbyPlayer.IsOwner)
    {
        return;
    }

    if (Input.GetKeyDown(KeyCode.E))
        {
            SitDown();
        }
}

private void SitDown()
{
    nearbyPlayer.transform.position = sitPoint.position;
    nearbyPlayer.transform.rotation = sitPoint.rotation;

    Debug.Log("Player moved to the seat");
}
}