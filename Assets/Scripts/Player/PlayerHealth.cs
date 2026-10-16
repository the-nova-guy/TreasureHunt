using System;
using UnityEngine;

/// <summary>
/// Tracks player health. Traps call TakeDamage(); the UI and game manager
/// listen to the events instead of the player knowing about them.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] float invulnerableTime = 1f; // grace period after a hit

    // Serialized so the live value is visible in the Inspector while testing
    [SerializeField] int current;

    public int Current => current;
    public int Max => maxHealth;
    public bool IsDead { get; private set; }

    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDamaged;
    public event Action OnDied;

    float lastHitTime = -99f;

    void Awake()
    {
        current = maxHealth;
    }

    void Start()
    {
        // Let listeners (HUD) show the starting value
        OnHealthChanged?.Invoke(current, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || Time.time < lastHitTime + invulnerableTime) return;

        lastHitTime = Time.time;
        current = Mathf.Max(0, current - amount);
        Debug.Log($"Player health: {current}/{maxHealth}");

        OnDamaged?.Invoke();
        OnHealthChanged?.Invoke(current, maxHealth);

        if (current == 0)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }

    // Handy for testing before traps exist: right-click the component header
    [ContextMenu("Take 1 Damage")]
    void DebugDamage()
    {
        TakeDamage(1);
    }
}
