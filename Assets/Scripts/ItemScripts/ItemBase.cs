using UnityEngine;

// Base class for all items in the game.
// Handles proximity detection, E to pickup prompt, weight, and value.

public abstract class ItemBase : MonoBehaviour
{
    [Header("Item Info")]
    [Tooltip("Name shown in the pickup prompt.")]
    public string itemName = "Item";

    [Tooltip("Value of this item when returned to base.")]
    public float itemValue = 10f;

    [Tooltip("How much this item contributes to carry weight.")]
    public float itemWeight = 1f;

    [Header("Pickup Settings")]
    [Tooltip("How close the player needs to be to see the pickup prompt.")]
    public float pickupRange = 2.5f;

    // States

    public bool IsPickedUp { get; private set; }
    public bool IsInRange { get; private set; }

    // References 

    protected Transform playerTransform;
    private Collider itemCollider;
    private Rigidbody itemRigidbody;


    protected virtual void Awake()
    {
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();

        // replace with multiplayer player reference
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
        else
            Debug.LogWarning($"[{itemName}] No GameObject tagged 'Player' found.");
    }

    protected virtual void Update()
    {
        if (IsPickedUp) return;
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = IsInRange;
        IsInRange = dist <= pickupRange;

        // event triggers for entering/exiting range
        if (IsInRange && !wasInRange) OnPlayerEnterRange();
        if (!IsInRange && wasInRange) OnPlayerExitRange();

        // Check for pickup input while in range
        // multiplayer make sure only the local player can trigger this
        if (IsInRange && Input.GetKeyDown(KeyCode.E))
        {
            TryPickup();
        }
    }

    // Pickup / Drop 

    private void TryPickup()
    {
        // Get the player inventory and check if they can carry this
        PlayerInventory inventory = playerTransform.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning($"[{itemName}] Player has no PlayerInventory component.");
            return;
        }

        if (!inventory.CanPickup(this))
        {
            OnPickupFailed();
            return;
        }

        inventory.AddItem(this);
        Pickup(playerTransform);
    }

    // Called when the item is successfully picked up

    public virtual void Pickup(Transform carrier)
    {
        IsPickedUp = true;

        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = true;
            itemRigidbody.velocity = Vector3.zero;
        }
        if (itemCollider != null) itemCollider.enabled = false;

        // sync with multiplayer so it disappears for other players too
        gameObject.SetActive(false); // just hide it
        OnPickedUp();
    }

    // Detaches the item from the carrier and drops it in the world.
    public virtual void Drop(Vector3 dropPosition)
    {
        IsPickedUp = false;

        // sync with multiplayer so it reappears for other players too
        gameObject.SetActive(true); // reappear
        transform.position = dropPosition;

        if (itemCollider != null) itemCollider.enabled = true;
        if (itemRigidbody != null) itemRigidbody.isKinematic = false;

        OnDropped();
    }

    //Functions to override for item-specific behaviour in other scripts

    // Called every frame while picked up. Override for carried behaviour e.g. flashlight.
    protected virtual void OnCarryUpdate() { }

    // Called once when the player enters pickup range.
    protected virtual void OnPlayerEnterRange() { }

    // Called once when the player leaves pickup range.
    protected virtual void OnPlayerExitRange() { }

    // Called once when successfully picked up.
    protected virtual void OnPickedUp() { }

    // Called once when dropped.
    protected virtual void OnDropped() { }

    // Called when pickup fails e.g. inventory full.
    protected virtual void OnPickupFailed() { }

    // Called by BaseZone when this item is returned to base.
    public virtual void OnReturnedToBase() { }



    // visualizers for range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}