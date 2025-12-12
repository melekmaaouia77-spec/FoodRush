using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class SodaContainer : BaseKitchenObject
{
    public EventHandler<OnprogressChangedEventArgs> OnProgressChanged;
    public class OnprogressChangedEventArgs : EventArgs
    {
        public float progressNormalized;
    }

    [SerializeField] private ObjectsSO defaultSoda;
    private bool isSpawning;
    private ObjectsSO selectedSoda;
    private ulong currentPlayerId;

    private void Start()
    {
        SodaSelectionUI.Instance.OnSodaSelected += OnSodaChosen;
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject() && !isSpawning)
        {
            if (!player.HasKitchenObject())
            {
                Debug.Log("Opening Soda Selection UI...");
                SodaSelectionUI.Instance.Show();
                currentPlayerId = player.GetComponent<NetworkObject>().NetworkObjectId;
            }
        }
    }

    private void OnSodaChosen(ObjectsSO chosen)
    {
        selectedSoda = chosen;
        if (currentPlayerId != 0)
        {
            RequestSpawnProcessServerRpc(currentPlayerId);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestSpawnProcessServerRpc(ulong playerNetworkId)
    {
        StartCoroutine(SpawnSodaAfterDelayCoroutine(playerNetworkId));
    }

    private IEnumerator SpawnSodaAfterDelayCoroutine(ulong playerNetworkId)
    {
        isSpawning = true;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObject))
        {
            Player player = playerNetworkObject.GetComponent<Player>();
            player?.FreezePlayer();
        }

        float timer = 0f;
        float spawnDuration = 3f;

        while (timer < spawnDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / spawnDuration;
            UpdateProgressClientRpc(progress);
            yield return null;
        }

        SpawnSodaServerRpc(playerNetworkId);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObj))
        {
            Player player = playerNetworkObj.GetComponent<Player>();
            player?.UnfreezePlayer();
        }

        isSpawning = false;
        selectedSoda = null;
        currentPlayerId = 0;
        UpdateProgressClientRpc(0f);
    }

    [ClientRpc]
    private void UpdateProgressClientRpc(float progress)
    {
        OnProgressChanged?.Invoke(this, new OnprogressChangedEventArgs { progressNormalized = progress });
    }

    [ServerRpc]
    private void SpawnSodaServerRpc(ulong playerNetworkId)
    {
        Transform prefabToSpawn = (selectedSoda != null ? selectedSoda.prefab : defaultSoda.prefab);
        Transform kitchenObjectTransform = Instantiate(prefabToSpawn);

        KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        NetworkObject networkKitchenObject = kitchenObjectTransform.GetComponent<NetworkObject>();

        if (networkKitchenObject != null)
        {
            networkKitchenObject.Spawn(true);
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject playerNetworkObject))
        {
            Player player = playerNetworkObject.GetComponent<Player>();
            if (player != null && kitchenObject != null)
            {
                kitchenObject.SetKitchenObjectParent(player);
            }
        }
    }
}