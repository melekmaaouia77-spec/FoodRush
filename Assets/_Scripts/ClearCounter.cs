using UnityEngine;
using Unity.Netcode;

public class ClearCounter : BaseKitchenObject
{
    [SerializeField] private ClearCounter secondClearCounter;
    [SerializeField] private bool testing;

    private void Update()
    {
        if (testing && Input.GetKeyDown(KeyCode.R))
        {
            if (HasKitchenObject())
            {
                GetKitchenObject().SetKitchenObjectParent(secondClearCounter);
            }
        }
    }

    public override void Interact(players player)
    {
        // Client sends request to server
        if (IsClient)
        {
            InteractServerRpc(player.NetworkObject.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void InteractServerRpc(ulong playerNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObject))
        {
            players player = playerNetworkObject.GetComponent<players>();
            if (player == null) return;

            PerformInteraction(player);
        }
    }

    private void PerformInteraction(players player)
    {
        if (!HasKitchenObject())
        {
            if (player.HasKitchenObject())
            {
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
        }
        else
        {
            if (player.HasKitchenObject())
            {
                // players already has something
            }
            else
            {
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }
}