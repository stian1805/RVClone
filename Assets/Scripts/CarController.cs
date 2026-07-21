using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [Header("Entry")]
    [SerializeField] private Transform driverSeat;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CinemachineCamera carFollowCamera;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftCollider;
    [SerializeField] private WheelCollider frontRightCollider;
    [SerializeField] private WheelCollider rearLeftCollider;
    [SerializeField] private WheelCollider rearRightCollider;

    [Header("Visible Wheels")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;

    [Header("Driving")]
    [SerializeField] private float motorTorque = 1500f;
    [SerializeField] private float brakeTorque = 3000f;
    [SerializeField] private float maximumSteeringAngle = 30f;

    [Tooltip("Maximum forward speed in metres per second.")]
    [SerializeField] private float maximumSpeed = 30f;

    private Rigidbody carRigidbody;

    private bool hasDriver;
    private float accelerationInput;
    private float steeringInput;
    private bool brakeInput;

    public Transform DriverSeat => driverSeat;
    public Transform ExitPoint => exitPoint;

    private void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();

        if (carRigidbody == null)
        {
            Debug.LogError(
                "CarController requires a Rigidbody on the same GameObject.",
                this
            );
        }

        // The player's Cinemachine camera starts as the active camera.
        if (carFollowCamera != null)
        {
            carFollowCamera.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!hasDriver)
        {
            accelerationInput = 0f;
            steeringInput = 0f;
            brakeInput = true;
            return;
        }

        ReadDrivingInput();
    }

    private void FixedUpdate()
    {
        if (carRigidbody == null)
        {
            return;
        }

        ApplySteering();
        ApplyMotor();
        ApplyBrakes();
    }

    private void LateUpdate()
    {
        UpdateWheelVisual(frontLeftCollider, frontLeftWheel);
        UpdateWheelVisual(frontRightCollider, frontRightWheel);
        UpdateWheelVisual(rearLeftCollider, rearLeftWheel);
        UpdateWheelVisual(rearRightCollider, rearRightWheel);
    }

    public void SetDriver(bool driverIsInside)
    {
        hasDriver = driverIsInside;

        if (carFollowCamera != null)
        {
            carFollowCamera.gameObject.SetActive(driverIsInside);
        }

        if (!driverIsInside)
        {
            accelerationInput = 0f;
            steeringInput = 0f;
            brakeInput = true;
        }
    }

    private void ReadDrivingInput()
    {
        if (Keyboard.current == null)
        {
            accelerationInput = 0f;
            steeringInput = 0f;
            brakeInput = true;
            return;
        }

        accelerationInput = 0f;
        steeringInput = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            accelerationInput += 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            accelerationInput -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            steeringInput += 1f;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            steeringInput -= 1f;
        }

        brakeInput = Keyboard.current.spaceKey.isPressed;
    }

    private void ApplySteering()
    {
        float steeringAngle =
            steeringInput * maximumSteeringAngle;

        if (frontLeftCollider != null)
        {
            frontLeftCollider.steerAngle = steeringAngle;
        }

        if (frontRightCollider != null)
        {
            frontRightCollider.steerAngle = steeringAngle;
        }
    }

    private void ApplyMotor()
    {
        float forwardSpeed = Vector3.Dot(
            carRigidbody.linearVelocity,
            transform.forward
        );

        bool aboveForwardLimit =
            forwardSpeed >= maximumSpeed &&
            accelerationInput > 0f;

        bool aboveReverseLimit =
            forwardSpeed <= -maximumSpeed * 0.5f &&
            accelerationInput < 0f;

        float torque =
            aboveForwardLimit || aboveReverseLimit
                ? 0f
                : accelerationInput * motorTorque;

        if (rearLeftCollider != null)
        {
            rearLeftCollider.motorTorque = torque;
        }

        if (rearRightCollider != null)
        {
            rearRightCollider.motorTorque = torque;
        }
    }

    private void ApplyBrakes()
    {
        float appliedBrakeTorque =
            brakeInput ? brakeTorque : 0f;

        if (frontLeftCollider != null)
        {
            frontLeftCollider.brakeTorque = appliedBrakeTorque;
        }

        if (frontRightCollider != null)
        {
            frontRightCollider.brakeTorque = appliedBrakeTorque;
        }

        if (rearLeftCollider != null)
        {
            rearLeftCollider.brakeTorque = appliedBrakeTorque;
        }

        if (rearRightCollider != null)
        {
            rearRightCollider.brakeTorque = appliedBrakeTorque;
        }
    }

    private void UpdateWheelVisual(
        WheelCollider wheelCollider,
        Transform visibleWheel
    )
    {
        if (wheelCollider == null || visibleWheel == null)
        {
            return;
        }

        wheelCollider.GetWorldPose(
            out Vector3 wheelPosition,
            out Quaternion wheelRotation
        );

        visibleWheel.SetPositionAndRotation(
            wheelPosition,
            wheelRotation
        );
    }
}