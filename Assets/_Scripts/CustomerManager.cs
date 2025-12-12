using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;

public class CustomerManager : MonoBehaviour  
{
    [Header("References")]
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform counterPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private GameObject deliveryCounterGO;
    [SerializeField] private GameObject orderManagerGO;

    private DIIeveryCounter deliveryCounter;
    private OrderManager orderManager;
    private List<CustomerAI> customers = new List<CustomerAI>();
    private float queueSpacing = 2f;

    private void Start()
    {
        // Register with NetworkManager
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnServerStarted()
    {

        if (deliveryCounterGO != null)
            deliveryCounter = deliveryCounterGO.GetComponent<DIIeveryCounter>();
        if (orderManagerGO != null)
            orderManager = orderManagerGO.GetComponent<OrderManager>();

        StartCoroutine(DelayedFirstSpawn());
    }

    private IEnumerator DelayedFirstSpawn()
    {
        yield return new WaitForSeconds(1f);
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (customerPrefab == null)
        {
            return;
        }

        Vector3 spawnPos = spawnPoint.position;

        GameObject customerObj = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
        NetworkObject netObj = customerObj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
          
            Destroy(customerObj);
            return;
        }

        netObj.Spawn();
        

        CustomerAI ai = customerObj.GetComponent<CustomerAI>();
        Vector3 queuePos = counterPoint.position - counterPoint.forward * (queueSpacing * customers.Count);

        ai.Initialize(this, queuePos, deliveryCounter, orderManager);

        if (customers.Count > 0)
            ai.SetCustomerInFront(customers[customers.Count - 1]);

        customers.Add(ai);
        
    }

    // ADD THIS METHOD
    public void SpawnNextCustomer()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(2f);
        SpawnCustomer();
    }

    public void CustomerLeft(CustomerAI customer)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        customers.Remove(customer);
       

        // Update queue positions
        for (int i = 0; i < customers.Count; i++)
        {
            Vector3 newPos = counterPoint.position - counterPoint.forward * (queueSpacing * i);
            customers[i].UpdateQueuePosition(newPos);
        }
    }

    // ADD THIS METHOD
    public Transform GetExitPoint() => exitPoint;

    // ADD THIS METHOD
    public CustomerAI GetFirstCustomer()
    {
        if (customers.Count > 0)
            return customers[0];
        return null;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }
}