using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    private bool isPaused = false;
    public static bool canPause = true;
    public static bool isGameOver = false;

    void Update()
    {
        //wenn pausieren nicht möglich oder gameover -> return
        if (!canPause || isGameOver) return;

        //Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused) PauseGame();
            else ResumeGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        pausePanel.SetActive(true);

        //Cursor freigeben
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        pausePanel.SetActive(false);

        //Cursor wieder locken
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}