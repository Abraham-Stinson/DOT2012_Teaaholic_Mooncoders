using UnityEngine;
using System.Collections.Generic;

public class DeliveryBox : MonoBehaviour, IInteractable
{
    private Dictionary<string, int> orderedItems = new Dictionary<string, int>();
    private Dictionary<string, MarketItem> itemDatabase = new Dictionary<string, MarketItem>();
    private bool hasItems = false;
    [SerializeField] private LayerMask placementLayer;

    public void Initialize(Dictionary<MarketItem, int> items, Dictionary<string, MarketItem> database)
    {
        orderedItems.Clear();
        itemDatabase = database;
        hasItems = false;

        foreach (var kvp in items)
        {
            if (kvp.Value > 0)
            {
                hasItems = true;
                orderedItems[kvp.Key.itemName] = kvp.Value;
                Debug.Log($"Added to delivery box: {kvp.Key.itemName} x{kvp.Value}");
            }
        }
        
        if (!hasItems)
        {
            Debug.Log("DeliveryBox: No items added to order");
        }
        else 
        {
            Debug.Log($"DeliveryBox initialized with {orderedItems.Count} different items");
        }
    }

    public void interact()
    {
        if (!hasItems)
        {
            Debug.Log("Item yok");
            return;
        }

        // Check for placement layer under the box
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
        {
            // LayerMask ile doğru karşılaştırma
            if (((1 << hit.collider.gameObject.layer) & placementLayer) == 0)
            {
                Debug.Log($"Kargoyu masaya koyun! Hit layer: {hit.collider.gameObject.layer}, Expected layer: {placementLayer}");
                return;
            }
        }
        else
        {
            Debug.Log("Kargoyu masaya koyun!");
            return;
        }

        // Spawn all ordered items
        foreach (var item in orderedItems)
        {
            if (itemDatabase.ContainsKey(item.Key))
            {
                for (int i = 0; i < item.Value; i++)
                {
                    Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
                    Instantiate(itemDatabase[item.Key].itemPrefab, spawnPosition, Quaternion.identity);
                }
            }
        }

        // Clear the box
        hasItems = false;
        orderedItems.Clear();
        
        // Destroy the delivery box after items are spawned
        Destroy(gameObject);
    }

    public void SetContents(Dictionary<MarketItem, int> itemQuantities)
    {
        orderedItems.Clear();
        itemDatabase.Clear();
        hasItems = false;

        foreach (var kvp in itemQuantities)
        {
            if (kvp.Value > 0)
            {
                hasItems = true;
                orderedItems[kvp.Key.itemName] = kvp.Value;
                itemDatabase[kvp.Key.itemName] = kvp.Key;
                Debug.Log($"Added to delivery box: {kvp.Key.itemName} x{kvp.Value}");
            }
        }
        
        if (!hasItems)
        {
            Debug.Log("DeliveryBox: No items added to order");
        }
        else 
        {
            Debug.Log($"DeliveryBox initialized with {orderedItems.Count} different items");
        }
    }

}
