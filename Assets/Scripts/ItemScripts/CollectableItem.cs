using UnityEngine;


// A collectible item that scores points when returned to base.
// attach this to the objects

public class CollectibleItem : ItemBase
{
    [Header("Collectible Settings")]
    [Tooltip("If true the item is destroyed when returned to base. If false it stays at base.")]
    public bool destroyOnReturn = true;

    [Tooltip("Optional sound to play when picked up.")]
    public AudioClip pickupSound;

    [Tooltip("Optional sound to play when returned to base.")]
    public AudioClip returnSound;

    private AudioSource audioSource;

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
    }

    protected override void OnPlayerEnterRange()
    {
        // Tell the UI to show the pickup prompt
        // this should be handled by the local player only (show only for them)
        ItemPromptUI.Show($"Press E to pick up {itemName}  [{itemWeight}kg  ${itemValue}]");
    }

    protected override void OnPlayerExitRange()
    {
        ItemPromptUI.Hide();
    }

    protected override void OnPickedUp()
    {
        PlaySound(pickupSound);
        ItemPromptUI.Hide();
    }

    protected override void OnPickupFailed()
    {
        ItemPromptUI.Show("Too heavy to carry!");
    }

    protected override void OnDropped()
    {
        ItemPromptUI.Show($"Press E to pick up {itemName}  [{itemWeight}kg  ${itemValue}]");
    }

    public override void OnReturnedToBase()
    {
        PlaySound(returnSound);

        // add to score
        // this should be done by host to sync for multiplayer
        GameScore.Add(itemValue);

        if (destroyOnReturn)
            Destroy(gameObject);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}