using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Heater";

    [Header("Held Version")]
    // This is the prefab that appears in the player's hand
    public GameObject heldPrefab;
}
