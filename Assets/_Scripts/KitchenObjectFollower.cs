using UnityEngine;

public class KitchenObjectFollower : MonoBehaviour
{
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            // Snap to position immediately
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    private void Update()
    {
        if (target != null)
        {
            // Follow continuously
            transform.position = target.position;
            transform.rotation = target.rotation;
        }
    }

    private void OnDestroy()
    {
        target = null;
    }
}