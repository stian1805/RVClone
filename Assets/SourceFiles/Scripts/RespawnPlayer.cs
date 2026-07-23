using UnityEngine;
using Unity.Cinemachine;
using Unity.Netcode;

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
    public class RespawnPlayer : MonoBehaviour
    {
        [Tooltip("The Y position threshold at which the player will respawn.")]
        public float yThreshold = -5f; 

        private Vector3 _startingPosition;

        private Quaternion _startingRotation;

        private CharacterController _characterController;
        private NetworkObject _networkObject;

        public CinemachineCamera vCam;

        private ThirdPersonController _thirdPersonController;
        public AudioClip respawnSound;


        private void Start()
{
    // Save the starting position and rotation
    _startingPosition = transform.position;
    _startingRotation = transform.rotation;

    // Get the CharacterController reference
    _characterController = GetComponent<CharacterController>();
    _networkObject = GetComponent<NetworkObject>();
    if (_characterController == null)
    {
        Debug.LogError("CharacterController component is required for RespawnPlayer script!");
    }

    // Get ThirdPersonController reference
    _thirdPersonController = GetComponent<ThirdPersonController>();
    if (_thirdPersonController == null)
    {
        Debug.LogError("ThirdPersonController component is required for RespawnPlayer!");
    }
}

        private void Update()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            // Check if the player's Y position has fallen below the threshold
            if (transform.position.y < yThreshold)
            {
                Respawn();
            }
        }

        private void Respawn()
{
    if (!IsLocalPlayer)
    {
        return;
    }

    // Disable the CharacterController so we can manually adjust position
    if (_characterController != null)
    {
        _characterController.enabled = false; // Disable to reset position/rotation correctly
    }

    // Reset the player's position and rotation
    transform.SetPositionAndRotation(_startingPosition, _startingRotation);

    // Reset the CharacterController's vertical velocity to ensure the robot doesn't keep falling
    if (_characterController != null)
    {
        _characterController.enabled = true; // Enable it back after resetting position
    }

    // Reset the camera's rotation
    if (_thirdPersonController != null)
    {
        _thirdPersonController.ResetVerticalVelocity();
        _thirdPersonController.ResetCameraRotation(_startingRotation.eulerAngles.y); // Reset camera to the saved spawn yaw
    }

    AudioSource.PlayClipAtPoint(respawnSound, transform.position);

}

        private bool IsLocalPlayer =>
            _networkObject == null || !_networkObject.IsSpawned || _networkObject.IsOwner;
    }
}
