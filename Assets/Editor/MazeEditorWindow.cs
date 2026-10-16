using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Designer tool: edit maze parameters and generate a random maze in the scene
/// with one click. Open it from Tools > Treasure Hunt > Maze Generator.
/// This script must live inside an "Editor" folder.
/// </summary>
public class MazeEditorWindow : EditorWindow
{
    const string MazeRootName = "Maze";          // walls + floor (NavMesh is baked from this)
    const string ActorsRootName = "MazeActors";  // start point, treasure, enemies, traps

    [SerializeField] MazeSettings settings;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject trapPrefab;
    [SerializeField] bool randomizeSeed = true;

    Editor settingsEditor;
    Vector2 scroll;
    string status = "";

    [MenuItem("Tools/Treasure Hunt/Maze Generator")]
    static void Open()
    {
        var window = GetWindow<MazeEditorWindow>("Maze Generator");
        window.minSize = new Vector2(320, 460);
    }

    void OnDisable()
    {
        if (settingsEditor != null) DestroyImmediate(settingsEditor);
    }

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Treasure Hunt - Maze Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        settings = (MazeSettings)EditorGUILayout.ObjectField("Maze Settings", settings, typeof(MazeSettings), false);
        wallPrefab = (GameObject)EditorGUILayout.ObjectField("Wall Prefab (optional)", wallPrefab, typeof(GameObject), false);
        enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Enemy Prefab", enemyPrefab, typeof(GameObject), false);
        trapPrefab = (GameObject)EditorGUILayout.ObjectField("Trap Prefab", trapPrefab, typeof(GameObject), false);

        if (settings == null)
        {
            EditorGUILayout.HelpBox("Assign a Maze Settings asset (Create > Treasure Hunt > Maze Settings).", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        EditorGUILayout.Space();

        // Draw the settings asset's own inspector so every field is editable here
        Editor.CreateCachedEditor(settings, null, ref settingsEditor);
        settingsEditor.OnInspectorGUI();

        EditorGUILayout.Space();
        float worldW = settings.width * settings.cellSize;
        float worldH = settings.height * settings.cellSize;
        EditorGUILayout.HelpBox($"Maze footprint: {worldW} x {worldH} units ({settings.width * settings.height} cells)", MessageType.None);

        if (settings.width * settings.height > 2500)
            EditorGUILayout.HelpBox("Large maze: generation and NavMesh baking may be slow.", MessageType.Warning);

        if (enemyPrefab == null && settings.enemyCount > 0)
            EditorGUILayout.HelpBox("No Enemy Prefab assigned, so no enemies will be spawned.", MessageType.Warning);

        if (trapPrefab == null && settings.trapCount > 0)
            EditorGUILayout.HelpBox("No Trap Prefab assigned, so no traps will be spawned.", MessageType.Warning);

        randomizeSeed = EditorGUILayout.Toggle("Randomize Seed On Generate", randomizeSeed);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Maze", GUILayout.Height(32))) GenerateMaze();
        if (GUILayout.Button("Clear Maze")) ClearMaze();

        if (!string.IsNullOrEmpty(status))
            EditorGUILayout.HelpBox(status, MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    void GenerateMaze()
    {
        // Group everything into one undo step
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Generate Maze");
        int group = Undo.GetCurrentGroup();

        ClearMaze();

        if (randomizeSeed)
        {
            Undo.RecordObject(settings, "Randomize Seed");
            settings.seed = Random.Range(0, 1000000);
            EditorUtility.SetDirty(settings);
        }

        var grid = MazeGenerator.Generate(settings.width, settings.height, settings.seed);

        // Walls + floor. The baker component bakes the NavMesh when the game starts.
        var root = MazeBuilder.Build(settings, grid, wallPrefab);
        root.AddComponent<MazeNavMeshBaker>();

        // Everything that isn't part of the maze geometry lives under a separate root
        // so it doesn't get baked into the NavMesh.
        var actors = new GameObject(ActorsRootName);
        Vector2Int treasureCell = PlaceStartAndTreasure(grid, actors.transform);
        int enemies = SpawnEnemies(actors.transform);
        int traps = SpawnTraps(actors.transform, treasureCell);

        Undo.RegisterCreatedObjectUndo(root, "Generate Maze");
        Undo.RegisterCreatedObjectUndo(actors, "Generate Maze");
        Undo.CollapseUndoOperations(group);

        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;

        status = $"Generated {settings.width}x{settings.height} maze (seed {settings.seed}). " +
                 $"Treasure at cell {treasureCell}. Enemies: {enemies}. Traps: {traps}.";
    }

    void ClearMaze()
    {
        DestroyAllNamed(MazeRootName);
        DestroyAllNamed(ActorsRootName);
        status = "Maze cleared.";
    }

    static void DestroyAllNamed(string objectName)
    {
        GameObject old;
        while ((old = GameObject.Find(objectName)) != null)
            Undo.DestroyObjectImmediate(old);
    }

    /// <summary>
    /// Player starts in the corner cell; the treasure goes to the cell
    /// that is farthest away by walking distance.
    /// </summary>
    Vector2Int PlaceStartAndTreasure(MazeCell[,] grid, Transform parent)
    {
        Vector2Int startCell = Vector2Int.zero;
        Vector2Int treasureCell = MazeGenerator.FindFarthestCell(grid, startCell);

        var start = new GameObject("PlayerStart");
        start.transform.SetParent(parent);
        start.transform.position = CellToWorld(startCell, 0.1f);

        // Placeholder treasure (we'll swap in a proper model later)
        var treasure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        treasure.name = "Treasure";
        treasure.transform.SetParent(parent);
        treasure.transform.position = CellToWorld(treasureCell, 1f);
        treasure.GetComponent<Collider>().isTrigger = true;
        treasure.AddComponent<TreasureGoal>();

        return treasureCell;
    }

    /// <summary>
    /// Spawns enemyCount enemies at random cells away from the start corner,
    /// each with a few nearby waypoints to patrol. Seeded, so the same seed
    /// always gives the same enemy layout.
    /// </summary>
    int SpawnEnemies(Transform parent)
    {
        if (enemyPrefab == null || settings.enemyCount <= 0) return 0;

        var rng = new System.Random(settings.seed + 1);
        int minDistanceFromStart = Mathf.Min(6, (settings.width + settings.height) / 4);

        int spawned = 0;
        int attempts = 0;
        while (spawned < settings.enemyCount && attempts++ < 500)
        {
            var cell = new Vector2Int(rng.Next(settings.width), rng.Next(settings.height));
            if (cell.x + cell.y < minDistanceFromStart) continue; // too close to the player

            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, parent);
            enemy.name = "Enemy_" + (spawned + 1);
            enemy.transform.position = CellToWorld(cell, 0f);

            var patrol = enemy.GetComponent<EnemyPatrol>();
            if (patrol != null) patrol.waypoints = BuildWaypoints(cell, rng);

            spawned++;
        }

        return spawned;
    }

    /// <summary>
    /// Spawns trapCount traps in unique cells, never on the start area or the treasure.
    /// Seeded like the enemies, so the same seed gives the same trap layout.
    /// </summary>
    int SpawnTraps(Transform parent, Vector2Int treasureCell)
    {
        if (trapPrefab == null || settings.trapCount <= 0) return 0;

        var rng = new System.Random(settings.seed + 2);
        var used = new HashSet<Vector2Int>();
        int minDistanceFromStart = Mathf.Min(4, (settings.width + settings.height) / 4);

        int spawned = 0;
        int attempts = 0;
        while (spawned < settings.trapCount && attempts++ < 1000)
        {
            var cell = new Vector2Int(rng.Next(settings.width), rng.Next(settings.height));
            if (cell.x + cell.y < minDistanceFromStart) continue; // keep the start safe
            if (cell == treasureCell || !used.Add(cell)) continue;

            var trap = (GameObject)PrefabUtility.InstantiatePrefab(trapPrefab, parent);
            trap.name = "Trap_" + (spawned + 1);
            trap.transform.position = CellToWorld(cell, 0f);

            spawned++;
        }

        return spawned;
    }

    /// <summary>Start cell plus 3 random cells within 4 cells of it.</summary>
    Vector3[] BuildWaypoints(Vector2Int origin, System.Random rng)
    {
        var points = new List<Vector3> { CellToWorld(origin, 0f) };

        for (int i = 0; i < 3; i++)
        {
            int x = Mathf.Clamp(origin.x + rng.Next(-4, 5), 0, settings.width - 1);
            int y = Mathf.Clamp(origin.y + rng.Next(-4, 5), 0, settings.height - 1);
            points.Add(CellToWorld(new Vector2Int(x, y), 0f));
        }

        return points.ToArray();
    }

    Vector3 CellToWorld(Vector2Int cell, float height)
    {
        return new Vector3(cell.x * settings.cellSize, height, cell.y * settings.cellSize);
    }
}
