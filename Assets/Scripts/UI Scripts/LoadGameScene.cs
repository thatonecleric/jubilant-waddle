using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScene : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Loading Game.");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Loading Main Menu.");
    }

    public void LoadSettingsMenu()
    {
        SceneManager.LoadScene("SettingsMenu");
        Debug.Log("Loading Setting Menu.");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
