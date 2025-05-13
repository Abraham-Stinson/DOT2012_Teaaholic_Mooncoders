using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [Header("NPC Spawning")]
    [SerializeField] private GameObject[] npcPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float minSpawnTime = 30f;
    [SerializeField] private float maxSpawnTime = 120f;
    [SerializeField] private int minGroupSize = 2;
    [SerializeField] private int maxGroupSize = 4;
    [SerializeField] private float groupSpacing = 1f;
    
    [Header("Locations")]
    [SerializeField] private Transform entranceArea;
    [SerializeField] private Transform exitArea;
    [SerializeField] private Transform cashierArea;
    
    [Header("Tables and Chairs")]
    [SerializeField] private Table[] tables;
    
    [Header("Game Items")]
    [SerializeField] private GameObject backgammonPrefab;
    [SerializeField] private GameObject cardsPrefab;
    [SerializeField] private GameObject okeyPrefab;
    
    [Header("Drinks")]
    [SerializeField] private string[] availableDrinks;
    
    [Header("Patience Settings")]
    [SerializeField] private float initialPatience = 120f;
    [SerializeField] private float patienceAfterServing = 60f;
    [SerializeField] private float cashierPatience = 60f;
    
    [Header("Behavior Settings")]
    [SerializeField] private float drinkConsumptionTime = 120f;
    [SerializeField] private float minDrinkConsumptionTime = 60f;
    [SerializeField] private float maxDrinkConsumptionTime = 180f;
    [SerializeField] private float reorderChance = 0.5f;
    
    private List<NPCGroup> activeGroups = new List<NPCGroup>();
    
    void Start()
    {
        ValidateReferences();
        StartCoroutine(SpawnNPCGroups());
    }
    
    void ValidateReferences()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogError("No NPC prefabs assigned in NPCManager!");
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("No spawn point assigned in NPCManager!");
        }
        
        if (entranceArea == null)
        {
            Debug.LogError("No entrance area assigned in NPCManager!");
        }
        
        if (exitArea == null)
        {
            Debug.LogError("No exit area assigned in NPCManager!");
        }
        
        if (cashierArea == null)
        {
            Debug.LogError("No cashier area assigned in NPCManager!");
        }
        
        if (tables == null || tables.Length == 0)
        {
            Debug.LogError("No tables assigned in NPCManager!");
        }
        
        if (availableDrinks == null || availableDrinks.Length == 0)
        {
            Debug.LogError("No available drinks assigned in NPCManager!");
        }
    }
    
    IEnumerator SpawnNPCGroups()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);
            
            SpawnGroup();
        }
    }
    
    void SpawnGroup()
    {
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogError("Cannot spawn group: No NPC prefabs available!");
            return;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("Cannot spawn group: No spawn point assigned!");
            return;
        }
        
        // Determine group size: either 2 or 4 people
        int groupSize = Random.Range(0, 2) == 0 ? 2 : 4;
        
        // Create group and leader
        NPCGroup newGroup = new NPCGroup();
        newGroup.size = groupSize;
        
        // Find a table with enough seats
        Table targetTable = FindAvailableTable(groupSize);
        newGroup.assignedTable = targetTable;
        
        if (targetTable != null)
        {
            targetTable.ReserveTable(groupSize);
            
            // Get all available chairs at once
            Chair[] groupChairs = targetTable.GetAvailableChairs(groupSize);
            
            if (groupChairs == null || groupChairs.Length != groupSize)
            {
                Debug.LogError("Not enough chairs available for group size: " + groupSize);
                targetTable.ReleaseTable();
                return;
            }
            
            // Spawn all NPCs in the group
            for (int i = 0; i < groupSize; i++)
            {
                GameObject npcPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];
                Vector3 spawnPos = spawnPoint.position + new Vector3(i * groupSpacing, 0, 0);
                
                GameObject npcObj = Instantiate(npcPrefab, spawnPos, Quaternion.identity);
                NPC npcScript = npcObj.GetComponent<NPC>();
                
                if (npcScript == null)
                {
                    npcScript = npcObj.AddComponent<NPC>();
                }
                
                // Set up NPC with assigned chair
                npcScript.Initialize(this, newGroup, i == 0, targetTable, groupChairs[i]);
                
                // Add to group
                newGroup.members.Add(npcScript);
            }
            
            // Assign game type based on group size
            if (groupSize == 2)
            {
                // 2-person group gets either backgammon or cards
                newGroup.requestedGameType = Random.Range(0, 2) == 0 ? "Backgammon" : "Cards";
            }
            else
            {
                // 4-person group gets okey
                newGroup.requestedGameType = "Okey";
            }
            
            activeGroups.Add(newGroup);
        }
        else
        {
            Debug.Log("No available table found for group size: " + groupSize);
        }
    }
    
    public Table FindAvailableTable(int requiredSeats)
    {
        if (tables == null || tables.Length == 0)
        {
            Debug.LogError("No tables available to find!");
            return null;
        }
        
        foreach (Table table in tables)
        {
            if (table != null && table.AvailableSeats >= requiredSeats && !table.IsReserved)
            {
                return table;
            }
        }
        return null;
    }
    
    public Vector3 GetRandomPositionInArea(Transform area)
    {
        if (area == null)
        {
            Debug.LogError("Area transform is null!");
            return Vector3.zero;
        }
        
        // Assuming area has a collider to define its bounds
        Collider areaCollider = area.GetComponent<Collider>();
        if (areaCollider == null)
        {
            Debug.LogError("Area has no collider!");
            return area.position;
        }
        
        // Get a random point within the collider bounds
        Bounds bounds = areaCollider.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            area.position.y, // Keep the same Y position
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
    
    public Transform GetEntranceArea()
    {
        return entranceArea;
    }
    
    public Transform GetExitArea()
    {
        return exitArea;
    }
    
    public Transform GetCashierArea()
    {
        return cashierArea;
    }
    
    public string GetRandomDrink()
    {
        if (availableDrinks == null || availableDrinks.Length == 0)
        {
            Debug.LogError("No available drinks!");
            return "Tea"; // Default drink
        }
        
        return availableDrinks[Random.Range(0, availableDrinks.Length)];
    }
    
    public float GetInitialPatience()
    {
        return initialPatience;
    }
    
    public float GetPatienceAfterServing()
    {
        return patienceAfterServing;
    }
    
    public float GetCashierPatience()
    {
        return cashierPatience;
    }
    
    public float GetDrinkConsumptionTime()
    {
        return Random.Range(minDrinkConsumptionTime, maxDrinkConsumptionTime);
    }
    
    public bool ShouldReorder()
    {
        return Random.value < reorderChance;
    }
    
    public void GroupLeaving(NPCGroup group)
    {
        if (group == null) return;
        
        if (group.assignedTable != null)
        {
            group.assignedTable.ReleaseTable();
        }
        
        activeGroups.Remove(group);
    }
}

// Class to hold information about NPC groups
public class NPCGroup
{
    public int size;
    public List<NPC> members = new List<NPC>();
    public Table assignedTable;
    public string requestedGameType;
    public bool hasReceivedGame = false;
    public int servedMembers = 0;
} 