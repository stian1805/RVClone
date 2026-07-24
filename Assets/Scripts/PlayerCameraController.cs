using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    private void Start()
    {
        if (IsOwner)
        {
            ActivatePlayerCamera();
        }
    }

    public override void OnNetworkSpawn()
    {
        // Only the owner gets the camera
        if (IsOwner)
        {
            ActivatePlayerCamera();
        }
    }

    private void ActivatePlayerCamera()
    {
        playerCamera.enabled = true;

        if (audioListener != null)
            audioListener.enabled = true;
    }
}