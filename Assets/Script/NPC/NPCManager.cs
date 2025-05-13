using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the spawning and management of NPC groups in the tea shop.
/// </summary>
public class NPCManager : MonoBehaviour
{
    [Header("NPC Prefabs")]
    [SerializeField] private List<GameObject> npcPrefabs = new List<GameObject>();
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float minSpawnDelay = 60f;
    [SerializeField] private float maxSpawnDelay = 180f;
    [SerializeField] private int minGroupSize = 2;
    [SerializeField] private int maxGroupSize = 4;
    [SerializeField] private float spawnAreaRadius = 2f;
    [SerializeField] private int maxGroupsInShop = 3;
    
    [Header("Entry Settings")]
    [SerializeField] private Transform entryArea;
    [SerializeField] private GameObject doorObject;
    [SerializeField] private string doorAnimationName = "DoorOpen";
    
    [Header("Exit Settings")]
    [SerializeField] private Transform cashierPosition;
    [SerializeField] private Transform exitPosition;
    
    [Header("Tables")]
    [SerializeField] private List<TableController> availableTables = new List<TableController>();
    
    // Internal state tracking
    private List<NPCGroup> activeGroups = new List<NPCGroup>();
    private Coroutine spawnRoutine;
    private bool isShopOpen = true;
    
    private void Start()
    {
        // Start spawning NPCs
        spawnRoutine = StartCoroutine(SpawnGroupsRoutine());
    }
    
    /// <summary>
    /// Coroutine that handles spawning NPC groups at regular intervals
    /// </summary>
    private IEnumerator SpawnGroupsRoutine()
    {
        while (isShopOpen)
        {
            // Wait for the next spawn time
            float spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(spawnDelay);
            
            // Check if we can spawn more groups
            if (activeGroups.Count < maxGroupsInShop)
            {
                SpawnNPCGroup();
            }
        }
    }
    
    /// <summary>
    /// Spawns a new group of NPCs
    /// </summary>
    private void SpawnNPCGroup()
    {
        // Determine group size
        int groupSize = (Random.value < 0.5f) ? 2 : 4; // 50% chance for either 2 or 4 NPCs
        
        // Create a new group
        NPCGroup newGroup = new GameObject("NPCGroup_" + activeGroups.Count).AddComponent<NPCGroup>();
        newGroup.transform.SetParent(transform);
        
        // Set up the group
        newGroup.Initialize(this, groupSize == 4);
        
        // Spawn NPCs for the group
        List<NPC> npcsInGroup = new List<NPC>();
        
        for (int i = 0; i < groupSize; i++)
        {
            // Randomly select an NPC prefab
            GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Count)];
            
            // Randomize position around spawn point
            Vector3 randomOffset = Random.insideUnitSphere * spawnAreaRadius;
            randomOffset.y = 0; // Keep NPCs on the ground
            Vector3 spawnPosition = spawnPoint.position + randomOffset;
            
            // Instantiate the NPC
            GameObject npcObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            NPC npcComponent = npcObject.GetComponent<NPC>();
            
            if (npcComponent == null)
            {
                npcComponent = npcObject.AddComponent<NPC>();
            }
            
            // Initialize the NPC
            npcComponent.Initialize(newGroup, i == 0); // First NPC is group leader
            
            // Set the cashier and exit positions
            if (cashierPosition != null) {
                npcComponent.SetCashierPosition(cashierPosition);
            } else {
                Debug.LogError("Cashier position not set in NPCManager!");
            }
            
            if (exitPosition != null) {
                npcComponent.SetExitPosition(exitPosition);
            } else {
                Debug.LogError("Exit position not set in NPCManager!");
            }
            
            npcComponent.name = "NPC_" + i;
            npcComponent.transform.SetParent(newGroup.transform);
            
            npcsInGroup.Add(npcComponent);
        }
        
        // Set NPCs in the group
        newGroup.SetNPCs(npcsInGroup);
        
        // Add to active groups
        activeGroups.Add(newGroup);
        
        // Have the group enter the shop
        newGroup.EnterShop(entryArea, doorObject);
        
        Debug.Log($"Spawned new NPC group with {groupSize} NPCs");
    }
    
    /// <summary>
    /// Finds an available table for a group of NPCs
    /// </summary>
    public TableController FindAvailableTable(NPCGroup group)
    {
        foreach (TableController table in availableTables)
        {
            if (table.IsAvailable() && table.CanFitGroup(group.GetNPCCount()))
            {
                return table;
            }
        }
        
        return null; // No available tables
    }
    
    /// <summary>
    /// Called when a group has exited the shop
    /// </summary>
    public void OnGroupExit(NPCGroup group)
    {
        if (activeGroups.Contains(group))
        {
            activeGroups.Remove(group);
            Destroy(group.gameObject, 2f); // Destroy the group after a delay
        }
    }
    
    /// <summary>
    /// Closes the shop and stops new customers from coming
    /// </summary>
    public void CloseShop()
    {
        isShopOpen = false;
        
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }
    }
} 