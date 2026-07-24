using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform grabPoint;

    private NetworkObject heldObject;


    private void Update()
    {
        if (!IsOwner)
            return;


        if (Input.GetMouseButtonDown(0))
        {
            TryGrab();
        }


        if (Input.GetMouseButtonUp(0))
        {
            Release();
        }
    }


    private void FixedUpdate()
    {
        if (!IsOwner)
            return;


        if (heldObject != null)
        {
            MoveHeldObjectServerRpc(
                heldObject.NetworkObjectId,
                grabPoint.position,
                grabPoint.rotation
            );
        }
    }



    private void TryGrab()
    {
        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );


        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            Grabbable grabbable = hit.collider.GetComponent<Grabbable>();

            if (grabbable != null)
            {
                heldObject = grabbable.NetworkObject;


                GrabServerRpc(
                    grabbable.NetworkObject.NetworkObjectId
                );
            }
        }
    }



    [ServerRpc]
    private void GrabServerRpc(
        ulong objectId)
    {
        if (!NetworkManager.Singleton
            .SpawnManager
            .SpawnedObjects
            .TryGetValue(objectId, out NetworkObject obj))
        {
            return;
        }


        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb == null)
            return;


        rb.useGravity = false;

        rb.linearDamping = 20f;
        rb.angularDamping = 20f;


        // Stops physics fighting the carry movement
        rb.isKinematic = true;

        rb.interpolation = RigidbodyInterpolation.None;
    }



    [ServerRpc]
    private void MoveHeldObjectServerRpc(
        ulong objectId,
        Vector3 position,
        Quaternion rotation)
    {
        if (!NetworkManager.Singleton
            .SpawnManager
            .SpawnedObjects
            .TryGetValue(objectId, out NetworkObject obj))
        {
            return;
        }


        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb == null)
            return;


        // Direct server movement while held
        rb.position = position;
        rb.rotation = rotation;
    }



    private void Release()
    {
        if (heldObject == null)
            return;


        ReleaseServerRpc(
            heldObject.NetworkObjectId
        );


        heldObject = null;
    }



    [ServerRpc]
    private void ReleaseServerRpc(
        ulong objectId)
    {
        if (!NetworkManager.Singleton
            .SpawnManager
            .SpawnedObjects
            .TryGetValue(objectId, out NetworkObject obj))
        {
            return;
        }


        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb == null)
            return;


        rb.isKinematic = false;

        rb.useGravity = true;

        rb.linearDamping = 0;

        rb.angularDamping = 0.05f;


        // Turn physics interpolation back on
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
}