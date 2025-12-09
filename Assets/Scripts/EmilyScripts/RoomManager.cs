using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public enum HauntType
    {
        None,
        ColdRoom,
        FlickerLights,
        BadSmell,
        BloodStain
    }

    [Header("Basic Info")]
    public string roomName = "Unnamed Room";

    [Tooltip("True while any haunt is active.")]
    public bool isHaunted = false;

    [Header("Timing")]
    public float minDelay = 20f;
    public float maxDelay = 60f;

    private float nextHauntTime;
    public HauntType currentHaunt = HauntType.None;
    private HauntType lastHaunt = HauntType.None;

    [Header("Cold Room")]
    public GameObject coldRoomVFX;
    private bool heaterInside = false;

    [Header("Flicker Lights")]
    public Light[] lightsToFlicker;
    private bool hasWorkingBulb = false;
    private bool[] originalLightStates;
    private float[] originalLightIntensities;

    [Header("Bad Smell")]
    public GameObject smellVFX;
    private bool candleInside = false;

    [Header("Blood Stain")]
    public GameObject bloodStainVFX;
    private bool bloodCleaned = false;

    [Header("Sage Protection")]
    private bool sageProtectionActive = false;
    private float sageProtectionEndTime = 0f;

    void Start()
    {
        ScheduleNextHaunt();
        StoreLightStates();
    }

    void StoreLightStates()
    {
        if (lightsToFlicker != null && lightsToFlicker.Length > 0)
        {
            originalLightStates = new bool[lightsToFlicker.Length];
            originalLightIntensities = new float[lightsToFlicker.Length];

            for (int i = 0; i < lightsToFlicker.Length; i++)
            {
                if (lightsToFlicker[i] != null)
                {
                    originalLightStates[i] = lightsToFlicker[i].enabled;
                    originalLightIntensities[i] = lightsToFlicker[i].intensity;
                }
            }
        }
    }

    void Update()
    {
        if (sageProtectionActive)
        {
            if (Time.time >= sageProtectionEndTime)
            {
                sageProtectionActive = false;
                Debug.Log($"<color=yellow>[Room {roomName}]</color> Sage protection expired.");
                
                if (currentHaunt == HauntType.None)
                {
                    ScheduleNextHaunt();
                }
            }
        }

        if (currentHaunt == HauntType.None)
        {
            if (!sageProtectionActive && Time.time >= nextHauntTime)
            {
                StartRandomHaunt();
            }
        }
        else
        {
            CheckHauntSolutions();
        }
    }

    void CheckHauntSolutions()
    {
        switch (currentHaunt)
        {
            case HauntType.ColdRoom:
                if (heaterInside)
                {
                    Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater detected during cold room. Ending haunt.");
                    StopCurrentHaunt();
                }
                break;

            case HauntType.FlickerLights:
                if (hasWorkingBulb)
                {
                    Debug.Log($"<color=yellow>[Room {roomName}]</color> Light bulb installed. Ending flicker.");
                    StopCurrentHaunt();
                }
                break;

            case HauntType.BadSmell:
                if (candleInside)
                {
                    Debug.Log($"<color=orange>[Room {roomName}]</color> Candle placed. Ending bad smell.");
                    StopCurrentHaunt();
                }
                break;

            case HauntType.BloodStain:
                if (bloodCleaned)
                {
                    Debug.Log($"<color=red>[Room {roomName}]</color> Blood cleaned. Ending haunt.");
                    StopCurrentHaunt();
                }
                break;
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
        if (sageProtectionActive)
        {
            Debug.Log($"<color=green>[Room {roomName}]</color> Sage protection is active. Skipping haunt.");
            ScheduleNextHaunt();
            return;
        }

        HauntType chosen = GetRandomHauntType();

        if (chosen == HauntType.None)
        {
            Debug.LogWarning($"[Room {roomName}] WARNING: No haunt types available.");
            ScheduleNextHaunt();
            return;
        }

        currentHaunt = chosen;
        lastHaunt = chosen;
        isHaunted = true;

        Debug.Log($"<color=magenta>[Room {roomName}]</color> Haunt started: <b>{currentHaunt}</b>");

        switch (currentHaunt)
        {
            case HauntType.ColdRoom:
                if (coldRoomVFX != null)
                {
                    coldRoomVFX.SetActive(true);
                }
                Debug.Log($"<color=cyan>[Room {roomName}]</color> Cold room VFX activated.");
                break;

            case HauntType.FlickerLights:
                StartFlickeringLights();
                Debug.Log($"<color=yellow>[Room {roomName}]</color> Lights started flickering.");
                break;

            case HauntType.BadSmell:
                if (smellVFX != null)
                {
                    smellVFX.SetActive(true);
                }
                Debug.Log($"<color=orange>[Room {roomName}]</color> Bad smell VFX activated.");
                break;

            case HauntType.BloodStain:
                if (bloodStainVFX != null)
                {
                    bloodStainVFX.SetActive(true);
                }
                bloodCleaned = false;
                Debug.Log($"<color=red>[Room {roomName}]</color> Blood stain appeared.");
                break;
        }
    }

    HauntType GetRandomHauntType()
    {
        HauntType[] possible =
        {
            HauntType.ColdRoom,
            HauntType.FlickerLights,
            HauntType.BadSmell,
            HauntType.BloodStain
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
                {
                    coldRoomVFX.SetActive(false);
                }
                Debug.Log($"<color=cyan>[Room {roomName}]</color> Cold room VFX disabled.");
                break;

            case HauntType.FlickerLights:
                StopFlickeringLights();
                Debug.Log($"<color=yellow>[Room {roomName}]</color> Lights stopped flickering.");
                break;

            case HauntType.BadSmell:
                if (smellVFX != null)
                {
                    smellVFX.SetActive(false);
                }
                Debug.Log($"<color=orange>[Room {roomName}]</color> Bad smell VFX disabled.");
                break;

            case HauntType.BloodStain:
                if (bloodStainVFX != null)
                {
                    bloodStainVFX.SetActive(false);
                }
                Debug.Log($"<color=red>[Room {roomName}]</color> Blood stain removed.");
                break;
        }

        currentHaunt = HauntType.None;
        isHaunted = false;

        ScheduleNextHaunt();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HeaterItem>() != null)
        {
            heaterInside = true;
            Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater ENTERED room.");

            if (currentHaunt == HauntType.ColdRoom)
            {
                Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater fixes cold room instantly.");
                StopCurrentHaunt();
            }
        }
        else if (other.GetComponent<CandleItem>() != null)
        {
            candleInside = true;
            Debug.Log($"<color=orange>[Room {roomName}]</color> Candle ENTERED room.");

            if (currentHaunt == HauntType.BadSmell)
            {
                Debug.Log($"<color=orange>[Room {roomName}]</color> Candle fixes bad smell instantly.");
                StopCurrentHaunt();
            }
        }
        else if (other.GetComponent<LightBulbItem>() != null)
        {
            hasWorkingBulb = true;
            Debug.Log($"<color=yellow>[Room {roomName}]</color> Light bulb ENTERED room.");

            if (currentHaunt == HauntType.FlickerLights)
            {
                Debug.Log($"<color=yellow>[Room {roomName}]</color> Light bulb fixes flickering instantly.");
                StopCurrentHaunt();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HeaterItem>() != null)
        {
            heaterInside = false;
            Debug.Log($"<color=cyan>[Room {roomName}]</color> Heater LEFT room.");
        }
        else if (other.GetComponent<CandleItem>() != null)
        {
            candleInside = false;
            Debug.Log($"<color=orange>[Room {roomName}]</color> Candle LEFT room.");
        }
        else if (other.GetComponent<LightBulbItem>() != null)
        {
            hasWorkingBulb = false;
            Debug.Log($"<color=yellow>[Room {roomName}]</color> Light bulb LEFT room.");
        }
    }

    void StartFlickeringLights()
    {
        if (lightsToFlicker == null || lightsToFlicker.Length == 0)
        {
            Debug.LogWarning($"[Room {roomName}] No lights assigned to flicker!");
            return;
        }

        foreach (Light light in lightsToFlicker)
        {
            if (light != null)
            {
                LightFlicker flicker = light.GetComponent<LightFlicker>();
                if (flicker == null)
                {
                    flicker = light.gameObject.AddComponent<LightFlicker>();
                }
                flicker.enabled = true;
                flicker.StartFlickering();
            }
        }
    }

    void StopFlickeringLights()
    {
        if (lightsToFlicker == null || lightsToFlicker.Length == 0)
        {
            return;
        }

        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
            {
                LightFlicker flicker = lightsToFlicker[i].GetComponent<LightFlicker>();
                if (flicker != null)
                {
                    flicker.StopFlickering();
                    flicker.enabled = false;
                }

                if (originalLightStates != null && i < originalLightStates.Length)
                {
                    lightsToFlicker[i].enabled = originalLightStates[i];
                    lightsToFlicker[i].intensity = originalLightIntensities[i];
                }
            }
        }
    }

    public void ActivateSageProtection(float duration)
    {
        sageProtectionActive = true;
        sageProtectionEndTime = Time.time + duration;

        Debug.Log($"<color=green>[Room {roomName}]</color> Sage protection activated for {duration} seconds.");

        if (currentHaunt != HauntType.None)
        {
            StopCurrentHaunt();
        }
    }

    public void CleanBloodStain()
    {
        bloodCleaned = true;

        if (currentHaunt == HauntType.BloodStain)
        {
            StopCurrentHaunt();
        }
    }
}
