using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FoodContainer : BaseKitchenObject
{
    public EventHandler<OnprogressChangedEventArgs> OnProgressChanged;
    public class OnprogressChangedEventArgs : EventArgs
    {
        public float progressNormalized;
    }

    [SerializeField] private ObjectsSO defaultPizza;
    private bool isSpawning;
    private ObjectsSO selectedPizza;
    private ulong currentPlayerId;

    private void Start()
    {
        PizzaSelectionUI.Instance.OnPizzaSelected += OnPizzaChosen;
    }

    public override void Interact(players player)
    {
        if (!HasKitchenObject() && !isSpawning)
        {
            if (!player.HasKitchenObject())
            {
                Debug.Log("Opening Pizza Selection UI...");
                PizzaSelectionUI.Instance.Show();
                currentPlayerId = player.GetComponent<NetworkObject>().NetworkObjectId;
            }
        }
    }

    private void OnPizzaChosen(ObjectsSO chosen)
    {
        selectedPizza = chosen;
        if (currentPlayerId != 0)
        {
            RequestSpawnProcessServerRpc(currentPlayerId);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSpawnProcessServerRpc(ulong playerNetworkId)
    {
        StartCoroutine(SpawnFoodAfterDelayCoroutine(playerNetworkId));
    }

    private IEnumerator SpawnFoodAfterDelayCoroutine(ulong playerNetworkId)
    {
        isSpawning = true;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObject))
        {
            players player = playerNetworkObject.GetComponent<players>();
            player?.FreezePlayer();
        }

        float timer = 0f;
        float spawnDuration = 5f;

        while (timer < spawnDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / spawnDuration;
            UpdateProgressClientRpc(progress);
            yield return null;
        }

        SpawnFoodServerRpc(playerNetworkId);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObj))
        {
            players player = playerNetworkObj.GetComponent<players>();
            player?.UnfreezePlayer();
        }

        isSpawning = false;
        selectedPizza = null;
        currentPlayerId = 0;
        UpdateProgressClientRpc(0f);
    }

    [ClientRpc]
    private void UpdateProgressClientRpc(float progress)
    {
        OnProgressChanged?.Invoke(this, new OnprogressChangedEventArgs { progressNormalized = progress });
    }

    [ServerRpc]
    private void SpawnFoodServerRpc(ulong playerNetworkId)
    {
        Transform prefabToSpawn = (selectedPizza != null ? selectedPizza.prefab : defaultPizza.prefab);
        Transform kitchenObjectTransform = Instantiate(prefabToSpawn);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        NetworkObject networkKitchenObject = kitchenObjectTransform.GetComponent<NetworkObject>();

        if (networkKitchenObject != null)
        {
            networkKitchenObject.Spawn(true);
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObject))
        {
            players player = playerNetworkObject.GetComponent<players>();
            if (player != null && kitchenObject != null)
            {
                kitchenObject.SetKitchenObjectParent(player);
            }
        }
    }
}