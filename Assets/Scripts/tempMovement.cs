using Unity.Netcode;
using UnityEngine;

public class TemporaryRVMovement : NetworkBehaviour
{
    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftCollider;
    [SerializeField] private WheelCollider frontRightCollider;
    [SerializeField] private WheelCollider rearLeftCollider;
    [SerializeField] private WheelCollider rearRightCollider;

    [Header("Visible Wheel Models")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;

    [Tooltip("Minimum horizontal RV displacement needed before wheel animation starts.")]
    [Min(0f)]
    [SerializeField] private float wheelVisualMovementThreshold = 0.01f;

    [Tooltip("How often to check whether the RV has genuinely moved.")]
    [Min(0.01f)]
    [SerializeField] private float wheelMovementSampleInterval = 0.2f;

    [Tooltip("Flip this if the wheel meshes spin the wrong way.")]
    [SerializeField] private bool invertWheelVisualRotation = true;

    [Header("Driving")]
    [SerializeField] private float motorTorque = 1800f;
    [SerializeField] private float brakeTorque = 4000f;
    [SerializeField] private float maximumSteeringAngle = 28f;
    [SerializeField] private float maximumSpeedKph = 90f;

    [Header("Stability")]
    [SerializeField] private Transform centreOfMass;

    private Rigidbody rvRigidbody;

    private bool canDrive;
    private ulong driverClientId = ulong.MaxValue;

    private float movementInput;
    private float steeringInput;
    private bool brakingInput;

    private Vector3 lastWheelMovementSamplePosition;
    private Vector3 lastWheelVisualPosition;
    private float nextWheelMovementSampleTime;
    private float wheelVisualForwardTravel;
    private bool shouldAnimateWheelRotation;

    private void Awake()
    {
        rvRigidbody = GetComponent<Rigidbody>();

        if (rvRigidbody == null)
        {
            Debug.LogError(
                "TemporaryRVMovement requires a Rigidbody on the RV root."
            );
        }

        if (rvRigidbody != null && centreOfMass != null)
        {
            rvRigidbody.centerOfMass =
                transform.InverseTransformPoint(centreOfMass.position);
        }

        lastWheelMovementSamplePosition = transform.position;
        lastWheelVisualPosition = transform.position;
    }

    private void Update()
    {
        if (canDrive)
        {
            ReadLocalInput();
        }

        UpdateWheelMovementState();
        UpdateAllWheelModels();
    }

    private void UpdateWheelMovementState()
    {
        Vector3 currentPosition = transform.position;
        Vector3 frameDisplacement =
            currentPosition - lastWheelVisualPosition;
        frameDisplacement.y = 0f;
        wheelVisualForwardTravel =
            Vector3.Dot(frameDisplacement, transform.forward);
        lastWheelVisualPosition = currentPosition;

        if (Time.time < nextWheelMovementSampleTime)
        {
            return;
        }

        Vector3 displacement =
            currentPosition - lastWheelMovementSamplePosition;
        displacement.y = 0f;

        shouldAnimateWheelRotation = displacement.sqrMagnitude >=
            wheelVisualMovementThreshold * wheelVisualMovementThreshold;

        lastWheelMovementSamplePosition = currentPosition;
        nextWheelMovementSampleTime =
            Time.time + wheelMovementSampleInterval;
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        ApplyDrivingPhysics();
    }

    private void ReadLocalInput()
    {
        float movement = 0f;
        float steering = 0f;

        if (Input.GetKey(KeyCode.I))
        {
            movement = 1f;
        }

        if (Input.GetKey(KeyCode.K))
        {
            movement = -1f;
        }

        if (Input.GetKey(KeyCode.J))
        {
            steering = -1f;
        }

        if (Input.GetKey(KeyCode.L))
        {
            steering = 1f;
        }

        bool braking = Input.GetKey(KeyCode.Space);

        SendDrivingInputServerRpc(
            movement,
            steering,
            braking
        );
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendDrivingInputServerRpc(
        float movement,
        float steering,
        bool braking,
        ServerRpcParams rpcParams = default
    )
    {
        ulong senderClientId =
            rpcParams.Receive.SenderClientId;

        if (senderClientId != driverClientId)
        {
            return;
        }

        movementInput = Mathf.Clamp(movement, -1f, 1f);
        steeringInput = Mathf.Clamp(steering, -1f, 1f);
        brakingInput = braking;
    }

    private void ApplyDrivingPhysics()
{
    if (rvRigidbody == null)
    {
        return;
    }

    float speedKph =
        rvRigidbody.linearVelocity.magnitude * 3.6f;

    bool hasNoThrottle =
        Mathf.Abs(movementInput) < 0.01f;

    bool isAlmostStopped =
        rvRigidbody.linearVelocity.magnitude < 0.15f;

    float appliedMotorTorque =
        movementInput * motorTorque;

    // Stop applying forward power after reaching the speed limit.
    if (speedKph >= maximumSpeedKph && movementInput > 0f)
    {
        appliedMotorTorque = 0f;
    }

    // Remove tiny motor forces while standing still.
    if (hasNoThrottle && isAlmostStopped)
    {
        appliedMotorTorque = 0f;
    }

    float steeringAngle =
        steeringInput * maximumSteeringAngle;

    frontLeftCollider.steerAngle = steeringAngle;
    frontRightCollider.steerAngle = steeringAngle;

    // Rear-wheel drive.
    rearLeftCollider.motorTorque = appliedMotorTorque;
    rearRightCollider.motorTorque = appliedMotorTorque;

    // Front wheels should not receive motor power.
    frontLeftCollider.motorTorque = 0f;
    frontRightCollider.motorTorque = 0f;

    float appliedBrakeTorque;

    if (brakingInput)
    {
        // Full brake when Space is held.
        appliedBrakeTorque = brakeTorque;
    }
    else if (hasNoThrottle && isAlmostStopped)
    {
        // Small automatic brake to prevent physics jitter
        // from slowly rotating the wheels while parked.
        appliedBrakeTorque = 750f;
    }
    else
    {
        appliedBrakeTorque = 0f;
    }

    frontLeftCollider.brakeTorque = appliedBrakeTorque;
    frontRightCollider.brakeTorque = appliedBrakeTorque;
    rearLeftCollider.brakeTorque = appliedBrakeTorque;
    rearRightCollider.brakeTorque = appliedBrakeTorque;
}
    private void UpdateAllWheelModels()
    {
        UpdateWheelModel(frontLeftCollider, frontLeftWheel);
        UpdateWheelModel(frontRightCollider, frontRightWheel);
        UpdateWheelModel(rearLeftCollider, rearLeftWheel);
        UpdateWheelModel(rearRightCollider, rearRightWheel);
    }

    private void UpdateWheelModel(
    WheelCollider wheelCollider,
    Transform wheelModel
)
{
    if (wheelCollider == null || wheelModel == null)
    {
        return;
    }

    wheelCollider.GetWorldPose(
        out Vector3 position,
        out _
    );

    wheelModel.position = position;

    if (!shouldAnimateWheelRotation)
    {
        return;
    }

    float wheelCircumference = 2f * Mathf.PI * wheelCollider.radius;

    if (wheelCircumference <= Mathf.Epsilon)
    {
        return;
    }

    float spinDegrees =
        wheelVisualForwardTravel / wheelCircumference * 360f;

    if (invertWheelVisualRotation)
    {
        spinDegrees = -spinDegrees;
    }

    wheelModel.Rotate(Vector3.forward, spinDegrees, Space.Self);
}

    public void EnableDriving(ulong clientId)
    {
        canDrive = true;
        SetDriverServerRpc(clientId);
    }

    public void DisableDriving()
    {
        canDrive = false;
        ClearDriverServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetDriverServerRpc(ulong clientId)
    {
        driverClientId = clientId;

        movementInput = 0f;
        steeringInput = 0f;
        brakingInput = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClearDriverServerRpc()
    {
        driverClientId = ulong.MaxValue;

        movementInput = 0f;
        steeringInput = 0f;
        brakingInput = true;
    }
}
