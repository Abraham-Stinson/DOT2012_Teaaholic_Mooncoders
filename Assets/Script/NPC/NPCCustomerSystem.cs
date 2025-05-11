using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCCustomerSystem : MonoBehaviour
{
    [System.Serializable]
    public class DrinkOption
    {
        public string drinkName;
        public float price;
        
        // Optional description for the menu
        [Tooltip("Optional description of the drink")]
        public string description;
    }

    [Header("NPC Prefabs")]
    public GameObject[] npcPrefabs; // Different looking NPC prefabs

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public Transform exitPoint;  // New exit point for despawning
    public float spawnInterval = 60.0f; // How often new customer groups spawn (seconds)
    
    [Header("Venue Settings")]
    public Transform doorPoint;
    public Transform cashierPoint;
    public List<TableController> tables = new List<TableController>();
    
    [Header("Game Settings")]
    [Tooltip("Available drinks that NPCs can order")]
    public DrinkOption[] drinkMenu;
    
    [Header("Prefabs")]
    // Carryable game prefabs (held by player)
    public GameObject backgammonPrefab;
    public GameObject cardGamePrefab;
    public GameObject okeyPrefab;
    
    // Active game prefabs (on table when customers play)
    public GameObject backgammonPlayingPrefab;
    public GameObject cardGamePlayingPrefab;
    public GameObject okeyPlayingPrefab;
    
    [Header("Patience Settings")]
    public float defaultGamePatience = 60f;  // Default patience for waiting for game (seconds)
    public float defaultDrinkPatience = 90f; // Default patience for waiting for drinks (seconds)
    public float defaultPaymentPatience = 45f; // Default patience for payment
    
    [Header("Customer Behavior Settings")]
    [Range(0f, 1f)] public float reorderChance = 0.3f;
    public float minStayTime = 150f; // 2.5 minutes
    public float maxStayTime = 400f; // ~6.5 minutes

    private List<CustomerGroup> activeCustomerGroups = new List<CustomerGroup>();
    private bool gameRunning = true;
    
    void Start()
    {
        // Find all tables at start
        TableController[] foundTables = FindObjectsOfType<TableController>();
        tables.AddRange(foundTables);
        
        // Start spawning customers at regular intervals
        StartCoroutine(SpawnCustomerGroups());
    }
    
    IEnumerator SpawnCustomerGroups()
    {
        while (gameRunning)
        {
            // Wait for spawn interval
            yield return new WaitForSeconds(spawnInterval);
            
            // Create a random sized customer group (2 or 4 people)
            int groupSize = Random.value < 0.5f ? 2 : 4;
            
            // Create new customer group
            CreateNewCustomerGroup(groupSize);
        }
    }
    
    void CreateNewCustomerGroup(int groupSize)
    {
        CustomerGroup newGroup = new CustomerGroup();
        newGroup.groupSize = groupSize;
        newGroup.members = new List<CustomerNPC>();
        
        // Set initial patience values
        newGroup.initialGameOrderPatience = defaultGamePatience;
        newGroup.initialDrinkOrderPatience = defaultDrinkPatience;
        newGroup.initialPaymentPatience = defaultPaymentPatience;
        
        // Create NPCs for this group
        for (int i = 0; i < groupSize; i++)
        {
            // Select a random NPC prefab
            int randomPrefabIndex = Random.Range(0, npcPrefabs.Length);
            GameObject npcPrefab = npcPrefabs[randomPrefabIndex];
            
            // Instantiate NPC at spawn point
            GameObject newNPC = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
            
            // Add and initialize NPC components
            CustomerNPC customerNPC = newNPC.AddComponent<CustomerNPC>();
            customerNPC.SetGroup(newGroup);
            customerNPC.SetSystemReference(this);
            
            // Add to group members
            newGroup.members.Add(customerNPC);
        }
        
        // Add to active groups list
        activeCustomerGroups.Add(newGroup);
        
        // Set first NPC as leader
        newGroup.leader = newGroup.members[0];
        
        // Tell leader to head to door
        newGroup.leader.GoToDoor();
    }
    
    public TableController FindEmptyTable(int groupSize)
    {
        // Check all tables and find an empty one
        foreach (TableController table in tables)
        {
            if (table.IsEmpty() && table.ChairCount() >= groupSize)
            {
                return table;
            }
        }
        
        // Return null if no empty table found
        return null;
    }
    
    // Called by player when delivering a game to a table
    public void DeliverGameToTable(TableController table, string gameType)
    {
        foreach (CustomerGroup group in activeCustomerGroups)
        {
            if (group.assignedTable == table && group.currentState == CustomerGroup.GroupState.WaitingForGame)
            {
                // Make sure it's the game they ordered
                if (group.requestedGame == gameType)
                {
                    // Find the leader and deliver the game
                    group.leader.ReceiveGame();
                    break;
                }
            }
        }
    }
    
    // Called by player when delivering a drink to an NPC
    public void DeliverDrinkToNPC(CustomerNPC customer, string drinkType)
    {
        if (customer != null && customer.myGroup != null && 
            customer.myGroup.currentState == CustomerGroup.GroupState.WaitingForDrinks)
        {
            customer.ReceiveDrink(drinkType);
        }
    }
    
    // Get GameObject for the requested game
    public GameObject GetGamePrefab(string gameName)
    {
        switch (gameName)
        {
            case "Backgammon":
                return backgammonPlayingPrefab;
            case "Cards":
                return cardGamePlayingPrefab;
            case "Okey":
                return okeyPlayingPrefab;
            default:
                Debug.LogWarning("Unknown game requested: " + gameName);
                return null;
        }
    }
    
    // Get all active drink orders for a specific customer group
    public Dictionary<CustomerNPC, string> GetGroupDrinkOrders(CustomerGroup group)
    {
        return group.drinkOrders;
    }
    
    public void RemoveGroup(CustomerGroup group)
    {
        // Remove group from active groups list
        activeCustomerGroups.Remove(group);
        
        // Destroy all group members
        foreach (CustomerNPC member in group.members)
        {
            Destroy(member.gameObject);
        }
    }
}