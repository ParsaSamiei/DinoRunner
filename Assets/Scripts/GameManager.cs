using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Playing, Paused, GameOver }

/// <summary>Owns game state, scrolling speed, score, high score (PlayerPrefs) and pause.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed")]
    [SerializeField] private float startSpeed = 6f;
    [SerializeField] private float maxSpeed = 16f;
    [SerializeField] private float acceleration = 0.1f;

    [Header("Score")]
    [SerializeField] private float scorePerSecond = 10f;

    [Header("Scenes")]
    [SerializeField] private string menuSceneName = "MainMenu";

    private const string HighScoreKey = "HighScore";

    public GameState State { get; private set; } = GameState.Playing;
    public float CurrentSpeed => State == GameState.Playing ? speed : 0f;
    public int Score => Mathf.FloorToInt(score);
    public int HighScore { get; private set; }

    /// <summary>(score, highScore)</summary>
    public event Action<int, int> ScoreChanged;
    public event Action<GameState> StateChanged;

    private float speed;
    private float score;
    private int lastNotifiedScore = -1;
    private InputAction pauseAction;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        speed = startSpeed;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Keyboard>/p");
        pauseAction.AddBinding("<Gamepad>/start");
    }

    private void OnEnable()
    {
        pauseAction.performed += OnPausePressed;
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPausePressed;
        pauseAction.Disable();
    }

    private void OnDestroy()
    {
        pauseAction?.Dispose();
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (State != GameState.Playing) return;

        speed = Mathf.MoveTowards(speed, maxSpeed, acceleration * Time.deltaTime);
        score += scorePerSecond * Time.deltaTime;
        NotifyScore();
    }

    public void AddScore(int amount)
    {
        if (State != GameState.Playing) return;
        score += amount;
        NotifyScore();
    }

    private void NotifyScore()
    {
        int s = Score;
        if (s > HighScore) HighScore = s;   // live high score
        if (s == lastNotifiedScore) return;

        lastNotifiedScore = s;
        ScoreChanged?.Invoke(s, HighScore);
    }

    private void OnPausePressed(InputAction.CallbackContext ctx) => TogglePause();

    public void TogglePause()
    {
        if (State == GameState.Playing) Pause();
        else if (State == GameState.Paused) Resume();
    }

    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        SaveHighScore();
        StateChanged?.Invoke(State);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        StateChanged?.Invoke(State);
    }

    public void GameOver()
    {
        if (State == GameState.GameOver) return;
        State = GameState.GameOver;
        SaveHighScore();
        StateChanged?.Invoke(State);
    }

    public void Restart()
    {
        SaveHighScore();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMenu()
    {
        SaveHighScore();
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    private void SaveHighScore()
    {
        if (HighScore > PlayerPrefs.GetInt(HighScoreKey, 0))
        {
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }
    }

    private void OnApplicationQuit() => SaveHighScore();
}
