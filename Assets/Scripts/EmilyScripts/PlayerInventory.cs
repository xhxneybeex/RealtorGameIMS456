using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class HeldItem
    {
        public PickupItem worldItem;   // original in the scene
        public GameObject heldObject;  // instance in the hand
    }

    [Header("Inventory Settings")]
    public int maxSlots = 3;

    [Header("Hand Setup")]
    public Transform handSocket;      // where items appear (child of camera)

    private List<HeldItem> items = new List<HeldItem>();
    private int currentIndex = -1;

    void Update()
    {
        HandleScrollInput();
    }

    void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (items.Count == 0) return;

        if (scroll > 0f)
        {
            currentIndex++;
            if (currentIndex >= items.Count) currentIndex = 0;
            ShowCurrentItem();
        }
        else if (scroll < 0f)
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = items.Count - 1;
            ShowCurrentItem();
        }
    }

    public bool TryAddItem(PickupItem pickup)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventory full!");
            return false;
        }

        if (pickup.heldPrefab == null)
        {
            Debug.LogWarning("Pickup item has no heldPrefab assigned.");
            return false;
        }

        // Remove the world object so it doesn’t interfere later
        Destroy(pickup.gameObject);

        // Spawn held version in hand, but keep it hidden until selected
        GameObject heldInstance = Instantiate(pickup.heldPrefab, handSocket);
        heldInstance.transform.localPosition = Vector3.zero;
        heldInstance.transform.localRotation = Quaternion.identity;
        heldInstance.SetActive(false);

        HeldItem newItem = new HeldItem
        {
            worldItem = pickup,
            heldObject = heldInstance
        };

        items.Add(newItem);

        // If this is the first item, auto select it
        if (currentIndex == -1)
        {
            currentIndex = 0;
            ShowCurrentItem();
        }

        return true;
    }

    void ShowCurrentItem()
    {
        // Turn off all held items except the current one
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].heldObject != null)
                items[i].heldObject.SetActive(i == currentIndex);
        }
    }
}
