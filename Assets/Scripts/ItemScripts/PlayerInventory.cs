using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// Attach this to the Player GameObject.
// Tracks all carried items, total weight, and handles dropping.
//
// MULTIPLAYER: When Photon is ready:
//   - Only run this on the local player: wrap logic in photonView.IsMine checks
//   - Sync inventory changes via RPCs

public class PlayerInventory : MonoBehaviour
{
    [Header("Carry Settings")]
    [Tooltip("Maximum total weight the player can carry.")]
    public float maxCarryWeight = 10f;

    [Tooltip("Press this key to drop all held items.")]
    public KeyCode dropKey = KeyCode.G;

    [Header("Events")]
    public UnityEvent<float> onWeightChanged;  
    public UnityEvent<int> onItemCountChanged;

    // running states

    public float CurrentWeight { get; private set; }
    public int ItemCount => carriedItems.Count;
    public float TotalItemValue { get; private set; }

    private List<ItemBase> carriedItems = new List<ItemBase>();




    private void Update()
    {
        // make sure only the local player can trigger dropping items in multiplayer
        if (Input.GetKeyDown(dropKey)) DropAll();
    }

    // Returns true if the player can carry this item without exceeding max weight.
    public bool CanPickup(ItemBase item)
    {
        return CurrentWeight + item.itemWeight <= maxCarryWeight;
    }

    public void AddItem(ItemBase item)
    {
        if (carriedItems.Contains(item)) return;

        carriedItems.Add(item);
        CurrentWeight += item.itemWeight;
        TotalItemValue += item.itemValue;

        onWeightChanged?.Invoke(CurrentWeight);
        onItemCountChanged?.Invoke(ItemCount);
    }

    public void RemoveItem(ItemBase item)
    {
        if (!carriedItems.Contains(item)) return;

        carriedItems.Remove(item);
        CurrentWeight -= item.itemWeight;
        TotalItemValue -= item.itemValue;

        onWeightChanged?.Invoke(CurrentWeight);
        onItemCountChanged?.Invoke(ItemCount);
    }

    // Returns all carried items — used by BaseZone to collect them.
    public List<ItemBase> GetAllItems()
    {
        return new List<ItemBase>(carriedItems); // return a copy so the list isn't modified mid-loop
    }

    public void DropAll()
    {
        // Drop slightly in front of the player
        Vector3 dropPos = transform.position + transform.forward * 1f;

        foreach (ItemBase item in GetAllItems())
        {
            RemoveItem(item);
            // sync drop in multiplayer
            item.Drop(dropPos);
        }
    }

    public void DropItem(ItemBase item)
    {
        if (!carriedItems.Contains(item)) return;
        Vector3 dropPos = transform.position + transform.forward * 1f;
        RemoveItem(item);
        // sync drop pos in multiplayer
        item.Drop(dropPos);
    }
}