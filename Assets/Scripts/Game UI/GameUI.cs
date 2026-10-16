using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// In-game UI: health pips, timer, damage flash, and the Game Over / Win screens.
/// Put this on an empty GameObject in the Game scene. Everything is built from code.
/// </summary>
public class GameUI : MonoBehaviour
{
    static readonly Color Gold = new Color(1f, 0.82f, 0.25f);
    static readonly Color PipFull = new Color(0.9f, 0.2f, 0.25f, 1f);
    static readonly Color PipEmpty = new Color(0.2f, 0.2f, 0.25f, 0.7f);

    Canvas canvas;
    Text timerText;
    Image damageFlash;
    Image[] pips;

    GameObject gameOverPanel;
    GameObject winPanel;
    Text gameOverTimeText;
    Text winTimeText;
    Text winBestText;

    PlayerHealth health;
    float flashAlpha;

    void Awake()
    {
        BuildUI();
    }

    void Start()
    {
        // Hide the cursor while playing (it is freed again on the end screens)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            health.OnHealthChanged += UpdateHealth;
            health.OnDamaged += OnDamaged;
            UpdateHealth(health.Current, health.Max);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEndScreen += ShowEndScreen;
        }
        else
        {
            Debug.LogError("GameUI: no GameManager in the scene.");
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= UpdateHealth;
            health.OnDamaged -= OnDamaged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEndScreen -= ShowEndScreen;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null)
            timerText.text = GameConstants.FormatTime(GameManager.Instance.ElapsedTime);

        // Fade the red damage flash (unscaled so it still fades if time is frozen)
        if (flashAlpha > 0f)
        {
            flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, Time.unscaledDeltaTime * 2f);
            damageFlash.color = new Color(0.8f, 0f, 0f, flashAlpha);
        }
    }

    // ---------- Building ----------

    void BuildUI()
    {
        canvas = UIFactory.CreateCanvas("GameCanvas");
        var topLeft = new Vector2(0f, 1f);
        var topCenter = new Vector2(0.5f, 1f);

        damageFlash = UIFactory.CreatePanel(canvas.transform, "DamageFlash", new Color(0.8f, 0f, 0f, 0f), false)
                               .GetComponent<Image>();

        UIFactory.CreateText(canvas.transform, "HEALTH", 28, new Color(0.85f, 0.88f, 0.95f),
                             TextAnchor.UpperLeft, topLeft, new Vector2(40f, -24f), new Vector2(300f, 40f));

        timerText = UIFactory.CreateText(canvas.transform, "00:00.0", 56, Color.white,
                                         TextAnchor.UpperCenter, topCenter, new Vector2(0f, -24f), new Vector2(500f, 80f));

        gameOverPanel = BuildEndPanel("GAME OVER", new Color(0.95f, 0.25f, 0.25f), "RETRY",
                                      out gameOverTimeText, out _);
        winPanel = BuildEndPanel("TREASURE FOUND!", Gold, "PLAY AGAIN",
                                 out winTimeText, out winBestText);
    }

    // Shared layout for the Game Over and Win screens
    GameObject BuildEndPanel(string title, Color titleColor, string retryLabel, out Text timeLine, out Text extraLine)
    {
        var center = new Vector2(0.5f, 0.5f);

        var panel = UIFactory.CreatePanel(canvas.transform, title + " Panel", new Color(0f, 0f, 0f, 0.78f));

        UIFactory.CreateText(panel, title, 110, titleColor, TextAnchor.MiddleCenter,
                             center, new Vector2(0f, 230f), new Vector2(1500f, 150f));

        timeLine = UIFactory.CreateText(panel, "", 48, Color.white, TextAnchor.MiddleCenter,
                                        center, new Vector2(0f, 120f), new Vector2(1000f, 70f));

        extraLine = UIFactory.CreateText(panel, "", 40, Gold, TextAnchor.MiddleCenter,
                                         center, new Vector2(0f, 60f), new Vector2(1000f, 60f));

        UIFactory.CreateButton(panel, retryLabel, new Vector2(0f, -60f), new Vector2(460f, 90f), Retry);
        UIFactory.CreateButton(panel, "MAIN MENU", new Vector2(0f, -170f), new Vector2(460f, 90f), ToMenu);

        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    // ---------- Health ----------

    void UpdateHealth(int current, int max)
    {
        if (pips == null || pips.Length != max) BuildPips(max);

        for (int i = 0; i < pips.Length; i++)
            pips[i].color = i < current ? PipFull : PipEmpty;
    }

    void BuildPips(int max)
    {
        if (pips != null)
            foreach (var old in pips) Destroy(old.gameObject);

        pips = new Image[max];
        for (int i = 0; i < max; i++)
        {
            pips[i] = UIFactory.CreateImage(canvas.transform, "Pip " + (i + 1), PipFull,
                                            new Vector2(0f, 1f), new Vector2(40f + i * 64f, -72f), new Vector2(52f, 52f));
        }
    }

    void OnDamaged()
    {
        flashAlpha = 0.45f;
    }

    // ---------- End screens ----------

    // Called after the slow-motion moment; picks the right screen
    void ShowEndScreen()
    {
        if (GameManager.Instance.HasWon) ShowWin();
        else ShowGameOver();
    }

    void ShowGameOver()
    {
        gameOverTimeText.text = "Time survived: " + GameConstants.FormatTime(GameManager.Instance.ElapsedTime);
        gameOverPanel.SetActive(true);
        EnterMenuMode();
    }

    void ShowWin()
    {
        float time = GameManager.Instance.ElapsedTime;
        float best = PlayerPrefs.GetFloat(GameConstants.BestTimeKey, float.MaxValue);
        bool newBest = time < best;

        if (newBest)
        {
            PlayerPrefs.SetFloat(GameConstants.BestTimeKey, time);
            PlayerPrefs.Save();
            best = time;
        }

        winTimeText.text = "Your time: " + GameConstants.FormatTime(time);
        winBestText.text = newBest ? "NEW BEST TIME!" : "Best time: " + GameConstants.FormatTime(best);
        winPanel.SetActive(true);
        EnterMenuMode();
    }

    // Free the mouse for the buttons (GameManager already switched off player input)
    void EnterMenuMode()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameConstants.MenuScene);
    }
}
