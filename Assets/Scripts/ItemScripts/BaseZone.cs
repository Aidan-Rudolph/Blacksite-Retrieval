using UnityEngine;


// Place this on your base object with a trigger collider.
// When the player walks in carrying items they are collected and scored.
// Flashlights are automatically recharged.

// adding score should happen on host for multiplayer

[RequireComponent(typeof(Collider))]
public class BaseZone : MonoBehaviour
{
    private void Awake()
    {
        // Make sure the collider is always a trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // on host should check if the player entered and has items to return to avoid duplicates
        if (!other.CompareTag("Player")) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        CollectItems(inventory);
    }

    private void CollectItems(PlayerInventory inventory)
    {
        var items = inventory.GetAllItems();
        if (items.Count == 0) return;

        foreach (ItemBase item in items)
        {
            inventory.RemoveItem(item);
            item.OnReturnedToBase();
        }

        Debug.Log($"[BaseZone] Collected {items.Count} items. Total score: {GameScore.CurrentScore}");
    }
}