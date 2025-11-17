using UnityEngine;

public class LightBulbPickup : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Light Bulb";      // name for clarity
    public GameObject heldPrefab;               // prefab that appears in hand

    [Header("References")]
    public Transform playerHand;                // where the bulb spawns when picked up

    private bool isHeld = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isHeld)
        {
            Pickup();
        }

        if (isHeld)
        {
            HandleScroll();
        }
    }

    void Pickup()
    {
        // Hide this bulb in the world
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
            r.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        // Create the handheld version
        if (heldPrefab && playerHand)
        {
            GameObject held = Instantiate(heldPrefab, playerHand);
            held.transform.localPosition = Vector3.zero;
            held.transform.localRotation = Quaternion.identity;
            isHeld = true;
        }

        Debug.Log("Picked up: " + itemName);
    }

    void HandleScroll()
    {
        // placeholder scroll-wheel swap system
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.1f)
        {
            Debug.Log("Switched item slot (scroll detected)");
        }
    }
}
