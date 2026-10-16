using System.Globalization;
using UnityEngine;

/// <summary>
/// Scene names and small helpers shared by the menu and in-game UI.
/// </summary>
public static class GameConstants
{
    public const string MenuScene = "MainMenu";
    public const string GameScene = "Game";
    public const string BestTimeKey = "BestTime";

    /// <summary>Formats seconds as MM:SS.s, for example 01:05.3</summary>
    public static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float secs = seconds - minutes * 60f;
        return minutes.ToString("00") + ":" + secs.ToString("00.0", CultureInfo.InvariantCulture);
    }
}
