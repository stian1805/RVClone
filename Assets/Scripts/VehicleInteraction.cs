using StarterAssets;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInteraction : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("The real Unity camera containing the Camera component.")]
    [SerializeField] private Camera playerCamera;

    [Tooltip("The Cinemachine camera used while walking.")]
    [SerializeField] private CinemachineCamera playerFollowCamera;

    [Tooltip("The Starter Assets movement component.")]
    [SerializeField] private ThirdPersonController playerMovement;

    [SerializeField] private CharacterController characterController;

    [Tooltip("Only the visible model, not the PlayerRobot root.")]
    [SerializeField] private GameObject playerVisuals;

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 5f;

    [Tooltip("Exclude the Player layer from this mask.")]
    [SerializeField] private LayerMask interactionLayers = ~0;

    private CarController _currentCar;
    private float _nextAllowedInteractionTime;
    private NetworkObject _networkObject;

    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();

        if (playerMovement == null)
        {
            playerMovement =
                GetComponentInChildren<ThirdPersonController>();
        }

        if (characterController == null)
        {
            characterController =
                GetComponentInChildren<CharacterController>();
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (Time.time < _nextAllowedInteractionTime)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.eKey.wasPressedThisFrame)
        {
            return;
        }

        if (_currentCar != null)
        {
            ExitCar();
        }
        else
        {
            TryEnterCar();
        }
    }

    private void TryEnterCar()
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "VehicleInteraction: Player Camera has not been assigned.",
                this
            );

            return;
        }

        Vector3 rayOrigin =
            playerCamera.transform.position +
            playerCamera.transform.forward * 0.25f;

        Ray ray = new Ray(
            rayOrigin,
            playerCamera.transform.forward
        );

        Debug.DrawRay(
            ray.origin,
            ray.direction * interactionDistance,
            Color.red,
            2f
        );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                interactionDistance,
                interactionLayers,
                QueryTriggerInteraction.Ignore
            ))
        {
            Debug.Log("Pressed E, but the raycast hit nothing.");
            return;
        }

        Debug.Log($"Interaction ray hit: {hit.collider.name}");

        CarController car =
            hit.collider.GetComponentInParent<CarController>();

        if (car == null)
        {
            Debug.Log(
                $"Hit {hit.collider.name}, but it does not belong to a car."
            );

            return;
        }

        EnterCar(car);
    }

    private void EnterCar(CarController car)
    {
        if (car.DriverSeat == null)
        {
            Debug.LogError(
                "The car does not have a DriverSeat assigned.",
                car
            );

            return;
        }

        _currentCar = car;
        _nextAllowedInteractionTime = Time.time + 0.3f;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (playerVisuals != null)
        {
            playerVisuals.SetActive(false);
        }

        // Move the hidden player to the driver position.
        // Do not parent PlayerRobot to the car.
        transform.SetPositionAndRotation(
            car.DriverSeat.position,
            car.DriverSeat.rotation
        );

        // Disable only the walking Cinemachine camera.
        // The real Unity Camera remains enabled.
        if (playerFollowCamera != null)
        {
            playerFollowCamera.gameObject.SetActive(false);
        }

        car.SetDriver(true);
    }

    private void ExitCar()
    {
        CarController carBeingExited = _currentCar;

        if (carBeingExited == null)
        {
            return;
        }

        if (carBeingExited.ExitPoint == null)
        {
            Debug.LogError(
                "The car does not have an ExitPoint assigned.",
                carBeingExited
            );

            return;
        }

        _currentCar = null;

        carBeingExited.SetDriver(false);

        transform.SetPositionAndRotation(
            carBeingExited.ExitPoint.position,
            carBeingExited.ExitPoint.rotation
        );

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerVisuals != null)
        {
            playerVisuals.SetActive(true);
        }

        if (playerFollowCamera != null)
        {
            playerFollowCamera.gameObject.SetActive(true);
        }

        _nextAllowedInteractionTime = Time.time + 0.3f;
    }

    private bool IsLocalPlayer =>
        _networkObject == null || !_networkObject.IsSpawned || _networkObject.IsOwner;
}