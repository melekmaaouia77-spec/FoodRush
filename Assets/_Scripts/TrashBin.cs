using UnityEngine;
using Unity.Netcode;

public class TrashBin : BaseKitchenObject
{
    public override void Interact(Player player)
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
            Player player = playerNetworkObject.GetComponent<Player>();
            if (player == null) return;

            PerformInteraction(player);
        }
    }

    private void PerformInteraction(Player player)
    {
        Debug.Log($"TrashBin.Interact() called on {gameObject.name}!");

        if (player.HasKitchenObject())
        {
            Debug.Log("Destroying player's kitchen object directly from player");

            // Destroy the object directly from the player WITHOUT transferring to trash bin
            player.GetKitchenObject().DestroySelf();
            Debug.Log("Kitchen object destroyed successfully");
        }
        else
        {
            Debug.Log("Player has no kitchen object to destroy!");
        }
    }
}