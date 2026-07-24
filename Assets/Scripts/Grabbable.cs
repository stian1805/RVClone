using Unity.Netcode;
using UnityEngine;

public class Grabbable : NetworkBehaviour
{
    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
}