using UnityEngine;

/// <summary>
/// Enemy sight: detects the player inside a cone (distance, angle, and a
/// line-of-sight check so walls block vision) and draws that cone on the floor
/// so the player can see and avoid it. Being seen for a moment = Game Over.
/// </summary>
public class EnemyVision : MonoBehaviour
{
    [Header("Sight")]
    [SerializeField] float viewDistance = 8f;
    [SerializeField, Range(10f, 180f)] float viewAngle = 70f; // full cone angle
    [SerializeField] float eyeHeight = 1f;
    [SerializeField] float reactionTime = 0.25f; // seconds in sight before Game Over

    [Header("Cone Visual")]
    [SerializeField] int rayCount = 30; // more rays = smoother cone edge
    [SerializeField] Color calmColor = new Color(1f, 0.9f, 0.2f, 0.35f);
    [SerializeField] Color alertColor = new Color(1f, 0.1f, 0.1f, 0.55f);

    Transform player;
    Material coneMaterial;
    Mesh coneMesh;
    Transform coneTransform;
    Vector3[] vertices;

    float detection; // 0 .. reactionTime

    void Start()
    {
        var playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else Debug.LogWarning("EnemyVision: no object tagged 'Player' found.");

        BuildCone();
    }

    void Update()
    {
        if (player == null) return;
        if (GameManager.Instance != null && (GameManager.Instance.IsGameOver || GameManager.Instance.HasWon)) return;

        // Build up (or cool down) detection so a split-second glimpse is forgiving
        if (CanSeePlayer()) detection += Time.deltaTime;
        else detection = Mathf.Max(0f, detection - Time.deltaTime);

        // Cone shifts from yellow to red as the enemy notices the player
        coneMaterial.color = Color.Lerp(calmColor, alertColor, detection / reactionTime);

        if (detection >= reactionTime && GameManager.Instance != null)
        {
            Sfx.Play2D(Sfx.Alert, 1f);
            GameManager.Instance.GameOver();
        }
    }

    void LateUpdate()
    {
        UpdateConeMesh();
    }

    bool CanSeePlayer()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * 1f; // roughly the player's chest
        Vector3 toTarget = target - eye;

        // 1. Close enough? (checked on the floor plane)
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
        if (flat.magnitude > viewDistance) return false;

        // 2. Inside the cone angle?
        if (Vector3.Angle(transform.forward, flat) > viewAngle * 0.5f) return false;

        // 3. Nothing in the way? The first thing the ray hits must be the player.
        if (Physics.Raycast(eye, toTarget.normalized, out RaycastHit hit, viewDistance,
                            ~0, QueryTriggerInteraction.Ignore))
        {
            return hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player");
        }

        return false;
    }

    // ---------- Cone visual ----------

    void BuildCone()
    {
        var go = new GameObject("VisionCone");
        coneTransform = go.transform;
        coneTransform.SetParent(transform, false);
        coneTransform.localPosition = new Vector3(0f, 0.05f, 0f); // just above the floor

        // Sprites/Default is transparent and double-sided, so no material setup is needed
        coneMaterial = new Material(Shader.Find("Sprites/Default")) { color = calmColor };

        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = coneMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        coneMesh = new Mesh { name = "VisionConeMesh" };
        coneMesh.MarkDynamic();
        go.AddComponent<MeshFilter>().sharedMesh = coneMesh;

        // Triangle fan: vertex 0 is the enemy, the rest form the cone's outer edge
        vertices = new Vector3[rayCount + 2];
        var triangles = new int[rayCount * 3];
        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
        coneMesh.vertices = vertices;
        coneMesh.triangles = triangles;
    }

    void UpdateConeMesh()
    {
        if (coneMesh == null) return;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        float startAngle = -viewAngle * 0.5f;
        float step = viewAngle / rayCount;

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= rayCount; i++)
        {
            // Cast a ray for each slice; walls shorten the cone so it matches what the enemy sees
            Vector3 dir = Quaternion.Euler(0f, startAngle + step * i, 0f) * transform.forward;
            dir.y = 0f;

            float distance = viewDistance;
            if (Physics.Raycast(eye, dir, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore))
                distance = hit.distance;

            Vector3 worldPoint = coneTransform.position + dir * distance;
            vertices[i + 1] = coneTransform.InverseTransformPoint(worldPoint);
        }

        coneMesh.vertices = vertices;
        coneMesh.RecalculateBounds();
    }

    void OnDestroy()
    {
        // The material and mesh are created at runtime, so clean them up
        if (coneMaterial != null) Destroy(coneMaterial);
        if (coneMesh != null) Destroy(coneMesh);
    }
}
