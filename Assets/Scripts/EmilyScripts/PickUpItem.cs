using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName = "Item";
    public GameObject heldPrefab;

    // Called by RaycastInteractor when you press E
    public void Interact()
    {
        FindObjectOfType<PlayerInventory>().TryAddItem(this);
    }
}
