using UnityEngine;

public class PickUpHeater : MonoBehaviour

{
    [Header("Item Info")]
    public string itemName = "Item";
    public GameObject heldPrefab;          // what appears in the player's hand

    [Header("References")]
    public Transform playerHand;           // drag the player’s hand slot here in inspector

    private bool isHeld = false;

    void Update()
    {
        // Press E while looking at it (your raycast already ensures focus)
        if (Input.GetKeyDown(KeyCode.E) && !isHeld)
        {
            Pickup();
        }

        // If already held, allow cycling with scroll wheel
        if (isHeld)
        {
            HandleScroll();
        }
    }

    void Pickup()
    {
        // Disable visible mesh + colliders
        foreach (var rend in GetComponentsInChildren<MeshRenderer>())
            rend.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Spawn held prefab in player’s hand
        if (heldPrefab && playerHand)
        {
            GameObject held = Instantiate(heldPrefab, playerHand);
            held.transform.localPosition = Vector3.zero;
            held.transform.localRotation = Quaternion.identity;

            // Optionally disable this script once picked up
            isHeld = true;
        }

        Debug.Log(itemName + " picked up!");
    }

    void HandleScroll()
    {
        // simple scroll cycling between 3 slots (placeholder for real inventory)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.1f)
        {
            Debug.Log("Switching item slot with scroll: " + scroll);
        }
    }
}
