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
    [SerializeField] public float[] minSpawnArray;
    [SerializeField] public float[] maxSpawnArray;
    [SerializeField] public float minSpawnDelay;
    [SerializeField] public float maxSpawnDelay;
    [SerializeField] private int minGroupSize = 2;
    [SerializeField] private int maxGroupSize = 4;
    [SerializeField] private float spawnAreaRadius = 2f;
    [SerializeField] private int maxGroupsInShop = 3;
    
    [Header("Entry Settings")]
    [SerializeField] private Transform entryArea;
    [SerializeField] private GameObject doorObject;
    [SerializeField] private string doorAnimationName = "DoorOpen";
    
    [Header("Exit Settings")]
    [SerializeField] private Transform exitPoint; // Exit point for NPCs
    [SerializeField] private Transform cashierPoint; // Cashier position for payment
    [SerializeField] private BoxCollider exitTriggerZone; // Çıkış trigger zone'u
    
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

        // Exit trigger zone'u oluştur
        if (exitTriggerZone == null)
        {
            CreateExitTriggerZone();
        }
    }
    
    private void CreateExitTriggerZone()
    {
        // Yeni bir GameObject oluştur
        GameObject exitZone = new GameObject("ExitTriggerZone");
        exitZone.transform.SetParent(transform);
        
        // BoxCollider ekle
        exitTriggerZone = exitZone.AddComponent<BoxCollider>();
        exitTriggerZone.isTrigger = true;
        
        // Çıkış noktasının konumuna yerleştir
        if (exitPoint != null)
        {
            exitZone.transform.position = exitPoint.position;
            // Trigger zone'un boyutunu ayarla (genişlik, yükseklik, derinlik)
            exitTriggerZone.size = new Vector3(3f, 3f, 3f);
        }
        else
        {
            Debug.LogError("Exit point is not set in NPCManager!");
        }
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
            npcComponent.name = "NPC_" + i;
            npcComponent.transform.SetParent(newGroup.transform);
            
            // Set exit and cashier positions for this NPC
            if (exitPoint != null)
            {
                npcComponent.SetExitPosition(exitPoint);
            }
            else
            {
                Debug.LogError("Exit point is not set in NPCManager!");
            }
            
            if (cashierPoint != null)
            {
                npcComponent.SetCashierPosition(cashierPoint);
            }
            else
            {
                Debug.LogError("Cashier point is not set in NPCManager!");
            }
            
            npcsInGroup.Add(npcComponent);
        }
        
        // Set NPCs in the group
        newGroup.SetNPCs(npcsInGroup);
        
        // Add to active groups
        activeGroups.Add(newGroup);
        
        // Have the group enter the shop
        newGroup.EnterShop(entryArea, doorObject);
        
        Debug.Log($"Yeni NPC grubu oluşturuldu: {groupSize} müşteri");
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
            Destroy(group.gameObject, 0.5f); // Destroy the group after a short delay
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

    private void OnTriggerEnter(Collider other)
    {
        // NPC'nin çıkış trigger zone'una girdiğini kontrol et
        NPC npc = other.GetComponent<NPC>();
        if (npc != null)
        {
            // NPC'yi yok et
            NPCGroup group = npc.GetComponentInParent<NPCGroup>();
            if (group != null)
            {
                group.OnNPCLeft(npc);
            }
            Destroy(npc.gameObject);
        }
    }
} 