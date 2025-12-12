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

    public override void Interact(Player player)
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
            Player player = playerNetworkObject.GetComponent<Player>();
            if (player == null) return;

            PerformInteraction(player);
        }
    }

    private void PerformInteraction(Player player)
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

                Debug.Log($"Player has: {playerObjectSO.objectName}, Customer wants: {requiredObjectSO.objectName}");

                if (playerObjectSO == requiredObjectSO)
                {
                    Debug.Log("Correct order delivered! Setting leave = true");

                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    GetKitchenObject().DestroySelf();

                    leave.Value = true;
                    StartCoroutine(ResetLeaveFlag());
                }
                else
                {
                    Debug.Log("Wrong order! Customer wants something else.");
                }
            }
            else
            {
                Debug.Log("Player has no kitchen object to deliver!");
            }
        }
        else
        {
            Debug.Log("Counter already has an object on it!");
        }
    }

    private IEnumerator ResetLeaveFlag()
    {
        yield return new WaitForSeconds(1f);
        leave.Value = false;
    }
}