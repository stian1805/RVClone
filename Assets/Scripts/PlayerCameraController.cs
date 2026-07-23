using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        // Disable for everyone by default
        playerCamera.enabled = false;

        if (audioListener != null)
            audioListener.enabled = false;
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
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }

        playerCamera.enabled = true;

        if (audioListener != null)
            audioListener.enabled = true;
    }
}