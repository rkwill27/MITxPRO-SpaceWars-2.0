using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string athenaLvl1Scene;

    void Start()
    {
        AudioManager.instance.PlayMenuMusic();
    }

    public void StartGame()
    {
        // Optional: fade out menu music here if you want.
        AudioManager.instance.StopMusic();
        SceneManager.LoadScene(athenaLvl1Scene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quitting Game");
    }
}
