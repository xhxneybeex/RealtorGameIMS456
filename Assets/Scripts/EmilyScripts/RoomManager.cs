using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public enum HauntType
    {
        None,
        ColdRoom,
        FlickerLights,
        BadSmell,
        FloatingObjects,
        BloodStain
    }

    [Header("Basic Info")]
    public string roomName = "Unnamed Room";

    // This is what NPCGhostAwareness is looking for
    [Tooltip("True while any haunt is active.")]
    public bool isHaunted = false;

    [Header("Timing")]
    public float minDelay = 20f;
    public float maxDelay = 60f;   // 1 minute max like you asked

    float nextHauntTime;
    HauntType currentHaunt = HauntType.None;
    HauntType lastHaunt = HauntType.None;

    [Header("Cold Room")]
    public GameObject coldRoomVFX;
    public bool heaterInside = false;

    void Start()
    {
        ScheduleNextHaunt();
    }

    void Update()
    {
        if (currentHaunt == HauntType.None)
        {
            if (Time.time >= nextHauntTime)
            {
                StartRandomHaunt();
            }
        }
        else
        {
            if (currentHaunt == HauntType.ColdRoom && heaterInside)
            {
                Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater detected during cold room. Ending haunt.");
                StopCurrentHaunt();
            }
        }
    }

    void ScheduleNextHaunt()
    {
        float delay = Random.Range(minDelay, maxDelay);
        nextHauntTime = Time.time + delay;

        Debug.Log($"<color=yellow>[Room {roomName}]</color> Next haunt scheduled in <b>{delay:F1}</b> seconds.");
    }

    void StartRandomHaunt()
    {
        HauntType chosen = GetRandomHauntType();

        if (chosen == HauntType.None)
        {
            Debug.LogWarning($"[Room {roomName}] WARNING: No haunt types available.");
            ScheduleNextHaunt();
            return;
        }

        currentHaunt = chosen;
        lastHaunt = chosen;
        isHaunted = true;   // <<< important for NPCGhostAwareness

        Debug.Log($"<color=magenta>[Room {roomName}]</color> Haunt started: <b>{currentHaunt}</b>");

        switch (currentHaunt)
        {
            case HauntType.ColdRoom:
                if (coldRoomVFX != null)
                    coldRoomVFX.SetActive(true);

                Debug.Log($"<color=cyan>[Room {roomName}]</color> Cold room VFX activated.");
                break;
        }
    }

    HauntType GetRandomHauntType()
    {
        HauntType[] possible =
        {
            HauntType.ColdRoom
            // add more later
        };

        HauntType chosen = possible[Random.Range(0, possible.Length)];

        Debug.Log($"[Room {roomName}] Random haunt chosen: {chosen}");

        return chosen;
    }

    void StopCurrentHaunt()
    {
        Debug.Log($"<color=green>[Room {roomName}]</color> Haunt ended: <b>{currentHaunt}</b>");

        switch (currentHaunt)
        {
            case HauntType.ColdRoom:
                if (coldRoomVFX != null)
                    coldRoomVFX.SetActive(false);
                Debug.Log($"<color=cyan>[Room {roomName}]</color> Cold room VFX disabled.");
                break;
        }

        currentHaunt = HauntType.None;
        isHaunted = false;   // <<< tell NPCs room is calm again

        ScheduleNextHaunt();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Heater"))
        {
            heaterInside = true;
            Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater ENTERED room.");

            if (currentHaunt == HauntType.ColdRoom)
            {
                Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater fixes cold room instantly.");
                StopCurrentHaunt();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Heater"))
        {
            heaterInside = false;
            Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater LEFT room.");
        }
    }
}
