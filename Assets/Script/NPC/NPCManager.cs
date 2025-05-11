using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private float spawnInterval = 30f;
    [SerializeField] private int maxGroupSize = 4;
    [SerializeField] private int minGroupSize = 2;
    [SerializeField] private int maxActiveGroups = 5;
    
    [Header("Area References")]
    [SerializeField] private TriggerArea spawnArea;
    [SerializeField] private TriggerArea entryArea;
    [SerializeField] private TriggerArea exitArea;
    [SerializeField] private TriggerArea cashierArea;
    
    [Header("Scene References")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private TableManager tableManager;
    
    private List<NPCGroup> activeGroups = new List<NPCGroup>();
    private bool isSpawning = true;

    private void Start()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("Missing required references in NPCManager!");
            return;
        }
        
        StartCoroutine(SpawnGroups());
    }

    private bool ValidateReferences()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC Prefab is not assigned!");
            return false;
        }
        
        if (spawnArea == null || entryArea == null || exitArea == null || cashierArea == null)
        {
            Debug.LogError("One or more trigger areas are not assigned!");
            return false;
        }
        
        if (doorAnimator == null)
        {
            Debug.LogError("Door Animator is not assigned!");
            return false;
        }
        
        if (tableManager == null)
        {
            Debug.LogError("Table Manager is not assigned!");
            return false;
        }
        
        return true;
    }

    private IEnumerator SpawnGroups()
    {
        while (isSpawning)
        {
            if (activeGroups.Count < maxActiveGroups)
            {
                SpawnNewGroup();
            }
            
            yield return new WaitForSeconds(spawnInterval);
            
            // Clean up any destroyed groups
            activeGroups.RemoveAll(group => group == null);
        }
    }

    private void SpawnNewGroup()
    {
        int groupSize = Random.Range(minGroupSize, maxGroupSize + 1);
        GameObject groupObject = new GameObject($"NPC_Group_{activeGroups.Count}");
        groupObject.transform.SetParent(transform);
        
        NPCGroup group = groupObject.AddComponent<NPCGroup>();
        
        // Initialize the group
        group.Initialize(groupSize, entryArea, exitArea, cashierArea, doorAnimator, tableManager);
        
        // Spawn individual NPCs
        for (int i = 0; i < groupSize; i++)
        {
            Vector3 spawnPosition = spawnArea.GetRandomPositionInArea();
            GameObject npcObject = Instantiate(npcPrefab, spawnPosition, Quaternion.identity, groupObject.transform);
            
            NPC npc = npcObject.GetComponent<NPC>();
            if (npc != null)
            {
                npc.Initialize(i == 0); // First NPC is the leader
                group.AddNPC(npc);
            }
        }
        
        activeGroups.Add(group);
        group.StartGroupBehavior();
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    // For debug testing
    [ContextMenu("Spawn Test Group")]
    public void DebugSpawnGroup()
    {
        SpawnNewGroup();
    }
} 