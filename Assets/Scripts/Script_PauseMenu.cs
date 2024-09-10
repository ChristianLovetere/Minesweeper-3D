using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Script_PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public static bool gameOver = false;
    public static bool isReviewing = false;

    private GameObject pauseMenuPanel;

    private void Start()
    {
        pauseMenuPanel = GameObject.Find("Panel_PauseMenu");
        pauseMenuPanel.SetActive(false);
    }
    public void Pause()
    {
        Time.timeScale = 0; // Pause the game
        pauseMenuPanel.SetActive(true);
        isPaused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1; // Resume the game
        pauseMenuPanel.SetActive(false);
        isPaused = false;
    }

    public void StartReviewing()
    {
        isReviewing = true;
    }

    public void StopReviewing()
    {
        isReviewing = false;
    }
    public void ResetBools()
    {
        gameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameOver)
                return;
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
}
