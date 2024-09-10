using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_SceneSwitcher : MonoBehaviour
{
    public void ChangeToMainMenu()
    {
        SceneManager.LoadScene("Scene_MainMenu");
    }
}
