using UnityEngine;

/// <summary>
/// The hidden treasure. Spins and bobs so it's easy to spot, and wins the game
/// when the player touches it. Needs a trigger collider on the same object.
/// </summary>
public class TreasureGoal : MonoBehaviour
{
    [SerializeField] float spinSpeed = 90f;   // degrees per second
    [SerializeField] float bobHeight = 0.2f;
    [SerializeField] float bobSpeed = 2f;

    Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
        transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerHealth>() == null) return;

        JuiceFx.Burst(transform.position, new Color(1f, 0.82f, 0.25f), 80, 7f, 1.4f, 0.2f, 0.6f);

        if (GameManager.Instance != null) GameManager.Instance.Win();
        gameObject.SetActive(false); // collected
    }
}
