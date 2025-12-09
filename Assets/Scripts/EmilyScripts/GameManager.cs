using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scoring")]
    public int houseScore = 100;

    [Header("Game Timer")]
    public float gameDuration = 300f;
    public bool gameActive = true;

    private float timeRemaining;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timeRemaining = gameDuration;
    }

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            EndGame();
        }
    }

    public void NPCGotScared()
    {
        houseScore -= 20;
        if (houseScore < 0) houseScore = 0;

        Debug.Log("House score is now: " + houseScore);
    }

    void EndGame()
    {
        gameActive = false;
        timeRemaining = 0f;

        Debug.Log($"<color=yellow>GAME OVER!</color> Final Score: {houseScore}");
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    public string GetTimeRemainingFormatted()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
