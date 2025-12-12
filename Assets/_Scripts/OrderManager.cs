using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class OrderManager : NetworkBehaviour
{
    public ObjectsSO[] Food;
    [SerializeField] private float orderInterval = 10f;
    public bool orderActive = false;
    public int randomIndex;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(GenerateOrders());
        }
    }

    IEnumerator GenerateOrders()
    {
        while (true)
        {
            yield return new WaitForSeconds(orderInterval);
            GenerateRandomOrder();
            orderActive = true;
        }
    }

    public string GenerateRandomOrder()
    {
        if (Food == null || Food.Length == 0)
        {
            Debug.LogError("Food array is empty or not assigned in OrderManager!");
            return "";
        }

        randomIndex = Random.Range(0, Food.Length);
        string order = Food[randomIndex].objectName;
        return order;
    }
}
