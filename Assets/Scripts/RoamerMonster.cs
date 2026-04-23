using UnityEngine;

// Roamer monster — attach this to monsters
//
// Class for extra features off base

public class RoamerMonster : MonsterBase
{
    [Header("Roamer — Optional Extras")]
    [Tooltip("Sound played when the monster spots the player. Leave empty to skip.")]
    public AudioClip aggroSound;

    [Tooltip("Sound played when the monster loses the player. Leave empty to skip.")]
    public AudioClip deaggroSound;

    [Tooltip("Sound played on each attack. Leave empty to skip.")]
    public AudioClip attackSound;

    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake(); // always call base so MonsterBase initialises correctly
        audioSource = GetComponent<AudioSource>(); // needed if sounds are added
    }

    // Called when the monster enters chase state
    protected override void OnAggroed()
    {
        PlaySound(aggroSound);
    }

    // Called once when the monster drops chase state
    protected override void OnDeaggroed()
    {
        PlaySound(deaggroSound);
    }

    // called every frame while monster is alive (even if not chasing)
    protected override void OnMonsterUpdate()
    {
        // Could add things like footsteps, roaming, etc.
    }

    // Called once when this monster dies (only if canDie = true)
    protected override void OnDeath()
    {

        Destroy(gameObject, 3f);
    }

    protected override void OnAttack()
    {
        PlaySound(attackSound);
    }

    // Helpers 

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}