using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController controller;

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 400f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    private Vector3 velocity;
    
    [SerializeField] private Transform cameraPivot; // Assign your Camera or a pivot object
    [SerializeField] private float lookSpeed = 300f;
    [SerializeField] private float minLookAngle = -80f;
    [SerializeField] private float maxLookAngle = 80f;

    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        GameObject spawnPoint = GameObject.FindWithTag("PlayerSpawn");

        if (spawnPoint != null)
        {
            transform.SetPositionAndRotation(
                spawnPoint.transform.position,
                spawnPoint.transform.rotation
            );
        }
    }
    void Update()
    {
        if (!IsOwner) return;

        Move();
        Turn();
        Jump();
    }

    private void Move()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection.z += 1f;

        if (Input.GetKey(KeyCode.S))
            moveDirection.z -= 1f;

        if (Input.GetKey(KeyCode.A))
            moveDirection.x -= 1f;

        if (Input.GetKey(KeyCode.D))
            moveDirection.x += 1f;


        // Move relative to player rotation
        moveDirection = transform.TransformDirection(moveDirection);

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);


        // Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }


    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }


    private void Turn()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Rotate player left/right
        transform.Rotate(Vector3.up * mouseX * turnSpeed * Time.deltaTime);

        // Rotate camera up/down
        xRotation -= mouseY * lookSpeed * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!IsOwner)
            return;

        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null)
            return;

        NetworkObject networkObject = rb.GetComponent<NetworkObject>();

        if (networkObject == null)
            return;

        Vector3 pushDirection = hit.moveDirection;
        pushDirection.y = 0; // Don't launch objects upward

        PushObjectServerRpc(networkObject.NetworkObjectId, pushDirection);
    }
    
    [ServerRpc]
    private void PushObjectServerRpc(ulong objectId, Vector3 direction)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject obj))
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddForce(direction.normalized * 20f, ForceMode.Force);
            }
        }
    }
    
}
