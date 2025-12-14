using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text loopText;
    public GameObject gameOverPanel;

    public void SetLoopText(int loopIndex)
    {
        loopText.text = "Loop: " + loopIndex;
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }
}
