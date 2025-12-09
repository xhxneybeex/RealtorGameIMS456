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
    public Transform dropPoint;
    public KeyCode dropKey = KeyCode.F;

    [Header("Use Settings")]
    public KeyCode useKey = KeyCode.G;

    private List<HeldItem> items = new List<HeldItem>();
    private int currentIndex = -1;

    void Update()
    {
        HandleScrollInput();
        HandleDropInput();
        HandleUseInput();
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

    void HandleUseInput()
    {
        if (items.Count == 0) return;
        if (!Input.GetKeyDown(useKey)) return;

        UseCurrentItem();
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

        GameObject heldInstance = Instantiate(pickup.heldPrefab, handSocket);
        heldInstance.transform.localPosition = Vector3.zero;
        heldInstance.transform.localRotation = Quaternion.identity;
        heldInstance.SetActive(false);

        Rigidbody heldRb = heldInstance.GetComponent<Rigidbody>();
        if (heldRb != null)
        {
            Destroy(heldRb);
        }

        HeldItem newItem = new HeldItem
        {
            pickupData = pickup,
            heldObject = heldInstance
        };

        items.Add(newItem);

        Rigidbody pickupRb = pickup.GetComponent<Rigidbody>();
        if (pickupRb != null)
        {
            pickupRb.isKinematic = true;
        }

        pickup.gameObject.SetActive(false);

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

        Vector3 pos = dropPoint != null
            ? dropPoint.position
            : transform.position + transform.forward * 1.0f + Vector3.up * 0.2f;

        Quaternion rot = dropPoint != null ? dropPoint.rotation : Quaternion.identity;

        if (item.pickupData.worldPrefab != null)
        {
            GameObject worldInstance = Instantiate(item.pickupData.worldPrefab, pos, rot);

            PickupItem newPickup = worldInstance.GetComponent<PickupItem>();
            if (newPickup == null)
            {
                newPickup = worldInstance.AddComponent<PickupItem>();
            }
            newPickup.itemName = item.pickupData.itemName;
            newPickup.heldPrefab = item.pickupData.heldPrefab;
            newPickup.worldPrefab = item.pickupData.worldPrefab;

            Rigidbody worldRb = worldInstance.GetComponent<Rigidbody>();
            if (worldRb != null)
            {
                worldRb.isKinematic = false;
            }

            Destroy(item.pickupData.gameObject);
        }
        else
        {
            item.pickupData.gameObject.SetActive(true);
            item.pickupData.transform.position = pos;
            item.pickupData.transform.rotation = rot;

            Rigidbody pickupRb = item.pickupData.GetComponent<Rigidbody>();
            if (pickupRb != null)
            {
                pickupRb.isKinematic = false;
                pickupRb.velocity = Vector3.zero;
                pickupRb.angularVelocity = Vector3.zero;
            }
        }

        Debug.Log($"[Inventory] Dropped: {item.pickupData.itemName} | pos: {pos}");

        if (item.heldObject != null)
        {
            Destroy(item.heldObject);
        }

        items.RemoveAt(currentIndex);

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

    void UseCurrentItem()
    {
        if (currentIndex < 0 || currentIndex >= items.Count)
        {
            Debug.LogWarning("[Inventory] UseCurrentItem called with invalid index.");
            return;
        }

        HeldItem item = items[currentIndex];

        if (item == null || item.pickupData == null)
        {
            Debug.LogWarning("[Inventory] HeldItem or pickupData is null.");
            return;
        }

        UsableItem usable = item.pickupData.GetComponent<UsableItem>();

        if (usable != null)
        {
            Debug.Log($"[Inventory] Using: {item.pickupData.itemName}");
            usable.Use(this);

            if (usable.isConsumable)
            {
                RemoveCurrentItem();
            }
        }
        else
        {
            Debug.Log($"[Inventory] {item.pickupData.itemName} cannot be used directly. Try dropping it in a room.");
        }
    }

    void RemoveCurrentItem()
    {
        if (currentIndex < 0 || currentIndex >= items.Count)
        {
            Debug.LogWarning("[Inventory] RemoveCurrentItem called with invalid index.");
            return;
        }

        HeldItem item = items[currentIndex];

        Debug.Log($"[Inventory] Consumed: {item.pickupData.itemName}");

        if (item.heldObject != null)
        {
            Destroy(item.heldObject);
        }

        if (item.pickupData != null && item.pickupData.gameObject != null)
        {
            Destroy(item.pickupData.gameObject);
        }

        items.RemoveAt(currentIndex);

        currentIndex = items.Count == 0 ? -1 : Mathf.Clamp(currentIndex, 0, items.Count - 1);
        ShowCurrentItem();
    }
}