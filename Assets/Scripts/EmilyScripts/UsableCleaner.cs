using UnityEngine;

public class UsableCleaner : UsableItem
{
    public override void Use(PlayerInventory inventory)
    {
        Vector3 playerPosition = inventory.transform.position;
        RoomManager nearestRoom = FindNearestRoom(playerPosition);

        if (nearestRoom == null)
        {
            Debug.LogWarning($"[Cleaner] No room nearby to clean. Use range: {useRange}m");
            return;
        }

        Debug.Log($"[Cleaner] Nearest room: {nearestRoom.roomName}, Current haunt: {nearestRoom.currentHaunt}");

        if (nearestRoom.currentHaunt == RoomManager.HauntType.BloodStain)
        {
            nearestRoom.CleanBloodStain();
            Debug.Log($"<color=cyan>[Cleaner]</color> Cleaned blood stain in {nearestRoom.roomName}!");
        }
        else
        {
            Debug.Log($"[Cleaner] No blood stain active in {nearestRoom.roomName}. Current haunt: {nearestRoom.currentHaunt}");
        }
    }
}
