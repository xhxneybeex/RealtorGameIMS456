using UnityEngine;

public class UsableSage : UsableItem
{
    public float protectionDuration = 20f;

    public override void Use(PlayerInventory inventory)
    {
        RoomManager[] allRooms = FindObjectsOfType<RoomManager>();

        if (allRooms.Length == 0)
        {
            Debug.LogWarning("[Sage] No rooms found in the scene.");
            return;
        }

        int roomsProtected = 0;
        foreach (RoomManager room in allRooms)
        {
            room.ActivateSageProtection(protectionDuration);
            roomsProtected++;
        }

        Debug.Log($"<color=green>[Sage]</color> Protected {roomsProtected} room(s) for {protectionDuration} seconds!");
    }
}
