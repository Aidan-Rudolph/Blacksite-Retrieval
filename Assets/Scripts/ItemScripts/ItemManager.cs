using UnityEngine;
using System.Collections.Generic;


// Made to manage item and closest item prompts and pickups for the player.
public class ItemManager : MonoBehaviour
{
    private List<ItemBase> itemsInRange = new List<ItemBase>();
    private ItemBase closestItem;

    private void Update()
    {
        UpdateClosestItem();

        if (closestItem != null && Input.GetKeyDown(KeyCode.E))
        {
            closestItem.TryPickupFromManager();
        }
    }

    private void UpdateClosestItem()
    {
        ItemBase newClosest = null;
        float closestDist = float.MaxValue;

        foreach (ItemBase item in itemsInRange)
        {
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                newClosest = item;
            }
        }

        if (newClosest != closestItem)
        {
            if (closestItem != null) closestItem.HidePrompt();
            if (newClosest != null) newClosest.ShowPrompt();
            closestItem = newClosest;
        }
    }

    public void RegisterItem(ItemBase item)
    {
        if (!itemsInRange.Contains(item))
            itemsInRange.Add(item);
    }

    public void UnregisterItem(ItemBase item)
    {
        itemsInRange.Remove(item);
        if (closestItem == item)
        {
            closestItem = null;
            ItemPromptUI.Hide();
        }
    }
}