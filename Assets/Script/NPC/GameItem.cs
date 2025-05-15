using UnityEngine;

/// <summary>
/// Represents a game item that can be placed on tables for NPCs
/// </summary>
public class GameItem : MonoBehaviour, IInteractable
{
    [Header("Game Item Settings")]
    [SerializeField] private string gameType; // "Tavla", "Iskambil", or "Okey"
    [SerializeField] private GameObject placedVisualPrefab; // The visual when placed on a table
    
    
    /// <summary>
    /// Get the type of this game
    /// </summary>
    public string GetGameType()
    {
        return gameType;
    }
    
    /// <summary>
    /// Called when player interacts with this game item
    /// </summary>
    public void interact()
    {
        // Get reference to the player
        Player player = FindObjectOfType<Player>();
        
        if (player != null)
        {
            // If player is already holding something, don't pick up
            if (player.inHandItem != null)
            {
                return;
            }
            
            // Check if we're looking at a table with NPCs
            RaycastHit hit;
            if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, 3f))
            {
                TableController table = hit.collider.GetComponent<TableController>();
                
                if (table != null && table.HasGroup())
                {
                    // Check if this is the requested game type
                    string requestedGame = table.GetRequestedGameType();
                    
                    if (requestedGame == gameType)
                    {
                        // Place the game on the table
                        table.PlaceGameBox(gameType);
                        
                        // Destroy this pickup item
                        Destroy(gameObject);
                        return;
                    }
                    else
                    {
                        // Wrong game type
                        player.ShowUIMessage($"Yanlış oyun! Bu masa {requestedGame} istiyor");
                        return;
                    }
                }
                else if (table != null)
                {
                    
                }
            }
            
            // Pick up the game item
            player.inHandItem = gameObject;
            
            // Disable physics
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // Parent to player's hand
            Transform firstPersonHand = player.transform.Find("FirstPersonHand");
            if (firstPersonHand != null)
            {
                transform.SetParent(firstPersonHand, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
    }
    
    /// <summary>
    /// Set the game type for this item
    /// </summary>
    public void SetGameType(string type)
    {
        gameType = type;
    }
} 