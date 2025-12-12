using UnityEngine;

public class PlateSystem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coffee"))
        {
            Debug.Log("Coffee placed on the plate!");
            // Optional: attach the coffee to the plate
            other.transform.SetParent(transform);
            // Optional: freeze movement
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;
        }
    }
}
