using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>In-game HUD: health bar, live score / high score, pause + game-over panels.</summary>
public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("HUD")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private float gameOverDelay = 1f;

    private void Start()
    {
        healthBar.wholeNumbers = true;
        healthBar.minValue = 0;
        healthBar.maxValue = player.MaxLives;
        healthBar.value = player.Lives;

        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        player.LivesChanged += OnLivesChanged;
        GameManager gm = GameManager.Instance;
        gm.ScoreChanged += OnScoreChanged;
        gm.StateChanged += OnStateChanged;

        OnScoreChanged(gm.Score, gm.HighScore);
    }

    private void OnDestroy()
    {
        if (player != null) player.LivesChanged -= OnLivesChanged;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ScoreChanged -= OnScoreChanged;
            GameManager.Instance.StateChanged -= OnStateChanged;
        }
    }

    private void OnLivesChanged(int lives) => healthBar.value = lives;

    private void OnScoreChanged(int score, int high)
    {
        scoreText.text = $"Score: {score:D5}";
        highScoreText.text = $"Best: {high:D5}";
    }

    private void OnStateChanged(GameState state)
    {
        pausePanel.SetActive(state == GameState.Paused);

        if (state == GameState.GameOver)
            StartCoroutine(ShowGameOver());
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);   // let the Death animation play
        if (finalScoreText != null)
            finalScoreText.text = $"Score: {GameManager.Instance.Score:D5}\nBest: {GameManager.Instance.HighScore:D5}";
        gameOverPanel.SetActive(true);
    }
}
