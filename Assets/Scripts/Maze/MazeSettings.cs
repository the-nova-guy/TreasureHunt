using UnityEngine;

/// <summary>
/// Designer-editable settings for one maze level.
/// </summary>
[CreateAssetMenu(menuName = "Treasure Hunt/Maze Settings")]
public class MazeSettings : ScriptableObject
{
    [Header("Maze Size")]
    public int width = 15;
    public int height = 15;
    public float cellSize = 4f;

    [Header("Walls")]
    public float wallHeight = 3f;
    public float wallThickness = 0.4f;

    [Header("Generation")]
    public int seed;

    [Header("Challenges")]
    public int trapCount = 8;
    public int enemyCount = 3;

    // Keep values in a safe range when edited in the Inspector or editor window
    void OnValidate()
    {
        width = Mathf.Max(2, width);
        height = Mathf.Max(2, height);
        cellSize = Mathf.Max(1f, cellSize);
        wallHeight = Mathf.Max(0.5f, wallHeight);
        wallThickness = Mathf.Clamp(wallThickness, 0.05f, cellSize * 0.5f);
        trapCount = Mathf.Max(0, trapCount);
        enemyCount = Mathf.Max(0, enemyCount);
    }
}
