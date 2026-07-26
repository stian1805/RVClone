using Unity.Netcode;
using UnityEngine;

public class GoalPoint : NetworkBehaviour
{
    private bool hasWinner;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || hasWinner)
            return;

        NetworkObject player =
            other.GetComponentInParent<NetworkObject>();

        if (player == null || !player.IsPlayerObject)
            return;

        hasWinner = true;
        PlayerWonClientRpc(player.OwnerClientId);
    }

    [ClientRpc]
    private void PlayerWonClientRpc(ulong winnerClientId)
    {
        Debug.Log($"Player {winnerClientId} wins!");
    }
}