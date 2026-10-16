using Unity.AI.Navigation;
using UnityEngine;

/// <summary>
/// Sits on the generated "Maze" object. Bakes the NavMesh when the game starts,
/// so enemies always walk on the current maze layout.
/// Runs before everything else (-100) so the NavMesh exists when enemies start.
/// </summary>
[RequireComponent(typeof(NavMeshSurface))]
[DefaultExecutionOrder(-100)]
public class MazeNavMeshBaker : MonoBehaviour
{
    void Awake()
    {
        var surface = GetComponent<NavMeshSurface>();

        // Only bake the maze itself, not the player or enemies
        surface.collectObjects = CollectObjects.Children;
        surface.BuildNavMesh();
    }
}
