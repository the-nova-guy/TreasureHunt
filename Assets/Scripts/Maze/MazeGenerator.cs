using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One grid cell. Walls are stored in the order: North, East, South, West.
/// </summary>
public class MazeCell
{
    public readonly Vector2Int pos;
    public bool visited;
    public readonly bool[] walls = { true, true, true, true };

    public MazeCell(int x, int y)
    {
        pos = new Vector2Int(x, y);
    }
}

/// <summary>
/// Pure maze logic (no GameObjects). Builds a "perfect" maze:
/// every cell is reachable and there is exactly one path between any two cells.
/// </summary>
public static class MazeGenerator
{
    public const int North = 0, East = 1, South = 2, West = 3;

    // Same order as the wall flags: N, E, S, W
    public static readonly Vector2Int[] Dirs =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    public static int Opposite(int dir) => (dir + 2) % 4;

    static bool InBounds(MazeCell[,] grid, Vector2Int p)
    {
        return p.x >= 0 && p.y >= 0 && p.x < grid.GetLength(0) && p.y < grid.GetLength(1);
    }

    /// <summary>
    /// Recursive backtracker (randomized DFS), written with a stack
    /// so large mazes don't cause a stack overflow.
    /// </summary>
    public static MazeCell[,] Generate(int width, int height, int seed)
    {
        var rng = new System.Random(seed);
        var grid = new MazeCell[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new MazeCell(x, y);

        var stack = new Stack<MazeCell>();
        var start = grid[0, 0];
        start.visited = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            var current = stack.Peek();

            // Collect directions that lead to an unvisited neighbour
            var options = new List<int>();
            for (int d = 0; d < 4; d++)
            {
                var n = current.pos + Dirs[d];
                if (InBounds(grid, n) && !grid[n.x, n.y].visited)
                    options.Add(d);
            }

            // Dead end: backtrack
            if (options.Count == 0)
            {
                stack.Pop();
                continue;
            }

            // Carve a passage to a random neighbour
            int dir = options[rng.Next(options.Count)];
            var np = current.pos + Dirs[dir];
            var next = grid[np.x, np.y];

            current.walls[dir] = false;
            next.walls[Opposite(dir)] = false;
            next.visited = true;
            stack.Push(next);
        }

        return grid;
    }

    /// <summary>
    /// BFS from 'start' through open passages. Returns the cell with the longest
    /// walking distance, a good spot for the treasure.
    /// </summary>
    public static Vector2Int FindFarthestCell(MazeCell[,] grid, Vector2Int start)
    {
        int w = grid.GetLength(0), h = grid.GetLength(1);
        var dist = new int[w, h];
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                dist[x, y] = -1;

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        dist[start.x, start.y] = 0;
        var farthest = start;

        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (dist[c.x, c.y] > dist[farthest.x, farthest.y]) farthest = c;

            for (int d = 0; d < 4; d++)
            {
                if (grid[c.x, c.y].walls[d]) continue; // wall blocks the way
                var n = c + Dirs[d];
                if (!InBounds(grid, n) || dist[n.x, n.y] != -1) continue;
                dist[n.x, n.y] = dist[c.x, c.y] + 1;
                queue.Enqueue(n);
            }
        }

        return farthest;
    }
}
