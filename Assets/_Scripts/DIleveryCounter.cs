using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class DIIeveryCounter : BaseKitchenObject
{
    public NetworkVariable<bool> leave = new NetworkVariable<bool>();

    [SerializeField] private GameObject orderManagerGO;
    [SerializeField] private GameObject customerManagerGO;
    private OrderManager orderManager;
    private CustomerManager customerManager;

    private void Awake()
    {
        if (orderManagerGO != null)
            orderManager = orderManagerGO.GetComponent<OrderManager>();
        if (customerManagerGO != null)
            customerManager = customerManagerGO.GetComponent<CustomerManager>();
    }

    public override void Interact(players player)
    {
        if (IsClient)
        {
            InteractServerRpc(player.NetworkObjectId);
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
                CustomerAI currentCustomer = customerManager.GetFirstCustomer();
                if (currentCustomer == null)
                {
                    Debug.Log("No customer waiting!");
                    return;
                }

                KitchenObject playerKitchenObject = player.GetKitchenObject();
                ObjectsSO playerObjectSO = playerKitchenObject.GetKitchenObject();

                int customerOrderIndex = currentCustomer.GetMyOrderIndex();
                ObjectsSO requiredObjectSO = orderManager.Food[customerOrderIndex];

                Debug.Log($"players has: {playerObjectSO.objectName}, Customer wants: {requiredObjectSO.objectName}");

                if (playerObjectSO == requiredObjectSO)
                {
                    Debug.Log("Correct order delivered! Setting leave = true");

                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    GetKitchenObject().DestroySelf();

                    leave.Value = true;

                    // Award money to the player who made the delivery
                    AwardMoneyToPlayerClientRpc(player.NetworkObjectId);

                    StartCoroutine(ResetLeaveFlag());
                }
                else
                {
                    Debug.Log("Wrong order! Customer wants something else.");
                }
            }
            else
            {
                Debug.Log("players has no kitchen object to deliver!");
            }
        }
        else
        {
            Debug.Log("Counter already has an object on it!");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AwardMoneyToPlayerClientRpc(ulong playerNetworkObjectId)
    {
        // Find the player object and award money only on their owning client
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetworkObject))
        {
            // Only the owner of this player should add money to their score
            if (playerNetworkObject.IsOwner)
            {
                MoneyManager.Instance.AddMoney(10);
                Debug.Log($"Player {playerNetworkObjectId} earned $10!");
            }
        }
    }

    private IEnumerator ResetLeaveFlag()
    {
        yield return new WaitForSeconds(1f);
        leave.Value = false;
    }
}