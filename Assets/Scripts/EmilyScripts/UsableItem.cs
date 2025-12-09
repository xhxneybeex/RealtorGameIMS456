using UnityEngine;

public abstract class UsableItem : MonoBehaviour
{
    public string useName = "Use Item";
    public bool isConsumable = true;
    public float useRange = 5f;

    public abstract void Use(PlayerInventory inventory);

    protected RoomManager FindNearestRoom(Vector3 playerPosition)
    {
        RoomManager[] rooms = FindObjectsOfType<RoomManager>();
        RoomManager nearest = null;
        float minDist = useRange;

        foreach (RoomManager room in rooms)
        {
            float dist = Vector3.Distance(playerPosition, room.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = room;
            }
        }

        return nearest;
    }
}
