using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlatesCounter : BaseKitchenObject
{
    public EventHandler OnPlateSpawned;
    public EventHandler OnPlateRemoved;

    private float spawnPlateTimer;
    private float spawnTimerMax=4f;
    private int PlatesSpawnedAmount=0;
    private int PlatesSpawnedAmountMax= 4;
    [SerializeField] private ObjectsSO kitchenObjectSo;
    private void Update()
    {
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnTimerMax)
        {
            spawnPlateTimer = 0f;
            if (PlatesSpawnedAmount < PlatesSpawnedAmountMax)
            {
                PlatesSpawnedAmount++;
                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }

        }
    }
    public override void Interact(players player)
    {
        if (!player.HasKitchenObject())
        {
            if (PlatesSpawnedAmount > 0)
            {
                PlatesSpawnedAmount--;
                Transform kitchenObjectTransform = Instantiate(kitchenObjectSo.prefab);
                kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(player);
                OnPlateRemoved?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
