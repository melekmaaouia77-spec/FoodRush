using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using Unity.Netcode;

public class CustomerAI : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Rigidbody rb; // ADD THIS
    private CustomerManager manager;
    private DIIeveryCounter counter;
    private OrderManager orderManager;

    [SerializeField] private GameObject orderBubblePrefab;
    [SerializeField] private GameObject deliveredIconPrefab;
    [SerializeField] private GameObject missedIconPrefab;
    [SerializeField] private float rotation = 0;

    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> netTarget = new NetworkVariable<Vector3>();
    private NetworkVariable<bool> netIsWaiting = new NetworkVariable<bool>();
    private NetworkVariable<int> netOrderIndex = new NetworkVariable<int>(-1);
    private NetworkVariable<bool> netOrderComplete = new NetworkVariable<bool>();

    private GameObject orderBubble;
    private GameObject statusIcon;
    private bool isLeaving = false;
    private CustomerAI customerInFront;

    public override void OnNetworkSpawn()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); // ADD THIS

        // CRITICAL FIX: Handle Rigidbody and NavMesh for clients
        if (!IsServer)
        {
            // Clients: Disable physics and NavMesh
            if (agent != null)
            {
                agent.enabled = false;
            }
            if (rb != null)
            {
                rb.isKinematic = true; // ← THIS STOPS FALLING
                rb.useGravity = false; // ← NO GRAVITY ON CLIENTS
            }
        }
        else
        {
            // Server: Enable NavMesh, handle Rigidbody properly
            if (rb != null)
            {
                rb.isKinematic = true; // Server also uses kinematic to avoid physics issues
            }
        }

        // Subscribe to network changes
        netPosition.OnValueChanged += OnPositionChanged;
        netTarget.OnValueChanged += OnTargetChanged;
        netIsWaiting.OnValueChanged += OnWaitingChanged;
        netOrderIndex.OnValueChanged += OnOrderIndexChanged;
        netOrderComplete.OnValueChanged += OnOrderCompleteChanged;

        // Apply initial position
        transform.position = netPosition.Value;
    }

    public void Initialize(CustomerManager mgr, Vector3 target, DIIeveryCounter cnt, OrderManager orderMgr)
    {
        manager = mgr;
        counter = cnt;
        orderManager = orderMgr;

        if (IsServer)
        {
            netPosition.Value = transform.position;
            netTarget.Value = target;
            MoveToTarget(target);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            ServerUpdate();
        }
    }

    private void ServerUpdate()
    {
        if (isLeaving) return;

        // Sync position to clients
        if (Vector3.Distance(transform.position, netPosition.Value) > 0.01f)
        {
            netPosition.Value = transform.position;
        }

        if (!netIsWaiting.Value && agent != null && agent.isActiveAndEnabled)
        {
            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                StartWaiting();
            }
        }
    }

    private void MoveToTarget(Vector3 target)
    {
        if (!IsServer || agent == null || !agent.isActiveAndEnabled) return;
        agent.SetDestination(target);
    }

    private void StartWaiting()
    {
        if (!IsServer) return;

        netIsWaiting.Value = true;
        GenerateOrder();
        StartCoroutine(WaitForOrder());
    }

    private void GenerateOrder()
    {
        if (!IsServer) return;

        string order = orderManager.GenerateRandomOrder();
        netOrderIndex.Value = orderManager.randomIndex;

        ShowOrderBubble(order);
        SyncOrderClientRpc(order);
    }

    [ClientRpc]
    private void SyncOrderClientRpc(string orderText)
    {
        ShowOrderBubble(orderText);
    }

    private void ShowOrderBubble(string text)
    {
        
        if (orderBubblePrefab == null) return;

        if (orderBubble != null)
            Destroy(orderBubble);

        orderBubble = Instantiate(orderBubblePrefab, transform);
        orderBubble.transform.localPosition = new Vector3(0, 8f, 1f);
        orderBubble.transform.Rotate(0, (float)rotation, 0);

        TMP_Text tmpText = orderBubble.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
            tmpText.text = text;
    }

    private IEnumerator WaitForOrder()
    {
        float timer = 0f;

        while (timer < 20f && !netOrderComplete.Value)
        {
            timer += Time.deltaTime;

            if (counter != null && counter.leave.Value)
            {
                netOrderComplete.Value = true;
                break;
            }

            yield return null;
        }

        Leave();
    }

    private void Leave()
    {
        if (!IsServer) return;

        isLeaving = true;

        if (orderBubble != null)
            Destroy(orderBubble);

        ShowStatusIcon(netOrderComplete.Value);

        if (manager != null && manager.GetExitPoint() != null)
        {
            Vector3 exitPos = manager.GetExitPoint().position;
            netTarget.Value = exitPos;
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.SetDestination(exitPos);
            }
            StartCoroutine(DestroyAfterReachingExit());
        }
        else
        {
            DestroyCustomer();
        }

        if (manager != null)
            manager.SpawnNextCustomer();
    }

    private void ShowStatusIcon(bool delivered)
    {
        GameObject prefab = delivered ? deliveredIconPrefab : missedIconPrefab;
        statusIcon = Instantiate(prefab, transform);
        statusIcon.transform.localPosition = new Vector3(0, 2.5f, 1f);
    }

    private IEnumerator DestroyAfterReachingExit()
    {
        yield return new WaitForSeconds(3f);
        DestroyCustomer();
    }

    private void DestroyCustomer()
    {
        if (IsServer)
        {
            if (manager != null)
                manager.CustomerLeft(this);
            GetComponent<NetworkObject>().Despawn(true);
        }
    }

    // NETWORK SYNC HANDLERS
    private void OnPositionChanged(Vector3 oldValue, Vector3 newValue)
    {
        if (!IsServer)
        {
            // Clients snap to server position - no physics
            transform.position = newValue;
        }
    }

    private void OnTargetChanged(Vector3 oldValue, Vector3 newValue) { }
    private void OnWaitingChanged(bool oldValue, bool newValue) { }
    private void OnOrderIndexChanged(int oldValue, int newValue) { }

    private void OnOrderCompleteChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            ShowStatusIcon(true);
        }
    }

    public void SetCustomerInFront(CustomerAI front) => customerInFront = front;

    public void UpdateQueuePosition(Vector3 newTarget)
    {
        if (!IsServer) return;
        netTarget.Value = newTarget;
        MoveToTarget(newTarget);
    }

    public int GetMyOrderIndex() => netOrderIndex.Value;
}