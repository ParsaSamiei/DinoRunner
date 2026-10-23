using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Main menu scene logic. Hook the methods up to Button OnClick events.</summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private TMP_Text highScoreText;

    private void Start()
    {
        Time.timeScale = 1f;
        if (highScoreText != null)
            highScoreText.text = $"Best: {PlayerPrefs.GetInt("HighScore", 0):D5}";
    }

    public void Play() => SceneManager.LoadScene(gameSceneName);

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
