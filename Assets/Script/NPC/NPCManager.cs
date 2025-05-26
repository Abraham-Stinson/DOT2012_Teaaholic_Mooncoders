using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the spawning and management of NPC groups in the tea shop.
/// </summary>
public class NPCManager : MonoBehaviour
{
    // private Coroutine enterShopCoroutine; // Bu değişkenler NPCGroup sınıfına ait
    // private Coroutine orderDrinksCoroutine;
    // private Coroutine secondRoundCoroutine;
    // private Coroutine prepareToLeaveCoroutine;

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
    
    [Header("Time Settings")]
    [SerializeField] private DayNightCycleController dayNightController; // Reference to day-night cycle controller
    [SerializeField] private int npcSpawnCutoffHour = 22; // No NPCs spawn after 10 PM (22:00)
    
    // Internal state tracking
    private List<NPCGroup> activeGroups = new List<NPCGroup>();
    private Coroutine spawnRoutine;
    private bool isShopOpen = true;
    
    private void Start()
    {
        // Find DayNightCycleController if not assigned
        if (dayNightController == null)
        {
            dayNightController = FindObjectOfType<DayNightCycleController>();
            if (dayNightController == null)
            {
                Debug.LogWarning("DayNightCycleController not found! NPCs will spawn regardless of time.");
            }
        }
        
        // Start spawning NPCs
        spawnRoutine = StartCoroutine(SpawnGroupsRoutine());

        // Exit trigger zone'u oluştur
        if (exitTriggerZone == null)
        {
            CreateExitTriggerZone();
        }
    }
    
    private void Update()
    {
        // Check if shop should be closed based on time or DayNightCycleController's flag
        if (isShopOpen && dayNightController != null && 
            (dayNightController.GetCurrentHour() >= npcSpawnCutoffHour || dayNightController.IsNPCSpawningDisabled()))
        {
            // It's past closing time or NPC spawning was disabled by DayNightCycleController
            Debug.Log($"Shop is now closed for new customers (Hour: {dayNightController.GetCurrentHour()}, NPC spawning disabled: {dayNightController.IsNPCSpawningDisabled()})");
            isShopOpen = false;
            
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
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
            if (activeGroups.Count < maxGroupsInShop && IsShopOpenForNewCustomers())
            {
                SpawnNPCGroup();
            }
        }
    }
    
    /// <summary>
    /// Checks if the shop is still open for new customers based on the time
    /// </summary>
    private bool IsShopOpenForNewCustomers()
    {
        // If we don't have a day-night controller, assume the shop is always open
        if (dayNightController == null)
            return true;
            
        // Get the current hour from the day-night controller
        int currentHour = dayNightController.GetCurrentHour();
        
        // Check if DayNightCycleController has disabled NPC spawning
        bool npcSpawningDisabled = dayNightController.IsNPCSpawningDisabled();
        
        // No new customers after the cutoff hour (10 PM / 22:00) or if spawning is disabled
        return currentHour < npcSpawnCutoffHour && !npcSpawningDisabled;
    }
    
    /// <summary>
    /// Spawns a new group of NPCs
    /// </summary>
    private void SpawnNPCGroup()
    {
        // Double-check time before spawning (in case time changed during delay)
        if (!IsShopOpenForNewCustomers())
        {
            Debug.Log("Shop is closed for new customers (after 10 PM). No new NPCs will spawn.");
            return;
        }
        
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
    // Bu metotlar NPCGroup sınıfına ait, NPCManager'da gerekli değil
    // ve derleyici hatasına neden oluyordu, bu yüzden kaldırıldı.

    private void OnDestroy()
    {
        // Spawning coroutine'ini durdur
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        // Aktif grupları temizle
        foreach (var group in activeGroups)
        {
            if (group != null)
            {
                Destroy(group.gameObject);
            }
        }
        activeGroups.Clear();
    }
} 