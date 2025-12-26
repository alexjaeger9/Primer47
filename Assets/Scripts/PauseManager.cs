using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public UIManager uiManager;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //wenn Cursor locked + ESC = Pause
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                PauseGame();
            }
            //wenn Cursor unlocked + ESC = Unpause
            else if (isPaused)
            {
                ResumeGame();
            }
        }
    }

    void PauseGame()
    {
        isPaused = true;
        uiManager.ShowPause();
    }

    void ResumeGame()
    {
        isPaused = false;
        uiManager.HidePause();
    }
}