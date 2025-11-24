using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Basic Info")]
    public string itemName = "Item";

    [Header("Prefabs")]
    public GameObject heldPrefab;     // model used when in the player's hand
    public GameObject worldPrefab;    // model dropped back into the world

    // Called by RaycastInteractor when you press E
    public void Interact()
    {
        // Same behavior you had before
        PlayerInventory inv = FindObjectOfType<PlayerInventory>();
        if (inv == null)
        {
            Debug.LogWarning("[PickupItem] No PlayerInventory found in scene.");
            return;
        }

        inv.TryAddItem(this);
    }
}
