using UnityEngine;
using UnityEditor;

public class SetupNPCs : EditorWindow
{
    [MenuItem("Teaaholic/Setup NPCs")]
    public static void SetupAllNPCs()
    {
        // Find all NPC components in the scene
        NPC[] npcs = GameObject.FindObjectsOfType<NPC>();
        int npcLayer = LayerMask.NameToLayer("NPC");
        
        if (npcLayer == -1)
        {
            Debug.LogError("NPC layer doesn't exist! Make sure to create this layer in your project settings.");
            return;
        }
        
        int npcCount = 0;
        
        foreach (NPC npc in npcs)
        {
            // Set the NPC's gameObject to the NPC layer
            npc.gameObject.layer = npcLayer;
            
            // Ensure NPC has a collider
            Collider col = npc.GetComponent<Collider>();
            if (col == null)
            {
                // Add a capsule collider if needed
                CapsuleCollider capsule = npc.gameObject.AddComponent<CapsuleCollider>();
                capsule.height = 1.8f;
                capsule.radius = 0.3f;
                capsule.center = new Vector3(0, 0.9f, 0);
                Debug.Log($"Added CapsuleCollider to NPC {npc.name}");
            }
            
            npcCount++;
        }
        
        Debug.Log($"Setup complete! {npcCount} NPCs were set to the NPC layer and have colliders.");
    }
    
    [MenuItem("Teaaholic/Check Player NPC Layer Settings")]
    public static void CheckPlayerNPCLayerSettings()
    {
        // Find the player in the scene
        Player player = GameObject.FindObjectOfType<Player>();
        
        if (player == null)
        {
            Debug.LogError("Player not found in the scene!");
            return;
        }
        
        // Check if the player's npcLayer includes the NPC layer
        int npcLayer = LayerMask.NameToLayer("NPC");
        
        if (npcLayer == -1)
        {
            Debug.LogError("NPC layer doesn't exist! Make sure to create this layer in your project settings.");
            return;
        }
        
        if ((player.npcLayer.value & (1 << npcLayer)) == 0)
        {
            Debug.LogError("Player's npcLayer doesn't include the NPC layer! Add the NPC layer to the Player's npcLayer mask in the Inspector.");
        }
        else
        {
            Debug.Log("Player's npcLayer correctly includes the NPC layer!");
        }
    }
} 