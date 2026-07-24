using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController controller;

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 40f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
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

        transform.Rotate(
            Vector3.up * mouseX * turnSpeed * Time.deltaTime
        );
    }
}