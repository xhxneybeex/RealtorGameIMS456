using UnityEngine;

public class NPCGhostAwareness : MonoBehaviour
{
    public RoomManager currentRoom;
    bool alreadyScared = false;

    void Update()
    {
        if (currentRoom == null) return;

        if (currentRoom.isHaunted)
        {
            if (!alreadyScared)
            {
                Debug.Log(name + " got scared in " + currentRoom.roomName);

                if (GameManager.Instance != null)
                    GameManager.Instance.NPCGotScared();

                alreadyScared = true;
            }
        }
        else
        {
            // room is calm again, we can be scared again later
            alreadyScared = false;
        }
    }

    // Detect entering a room trigger
    void OnTriggerEnter(Collider other)
    {
        RoomManager room = other.GetComponent<RoomManager>();
        if (room != null)
        {
            currentRoom = room;
            Debug.Log(name + " entered " + room.roomName);
        }
    }

    // Detect leaving that room
    void OnTriggerExit(Collider other)
    {
        RoomManager room = other.GetComponent<RoomManager>();
        if (room != null && room == currentRoom)
        {
            Debug.Log(name + " left " + room.roomName);
            currentRoom = null;
        }
    }
}
