using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central game state. Enemies, traps and the treasure call into this;
/// the UI and juice effects listen to its events.
/// Needs to exist once in the scene (empty GameObject).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }
    public bool HasWon { get; private set; }

    /// <summary>Seconds since the level started. Stops on win or game over.</summary>
    public float ElapsedTime { get; private set; }

    public event Action OnGameOver;   // the moment the player loses (sounds, shake)
    public event Action OnWin;        // the moment the player wins
    public event Action OnEndScreen;  // after the slow-motion moment (show the UI)

    PlayerHealth playerHealth;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f; // make sure a previous run didn't leave time frozen
    }

    void Start()
    {
        // Dying from damage also ends the game
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null) playerHealth.OnDied += GameOver;
    }

    void Update()
    {
        if (!IsGameOver && !HasWon) ElapsedTime += Time.deltaTime;
    }

    void OnDestroy()
    {
        if (playerHealth != null) playerHealth.OnDied -= GameOver;
        if (Instance == this) Instance = null;
    }

    public void GameOver()
    {
        if (IsGameOver || HasWon) return;

        IsGameOver = true;
        Debug.Log("GAME OVER");
        OnGameOver?.Invoke();

        StartCoroutine(EndSequence(0.2f, 1.0f));
    }

    public void Win()
    {
        if (IsGameOver || HasWon) return;

        HasWon = true;
        Debug.Log("YOU WIN");
        OnWin?.Invoke();

        StartCoroutine(EndSequence(0.35f, 1.2f));
    }

    // Slow motion for a moment, then freeze and show the end screen
    IEnumerator EndSequence(float slowMotionScale, float realSeconds)
    {
        var playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput != null) playerInput.DeactivateInput();

        Time.timeScale = slowMotionScale;
        yield return new WaitForSecondsRealtime(realSeconds); // real time, unaffected by slow motion

        Time.timeScale = 0f;
        OnEndScreen?.Invoke();
    }
}
