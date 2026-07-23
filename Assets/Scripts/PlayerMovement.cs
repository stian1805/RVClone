using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour 
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;

        Move();
    }
    
    private void Move()
    {
        Vector3 moveDirection = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDirection.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDirection.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDirection.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDirection.x = 1f;

        float moveSpeed = 3f;
            
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
    
}
