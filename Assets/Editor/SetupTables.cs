using UnityEngine;
using UnityEditor;

public class SetupTables : EditorWindow
{
    [MenuItem("Teaaholic/Setup Tables")]
    public static void SetupAllTables()
    {
        // Find all TableController components in the scene
        TableController[] tables = GameObject.FindObjectsOfType<TableController>();
        int usableLayer = LayerMask.NameToLayer("Usable");
        
        if (usableLayer == -1)
        {
            Debug.LogError("Usable layer doesn't exist! Make sure to create this layer in your project settings.");
            return;
        }
        
        int tableCount = 0;
        
        foreach (TableController table in tables)
        {
            // Set the table's gameObject to the Usable layer
            table.gameObject.layer = usableLayer;
            tableCount++;
            
            // Optionally, you can also set all child objects to the same layer
            Transform[] children = table.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                // Skip the table itself (already set)
                if (child == table.transform)
                    continue;
                    
                child.gameObject.layer = usableLayer;
            }
        }
        
        Debug.Log($"Setup complete! {tableCount} tables were set to the Usable layer.");
    }
    
    [MenuItem("Teaaholic/Check Player Layer Settings")]
    public static void CheckPlayerLayerSettings()
    {
        // Find the player in the scene
        Player player = GameObject.FindObjectOfType<Player>();
        
        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
            return;
        }
        
        // Check if the player's useableLayer includes the Usable layer
        int usableLayer = LayerMask.NameToLayer("Usable");
        
        if (usableLayer == -1)
        {
            Debug.LogError("Usable layer doesn't exist! Make sure to create this layer in your project settings.");
            return;
        }
        
        if ((player.useableLayer.value & (1 << usableLayer)) == 0)
        {
            Debug.LogError("Player's useableLayer doesn't include the Usable layer! Add the Usable layer to the Player's useableLayer mask in the Inspector.");
        }
        else
        {
            Debug.Log("Player's useableLayer correctly includes the Usable layer!");
        }
    }
} 