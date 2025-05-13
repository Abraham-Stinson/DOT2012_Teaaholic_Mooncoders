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
    [SerializeField] private Transform exitArea;
    [SerializeField] private float exitAreaRadius = 3f;
    
    [Header("Tables")]
    [SerializeField] private List<TableController> availableTables = new List<TableController>();
    
    // Internal state tracking
    private List<NPCGroup> activeGroups = new List<NPCGroup>();
    private Coroutine spawnRoutine;
    private bool isShopOpen = true;
    private List<NPCGroup> waitingGroups = new List<NPCGroup>();
    
    private void Start()
    {
        if (exitArea != null)
        {
            // Gizmo sphere eklenebilir veya visual bir gösterge
        }
        
        spawnRoutine = StartCoroutine(SpawnGroupsRoutine());
    }
    
    private void OnDrawGizmos()
    {
        if (exitArea != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(exitArea.position, exitAreaRadius);
        }
    }
    
    /// <summary>
    /// Coroutine that handles spawning NPC groups at regular intervals
    /// </summary>
    private IEnumerator SpawnGroupsRoutine()
    {
        while (isShopOpen)
        {
            float spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(spawnDelay);
            
            bool canSpawn = activeGroups.Count < maxGroupsInShop;
            
            CheckWaitingGroups();
            
            if (canSpawn)
            {
                SpawnNPCGroup();
            }
            else
            {
                Debug.Log("Cannot spawn new groups: Shop is at capacity");
                
                if (waitingGroups.Count > 0)
                {
                    CleanupStaleWaitingGroups();
                }
            }
        }
    }
    
    /// <summary>
    /// Spawns a new group of NPCs
    /// </summary>
    private void SpawnNPCGroup()
    {
        int groupSize = (Random.value < 0.5f) ? 2 : 4;
        
        NPCGroup newGroup = new GameObject("NPCGroup_" + activeGroups.Count).AddComponent<NPCGroup>();
        newGroup.transform.SetParent(transform);
        
        newGroup.Initialize(this, groupSize == 4);
        
        List<NPC> npcsInGroup = new List<NPC>();
        
        for (int i = 0; i < groupSize; i++)
        {
            GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Count)];
            
            Vector3 randomOffset = Random.insideUnitSphere * spawnAreaRadius;
            randomOffset.y = 0;
            Vector3 spawnPosition = spawnPoint.position + randomOffset;
            
            GameObject npcObject = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            NPC npcComponent = npcObject.GetComponent<NPC>();
            
            if (npcComponent == null)
            {
                npcComponent = npcObject.AddComponent<NPC>();
            }
            
            npcComponent.Initialize(newGroup, i == 0);
            
            if (cashierPosition != null) {
                npcComponent.SetCashierPosition(cashierPosition);
            } else {
                Debug.LogError("Cashier position not set in NPCManager!");
            }
            
            if (exitArea != null) {
                Vector3 randomExitOffset = Random.insideUnitSphere * exitAreaRadius;
                randomExitOffset.y = 0;
                
                GameObject tempExitPoint = new GameObject("ExitPoint_" + npcComponent.name);
                tempExitPoint.transform.position = exitArea.position + randomExitOffset;
                tempExitPoint.transform.SetParent(exitArea);
                
                npcComponent.SetExitPosition(tempExitPoint.transform);
            } else {
                Debug.LogError("Exit area not set in NPCManager!");
            }
            
            npcComponent.name = "NPC_" + i;
            npcComponent.transform.SetParent(newGroup.transform);
            
            npcsInGroup.Add(npcComponent);
        }
        
        newGroup.SetNPCs(npcsInGroup);
        
        TableController table = FindAvailableTable(newGroup);
        
        if (table != null)
        {
            activeGroups.Add(newGroup);
            
            newGroup.EnterShop(entryArea, doorObject);
            
            Debug.Log($"Spawned new NPC group with {groupSize} NPCs - Table available");
        }
        else
        {
            waitingGroups.Add(newGroup);
            
            newGroup.gameObject.SetActive(false);
            
            Debug.Log($"New NPC group with {groupSize} NPCs waiting for a table");
        }
    }
    
    /// <summary>
    /// Check if waiting groups can now enter the shop
    /// </summary>
    private void CheckWaitingGroups()
    {
        if (waitingGroups.Count == 0) return;
        
        for (int i = waitingGroups.Count - 1; i >= 0; i--)
        {
            NPCGroup waitingGroup = waitingGroups[i];
            TableController table = FindAvailableTable(waitingGroup);
            
            if (table != null)
            {
                waitingGroup.gameObject.SetActive(true);
                
                activeGroups.Add(waitingGroup);
                
                waitingGroups.RemoveAt(i);
                
                waitingGroup.EnterShop(entryArea, doorObject);
                
                Debug.Log("Waiting group now entering the shop");
                
                break;
            }
        }
    }
    
    /// <summary>
    /// Clean up any stale waiting groups that have been waiting too long
    /// </summary>
    private void CleanupStaleWaitingGroups()
    {
        if (waitingGroups.Count > 0)
        {
            NPCGroup oldestGroup = waitingGroups[0];
            Destroy(oldestGroup.gameObject);
            waitingGroups.RemoveAt(0);
            Debug.Log("Removed stale waiting group");
        }
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
        
        return null;
    }
    
    /// <summary>
    /// Called when a group has exited the shop
    /// </summary>
    public void OnGroupExit(NPCGroup group)
    {
        if (activeGroups.Contains(group))
        {
            activeGroups.Remove(group);
            Destroy(group.gameObject, 2f);
            
            CheckWaitingGroups();
            
            Debug.Log("Group has left the shop, checking waiting groups");
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
        
        foreach (NPCGroup group in waitingGroups)
        {
            Destroy(group.gameObject);
        }
        waitingGroups.Clear();
    }
} 