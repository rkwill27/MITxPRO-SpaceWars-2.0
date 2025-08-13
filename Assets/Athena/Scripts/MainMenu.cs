using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string athenaLvl1Scene;

    // Start is called before the first frame update
    void Start()
    {
        //AudioManager.instance.PlayMenuMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(athenaLvl1Scene);

        //AudioManager.instance.PlaySFX(0);
    }

    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Quitting Game");

        //AudioManager.instance.PlaySFX(0);
    }
}
