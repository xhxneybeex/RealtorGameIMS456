using UnityEngine;
using UnityEngine.SceneManagement;

public class Change : MonoBehaviour
{
    // Loads a specific scene by name
    public void StartGame()
    {
        SceneManager.LoadScene("LevelSelection"); // change to your scene name
    }

    // Quits the entire game/application
    public void QuitGame()
    {
        // This works when the game is built
        Application.Quit();

        // This is just so you can see it works in the editor
        Debug.Log("Quit Game!");
    }
}
