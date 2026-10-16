using UnityEngine;

/// <summary>
/// All the "game feel" for the player in one place: camera shake, hit particles
/// and sounds when hurt, plus the game over and win sounds.
/// Add to PlayerArmature, next to PlayerHealth.
/// </summary>
public class PlayerFeedback : MonoBehaviour
{
    [Header("Camera Shake")]
    [SerializeField] float hitShake = 0.6f;       // 0..1 strength added per hit
    [SerializeField] float maxOffset = 0.35f;     // camera target movement at full shake
    [SerializeField] float shakeSpeed = 25f;
    [SerializeField] float shakeDecay = 1.8f;     // how fast the shake dies down

    PlayerHealth health;
    Transform cameraTarget;
    Vector3 baseLocalPosition;
    float trauma; // current shake amount, 0..1

    void Start()
    {
        // The Cinemachine camera follows this child; shaking it shakes the view
        cameraTarget = transform.Find("PlayerCameraRoot");
        if (cameraTarget != null) baseLocalPosition = cameraTarget.localPosition;
        else Debug.LogWarning("PlayerFeedback: no child named 'PlayerCameraRoot', camera shake disabled.");

        health = GetComponent<PlayerHealth>();
        if (health != null) health.OnDamaged += OnDamaged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += OnGameOver;
            GameManager.Instance.OnWin += OnWin;
        }
    }

    void OnDestroy()
    {
        if (health != null) health.OnDamaged -= OnDamaged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= OnGameOver;
            GameManager.Instance.OnWin -= OnWin;
        }
    }

    void LateUpdate()
    {
        if (cameraTarget == null) return;

        if (trauma <= 0f)
        {
            cameraTarget.localPosition = baseLocalPosition;
            return;
        }

        // Squared so small shakes are subtle and big ones are violent
        float shake = trauma * trauma;
        float t = Time.unscaledTime * shakeSpeed;
        Vector3 offset = new Vector3(Noise(t, 0f), Noise(t, 10f), Noise(t, 20f)) * (maxOffset * shake);

        cameraTarget.localPosition = baseLocalPosition + offset;
        trauma = Mathf.Max(0f, trauma - shakeDecay * Time.unscaledDeltaTime); // unscaled: works in slow motion
    }

    void OnDamaged()
    {
        AddShake(hitShake);
        Sfx.Play2D(Sfx.Hurt, 0.9f);
        JuiceFx.Burst(transform.position + Vector3.up, new Color(0.9f, 0.1f, 0.15f), 25, 4f, 0.7f, 0.12f, 1.5f);
    }

    void OnGameOver()
    {
        AddShake(1f);
        Sfx.Play2D(Sfx.GameOver, 0.9f);
    }

    void OnWin()
    {
        Sfx.Play2D(Sfx.Win, 0.9f);
    }

    public void AddShake(float amount)
    {
        trauma = Mathf.Clamp01(trauma + amount);
    }

    static float Noise(float x, float seed)
    {
        return Mathf.PerlinNoise(x, seed) * 2f - 1f;
    }
}
