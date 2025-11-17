using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int houseScore = 100;

    void Awake()
    {
        Instance = this;
    }

    public void NPCGotScared()
    {
        // drop score when an NPC gets spooked
        houseScore -= 20;
        if (houseScore < 0) houseScore = 0;

        Debug.Log("House score is now: " + houseScore);
    }
}
