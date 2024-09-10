using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_MainMenu : MonoBehaviour
{
    public string newGameScene;

    public void newGame()
    {
        SceneManager.LoadScene(newGameScene);
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
