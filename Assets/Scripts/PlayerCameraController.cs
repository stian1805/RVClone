using Unity.Netcode;
using UnityEngine;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (audioListener == null && playerCamera != null)
        {
            audioListener = playerCamera.GetComponent<AudioListener>();
        }

        SetCameraActive(false);
    }

    private void Start()
    {
        SetCameraActive(IsOwner);
    }

    public override void OnNetworkSpawn()
    {
        // Only the owning client may have an active camera and audio listener.
        SetCameraActive(IsOwner);
    }

    public override void OnNetworkDespawn()
    {
        SetCameraActive(false);
    }

    private void SetCameraActive(bool isActive)
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = isActive;
        }

        if (audioListener != null)
        {
            audioListener.enabled = isActive;
        }
    }
}
