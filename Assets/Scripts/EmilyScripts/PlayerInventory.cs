using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class HeldItem
    {
        public PickupItem pickupData;   // reference to original pickup component
        public GameObject heldObject;   // in-hand instance
    }

    [Header("Inventory Settings")]
    public int maxSlots = 3;

    [Header("Hand Setup")]
    public Transform handSocket;        // where held items attach

    [Header("Drop Settings")]
    public Transform dropPoint;         // drop position in front of the player
    public KeyCode dropKey = KeyCode.F; // key to drop selected item

    private List<HeldItem> items = new List<HeldItem>();
    private int currentIndex = -1;

    void Update()
    {
        HandleScrollInput();
        HandleDropInput();
    }

    // ----------------- SCROLL SELECT -----------------
    void HandleScrollInput()
    {
        if (items.Count == 0) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            currentIndex = (currentIndex + 1) % items.Count;
            ShowCurrentItem();
        }
        else if (scroll < 0f)
        {
            currentIndex = (currentIndex - 1 + items.Count) % items.Count;
            ShowCurrentItem();
        }
    }

    // ----------------- DROP INPUT -----------------
    void HandleDropInput()
    {
        if (items.Count == 0) return;
        if (!Input.GetKeyDown(dropKey)) return;

        DropCurrentItem();
    }

    // ----------------- ADD ITEM -----------------
    public bool TryAddItem(PickupItem pickup)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("[Inventory] Inventory full!");
            return false;
        }

        if (pickup.heldPrefab == null)
        {
            Debug.LogWarning("[Inventory] Item has no heldPrefab assigned.");
            return false;
        }

        // Create held model in hand
        GameObject heldInstance = Instantiate(pickup.heldPrefab, handSocket);
        heldInstance.transform.localPosition = Vector3.zero;
        heldInstance.transform.localRotation = Quaternion.identity;
        heldInstance.SetActive(false);

        HeldItem newItem = new HeldItem
        {
            pickupData = pickup,
            heldObject = heldInstance
        };

        items.Add(newItem);

        // Disable world object instead of destroying
        //pickup.gameObject.SetActive(false);

        // Auto-select first item
        if (currentIndex == -1)
        {
            currentIndex = 0;
            ShowCurrentItem();
        }

        Debug.Log("[Inventory] Picked up: " + pickup.itemName);
        return true;
    }

    // ----------------- DROP CURRENT ITEM -----------------
    void DropCurrentItem()
    {
        if (currentIndex < 0 || currentIndex >= items.Count)
        {
            Debug.LogWarning("[Inventory] DropCurrentItem called with invalid index.");
            return;
        }

        HeldItem item = items[currentIndex];

        if (item == null || item.pickupData == null)
        {
            Debug.LogWarning("[Inventory] HeldItem or pickupData is null.");
            return;
        }

        // Decide where to drop it
        Vector3 pos = dropPoint != null
            ? dropPoint.position
            : transform.position + transform.forward * 1.0f + Vector3.up * 0.2f;

        Quaternion rot = dropPoint != null ? dropPoint.rotation : Quaternion.identity;

        // Reactivate original pickup object
        item.pickupData.gameObject.SetActive(true);
        item.pickupData.transform.position = pos;
        item.pickupData.transform.rotation = rot;

        Debug.Log($"[Inventory] Dropped: {item.pickupData.itemName} | pos: {pos}");

        // Remove the held version (only the in-hand model)
        if (item.heldObject != null)
        {
            Destroy(item.heldObject);
        }

        // Remove from inventory list
        items.RemoveAt(currentIndex);

        // Fix index and update what is shown in hand
        currentIndex = items.Count == 0 ? -1 : Mathf.Clamp(currentIndex, 0, items.Count - 1);
        ShowCurrentItem();
    }

    // ----------------- SHOW CURRENT HELD ITEM -----------------
    void ShowCurrentItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].heldObject != null)
                items[i].heldObject.SetActive(i == currentIndex);
        }
    }
}
