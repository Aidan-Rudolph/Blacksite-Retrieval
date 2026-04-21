using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Minimal player health script.
/// The monster calls TakeDamage() on this when it lands a hit.
///
/// MULTIPLAYER: TakeDamage should become a Photon RPC so the server/host
/// is the authority on HP, and death is synced to all clients.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Events")]
    [Tooltip("Fired every time the player takes damage. Passes remaining HP as float.")]
    public UnityEvent<float> onDamaged;

    [Tooltip("Fired once when the player dies.")]
    public UnityEvent onDied;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    /// <summary>Called by monsters (and anything else) to deal damage.</summary>
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0f);

        onDamaged?.Invoke(CurrentHealth);

        Debug.Log($"[PlayerHealth] Took {amount} damage. HP: {CurrentHealth}/{maxHealth}");

        if (CurrentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        onDied?.Invoke();
        Debug.Log("[PlayerHealth] Player died.");

        // MULTIPLAYER: sync death to other clients here
        // For solo testing you could load a game-over scene here, e.g.:
        // SceneManager.LoadScene("GameOver");
    }
}