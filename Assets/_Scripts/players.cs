using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class players : NetworkBehaviour, IKitchenObjectParent
{
    public static players Instance { get; private set; }
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseKitchenObject selectedCounter;
    }

    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 moveInput;
    private Vector3 velocity;
    private bool isWalking;
    private Vector3 lastInteraction;

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask interactableLaye;
    [SerializeField] Transform KitchenObjectHoldPoint;
    private BaseKitchenObject selectedCounter;
    private BaseKitchenObject baseKitchenObject;
    private KitchenObject kitchenObject;

    private NetworkVariable<bool> isFrozen = new NetworkVariable<bool>();

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isFrozen.OnValueChanged += OnFrozenStateChanged;
    }

    private void OnFrozenStateChanged(bool oldValue, bool newValue)
    {
        Debug.Log($"players {NetworkObjectId} frozen: {newValue}");
    }

    public void OnMove(InputValue value)
    {
        if (isFrozen.Value) return;

        Vector2 input = value.Get<Vector2>();
        moveInput = new Vector3(input.x, 0f, input.y).normalized;
        isWalking = moveInput.magnitude > 0.1f;
    }

    public void Interaction()
    {
        if (isFrozen.Value) return;

        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
        UpdateRotation();
        HandleInteractions();
    }

    private void HandleMovement()
    {
        if (isFrozen.Value) return;

        Vector3 move = moveInput * speed;
        controller.Move(move * Time.deltaTime);

        if (moveInput != Vector3.zero)
            lastInteraction = moveInput;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateRotation()
    {
        if (moveInput != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void HandleInteractions()
    {
        if (isFrozen.Value) return;

        float interactDistance = 5f;
        float playerHeight = controller.height;
        float playerRadius = controller.radius;

        Vector3 point1 = transform.position + Vector3.up * playerRadius;
        Vector3 point2 = transform.position + Vector3.up * (playerHeight - playerRadius);

        if (Physics.CapsuleCast(point1, point2, playerRadius, lastInteraction, out RaycastHit hit, interactDistance))
        {
            if (hit.transform.TryGetComponent(out baseKitchenObject))
            {
                if (baseKitchenObject != selectedCounter)
                {
                    selectedCounter = baseKitchenObject;
                    if (baseKitchenObject is ClearCounter clearcounter)
                    {
                        SetSelectedCounter(baseKitchenObject);
                    }
                }
                else
                {
                    SetSelectedCounter(null);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Interaction();
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetFrozenStateServerRpc(bool freeze)
    {
        isFrozen.Value = freeze;
    }

    public void FreezePlayer()
    {
        if (IsServer)
        {
            isFrozen.Value = true;
        }
        else
        {
            SetFrozenStateServerRpc(true);
        }
    }

    public void UnfreezePlayer()
    {
        if (IsServer)
        {
            isFrozen.Value = false;
        }
        else
        {
            SetFrozenStateServerRpc(false);
        }
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }

    private void SetSelectedCounter(BaseKitchenObject selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetKitchenObjectToFollowTransform()
    {
        return KitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}