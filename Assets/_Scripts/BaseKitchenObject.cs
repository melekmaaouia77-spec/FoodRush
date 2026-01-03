using Unity.Netcode;
using UnityEngine;

public class BaseKitchenObject : NetworkBehaviour, IKitchenObjectParent
{
    [SerializeField] private Transform ObjectSoPos;
    protected KitchenObject kitchenObject;
    public virtual void Interact(players player)
    {

    }
    public Transform GetKitchenObjectToFollowTransform()
    {
        return ObjectSoPos;
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


