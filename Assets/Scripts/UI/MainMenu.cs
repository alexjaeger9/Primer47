using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public TransitionController transitionController;
    public SettingsPanel settingsPanel;

    public Text highScoreText;
    public Text lastScoreText;

    public GameObject[] playerPoses;
    private int currentPoseIndex;
    private static int lastPoseIndex = -1;
    private float rotationSpeed = 30f;

     private void Start()
    {
        displayScores();
        SelectRandomPose();
    }

    private void Update()
    {
        if (playerPoses[currentPoseIndex] != null)
        {
            playerPoses[currentPoseIndex].transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    private void SelectRandomPose() 
    {
        if (playerPoses.Length < 2) return;

        // Alle Posen deaktivieren
        foreach (var pose in playerPoses) pose.SetActive(false);

        // Neue Pose w�hlen, die nicht die letzte war
        do
        {
            currentPoseIndex = Random.Range(0, playerPoses.Length);
        } while (currentPoseIndex == lastPoseIndex);

        // Pose aktivieren und Index speichern
        playerPoses[currentPoseIndex].SetActive(true);
        lastPoseIndex = currentPoseIndex;
    }

    private void displayScores()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
      
        highScoreText.text = "High Score: " + highScore;
        lastScoreText.text = "Last Score: " + lastScore;
    }

    public void StartGame()
    {
        StartCoroutine(StartGameWithFade());
    }

    private IEnumerator StartGameWithFade()
    {
        Time.timeScale = 1f;
        
        //1 Sekunde Fade to Black
        yield return transitionController.FadeIn(1f);
        
        //Scene laden
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        settingsPanel.OpenSettings();
    }
}