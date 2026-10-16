using UnityEngine;

/// <summary>
/// Turns maze grid data into actual GameObjects (floor + walls).
/// Cell (x, y) is centred at world position (x * cellSize, 0, y * cellSize).
/// </summary>
public static class MazeBuilder
{
    public static GameObject Build(MazeSettings s, MazeCell[,] grid, GameObject wallPrefab)
    {
        var root = new GameObject("Maze");

        BuildFloor(s, root.transform);

        for (int x = 0; x < s.width; x++)
        {
            for (int y = 0; y < s.height; y++)
            {
                var cell = grid[x, y];
                Vector3 center = new Vector3(x * s.cellSize, 0f, y * s.cellSize);

                // Each shared wall is built once: North and East for every cell...
                if (cell.walls[MazeGenerator.North]) SpawnWall(s, root.transform, wallPrefab, center, true, 1);
                if (cell.walls[MazeGenerator.East]) SpawnWall(s, root.transform, wallPrefab, center, false, 1);

                // ...plus South and West only on the outer border.
                if (y == 0 && cell.walls[MazeGenerator.South]) SpawnWall(s, root.transform, wallPrefab, center, true, -1);
                if (x == 0 && cell.walls[MazeGenerator.West]) SpawnWall(s, root.transform, wallPrefab, center, false, -1);
            }
        }

        return root;
    }

    static void BuildFloor(MazeSettings s, Transform parent)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(parent);

        float totalW = s.width * s.cellSize;
        float totalH = s.height * s.cellSize;
        floor.transform.localScale = new Vector3(totalW, 0.2f, totalH);
        floor.transform.localPosition = new Vector3((s.width - 1) * s.cellSize / 2f, -0.1f, (s.height - 1) * s.cellSize / 2f);
    }

    /// <param name="alongX">true = wall runs along X (North/South), false = along Z (East/West)</param>
    /// <param name="sign">+1 = North/East side of the cell, -1 = South/West side</param>
    static void SpawnWall(MazeSettings s, Transform parent, GameObject prefab, Vector3 cellCenter, bool alongX, int sign)
    {
        // Assumes the prefab is a 1x1x1 cube; we scale it to the wall size.
        GameObject wall = prefab != null ? Object.Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.SetParent(parent);

        float length = s.cellSize + s.wallThickness; // overlap corners so there are no gaps
        float offset = s.cellSize / 2f * sign;
        float y = s.wallHeight / 2f;

        if (alongX)
        {
            wall.transform.localPosition = cellCenter + new Vector3(0f, y, offset);
            wall.transform.localScale = new Vector3(length, s.wallHeight, s.wallThickness);
        }
        else
        {
            wall.transform.localPosition = cellCenter + new Vector3(offset, y, 0f);
            wall.transform.localScale = new Vector3(s.wallThickness, s.wallHeight, length);
        }
    }
}
