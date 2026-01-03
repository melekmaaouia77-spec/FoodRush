using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class IceCreamContainer : BaseKitchenObject
{
    public EventHandler<OnprogressChangedEventArgs> OnProgressChanged;
    public class OnprogressChangedEventArgs : EventArgs
    {
        public float progressNormalized;
    }

    [SerializeField] private ObjectsSO defaultIceCream;
    private bool isSpawning;
    private ObjectsSO selectedIceCream;
    private ulong currentPlayerId;

    private void Start()
    {
        IceCreamSelectionUI.Instance.OnIceCreamSelected += OnIceCreamChosen;
    }

    public override void Interact(players player)
    {
        if (!HasKitchenObject() && !isSpawning)
        {
            if (!player.HasKitchenObject())
            {
                Debug.Log("Opening Ice Cream Selection UI...");
                IceCreamSelectionUI.Instance.Show();
                currentPlayerId = player.GetComponent<NetworkObject>().NetworkObjectId;
            }
        }
    }

    private void OnIceCreamChosen(ObjectsSO chosen)
    {
        selectedIceCream = chosen;
        if (currentPlayerId != 0)
        {
            RequestSpawnProcessServerRpc(currentPlayerId);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSpawnProcessServerRpc(ulong playerNetworkId)
    {
        StartCoroutine(SpawnIceCreamAfterDelayCoroutine(playerNetworkId));
    }

    private IEnumerator SpawnIceCreamAfterDelayCoroutine(ulong playerNetworkId)
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

        SpawnIceCreamServerRpc(playerNetworkId);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObj))
        {
            players player = playerNetworkObj.GetComponent<players>();
            player?.UnfreezePlayer();
        }

        isSpawning = false;
        selectedIceCream = null;
        currentPlayerId = 0;
        UpdateProgressClientRpc(0f);
    }

    [ClientRpc]
    private void UpdateProgressClientRpc(float progress)
    {
        OnProgressChanged?.Invoke(this, new OnprogressChangedEventArgs { progressNormalized = progress });
    }

    [ServerRpc]
    private void SpawnIceCreamServerRpc(ulong playerNetworkId)
    {
        Transform prefabToSpawn = (selectedIceCream != null ? selectedIceCream.prefab : defaultIceCream.prefab);
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