using UnityEngine;
using Unity.Netcode;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private ObjectsSO Kitchenobject;
    private IKitchenObjectParent kitchenObjectParent;
    private KitchenObjectFollower follower;

    public ObjectsSO GetKitchenObject()
    {
        return Kitchenobject;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        if (this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }

        this.kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("Counter already has a KitchenObject");
        }

        kitchenObjectParent.SetKitchenObject(this);

        Transform followTransform = kitchenObjectParent.GetKitchenObjectToFollowTransform();
        transform.position = followTransform.position;
        transform.localPosition = Vector3.zero;

        // Add/update follower
        if (follower != null)
        {
            Destroy(follower);
        }
        follower = gameObject.AddComponent<KitchenObjectFollower>();
        follower.SetTarget(followTransform);
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
        if (kitchenObjectParent != null)
        {
            kitchenObjectParent.ClearKitchenObject();
        }

        if (follower != null)
        {
            Destroy(follower);
        }

        if (IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}