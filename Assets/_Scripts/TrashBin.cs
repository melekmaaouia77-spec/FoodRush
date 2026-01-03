using UnityEngine;
using Unity.Netcode;

public class TrashBin : BaseKitchenObject
{
    public override void Interact(players player)
    {
        if (IsClient)
        {
            InteractServerRpc(player.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
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
        Debug.Log($"TrashBin.Interact() called on {gameObject.name}!");

        if (player.HasKitchenObject())
        {
            Debug.Log("Destroying players's kitchen object directly from players");

            // Destroy the object directly from the players WITHOUT transferring to trash bin
            player.GetKitchenObject().DestroySelf();
            Debug.Log("Kitchen object destroyed successfully");
        }
        else
        {
            Debug.Log("players has no kitchen object to destroy!");
        }
    }
}