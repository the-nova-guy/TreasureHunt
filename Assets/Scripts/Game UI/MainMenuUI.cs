using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds the main menu (title, Play, Quit, best time) when the scene starts.
/// Put this on an empty GameObject in the MainMenu scene.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    static readonly Color Gold = new Color(1f, 0.82f, 0.25f);

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        BuildMenu();
    }

    void BuildMenu()
    {
        var canvas = UIFactory.CreateCanvas("MainMenuCanvas");
        var center = new Vector2(0.5f, 0.5f);

        UIFactory.CreatePanel(canvas.transform, "Background", new Color(0.05f, 0.07f, 0.12f, 1f));

        UIFactory.CreateText(canvas.transform, "TREASURE HUNT", 120, Gold,
                             TextAnchor.MiddleCenter, center, new Vector2(0f, 250f), new Vector2(1400f, 160f));

        UIFactory.CreateText(canvas.transform, "Find the treasure. Avoid the guards and the traps.", 38,
                             new Color(0.75f, 0.8f, 0.9f), TextAnchor.MiddleCenter, center,
                             new Vector2(0f, 150f), new Vector2(1400f, 60f));

        UIFactory.CreateButton(canvas.transform, "PLAY", new Vector2(0f, 10f), new Vector2(420f, 90f), PlayGame);
        UIFactory.CreateButton(canvas.transform, "QUIT", new Vector2(0f, -110f), new Vector2(420f, 90f), QuitGame);

        if (PlayerPrefs.HasKey(GameConstants.BestTimeKey))
        {
            string best = GameConstants.FormatTime(PlayerPrefs.GetFloat(GameConstants.BestTimeKey));
            UIFactory.CreateText(canvas.transform, "Best time: " + best, 36, Gold,
                                 TextAnchor.MiddleCenter, center, new Vector2(0f, -240f), new Vector2(800f, 50f));
        }

        UIFactory.CreateText(canvas.transform, "WASD move  |  Mouse look  |  Shift sprint  |  Space jump", 28,
                             new Color(0.55f, 0.6f, 0.7f), TextAnchor.MiddleCenter,
                             new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1400f, 40f));
    }

    void PlayGame()
    {
        SceneManager.LoadScene(GameConstants.GameScene);
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
