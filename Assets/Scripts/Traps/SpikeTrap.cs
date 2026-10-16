using UnityEngine;

/// <summary>
/// Floor trap that cycles: hidden -> warning (spikes peek out) -> active (spikes up, hurts) -> hidden.
/// The warning phase gives the player a fair chance to step away.
/// Setup: the root needs a trigger collider; 'spikes' is the child that moves up and down.
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform spikes;

    [Header("Spike Heights (local Y)")]
    [SerializeField] float hiddenY = -0.45f;  // fully below the floor
    [SerializeField] float warnPeek = 0.3f;   // how far spikes rise during the warning
    [SerializeField] float raisedY = 0.4f;    // fully extended

    [Header("Timing (seconds)")]
    [SerializeField] float idleTime = 2f;
    [SerializeField] float warnTime = 0.6f;
    [SerializeField] float activeTime = 1f;

    [Header("Damage")]
    [SerializeField] int damage = 1;

    enum State { Idle, Warning, Active }
    State state = State.Idle;
    float timer;

    void Start()
    {
        // Random start offset so all traps don't fire in sync
        timer = Random.Range(0f, idleTime);
        SetHeight(hiddenY);
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                if (timer >= idleTime) ChangeState(State.Warning);
                break;
            case State.Warning:
                if (timer >= warnTime) ChangeState(State.Active);
                break;
            case State.Active:
                if (timer >= activeTime) ChangeState(State.Idle);
                break;
        }

        // Move spikes toward the height for the current state (fast up, slow down)
        float target = state == State.Active ? raisedY
                     : state == State.Warning ? hiddenY + warnPeek
                     : hiddenY;
        float speed = state == State.Active ? 30f : 3f;

        float y = Mathf.MoveTowards(spikes.localPosition.y, target, speed * Time.deltaTime);
        SetHeight(y);
    }

    void ChangeState(State next)
    {
        state = next;
        timer = 0f;

        // Spikes shoot up: sound plus a little dust
        if (next == State.Active)
        {
            Sfx.Play3D(Sfx.Spikes, transform.position, 0.9f);
            JuiceFx.Burst(transform.position + Vector3.up * 0.1f, new Color(0.7f, 0.7f, 0.7f), 12, 3f, 0.5f, 0.1f, 1f);
        }
    }

    void SetHeight(float y)
    {
        if (spikes == null) return;
        Vector3 p = spikes.localPosition;
        p.y = y;
        spikes.localPosition = p;
    }

    // PlayerHealth has its own hit cooldown, so calling this every frame is safe
    void OnTriggerStay(Collider other)
    {
        if (state != State.Active) return;

        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage);
    }
}
