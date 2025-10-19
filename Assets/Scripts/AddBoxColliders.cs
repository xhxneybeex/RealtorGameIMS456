using UnityEngine;

public class AddBoxColliders : MonoBehaviour
{
    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Collider>() == null)
            {
                child.gameObject.AddComponent<BoxCollider>();
            }
        }
    }
}
