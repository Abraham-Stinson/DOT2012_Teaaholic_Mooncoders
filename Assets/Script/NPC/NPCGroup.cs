using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a group of NPCs that enter the tea shop together
/// </summary>
public class NPCGroup : MonoBehaviour
{
    [Header("Group Settings")]
    [SerializeField] private float secondDrinkChance = 0.3f; // 30% chance for second round
    
    [Header("Patience Settings")]
    [SerializeField] private float groupPatienceTime = 120f; // Base patience time for the group
    [SerializeField] private float patienceBonusPerDrink = 15f; // Patience bonus when a drink is served
    [SerializeField] private float patienceBonusPerGame = 20f; // Patience bonus when a game is served
    
    // State tracking for deciding exit behavior
    private bool receivedRequestedGame = false;
    private bool receivedAtLeastOneDrink = false;
    private bool receivedAllDrinks = false;
    private bool patienceExpired = false;
    
    // Table games
    private const string TAVLA_GAME = "Tavla";
    private const string ISKAMBIL_GAME = "Iskambil";
    private const string OKEY_GAME = "Okey";
    
    // State tracking
    private NPCManager npcManager;
    private List<NPC> npcs = new List<NPC>();
    private TableController assignedTable;
    private string requestedGame;
    private bool isOkeyGroup = false;
    private bool hasGameBox = false;
    private bool allSeated = false;
    private bool allDrinksServed = false;
    private bool allDrinksFinished = false;
    private bool isSecondOrder = false;
    private bool isLeaving = false;
    private int seatedCount = 0;
    private int servedCount = 0;
    private int finishedDrinkingCount = 0;
    private int npcLeftCount = 0;
    
    // Patience tracking
    private float currentGroupPatienceTime;
    private Coroutine patienceCoroutine;
    
    /// <summary>
    /// Initialize the group with manager reference and whether it's a 4-person group
    /// </summary>
    public void Initialize(NPCManager manager, bool isOkey)
    {
        npcManager = manager;
        isOkeyGroup = isOkey;
        
        // Initialize patience time
        currentGroupPatienceTime = groupPatienceTime;
        
        // Determine which game the group wants
        if (isOkeyGroup)
        {
            requestedGame = OKEY_GAME;
        }
        else
        {
            // 50% chance for Tavla or Iskambil
            requestedGame = Random.value < 0.5f ? TAVLA_GAME : ISKAMBIL_GAME;
        }
    }
    
    /// <summary>
    /// Set the NPCs in this group
    /// </summary>
    public void SetNPCs(List<NPC> groupNpcs)
    {
        npcs = groupNpcs;
    }
    
    /// <summary>
    /// Get the number of NPCs in the group
    /// </summary>
    public int GetNPCCount()
    {
        return npcs.Count;
    }
    
    /// <summary>
    /// Check if all NPCs in the group are seated
    /// </summary>
    public bool IsFullySeated()
    {
        // Hiç NPC yoksa grup tam oturmuş sayılamaz
        if (npcs.Count == 0)
            return false;
            
        // allSeated değişkeni, tüm NPClerin oturduğunu gösterir
        return allSeated;
    }
    
    /// <summary>
    /// Called to make the group enter the shop
    /// </summary>
    public void EnterShop(Transform entryArea, GameObject door)
    {
        StartCoroutine(EnterShopRoutine(entryArea, door));
    }
    
    /// <summary>
    /// Coroutine to handle the shop entry process
    /// </summary>
    private IEnumerator EnterShopRoutine(Transform entryArea, GameObject door)
    {
        // First, select a random position in the entry area for the group
        Vector3 entryPosition = entryArea.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        
        // Get the leader to move to the entry point
        NPC leader = GetGroupLeader();
        
        if (leader != null)
        {
            // Leader moves to the entry and other NPCs follow
            leader.MoveTo(entryArea, null);
            
            // Wait for leader to get close to entry
            float minDistance = 3f;
            while (Vector3.Distance(leader.transform.position, entryArea.position) > minDistance)
            {
                yield return null;
            }
            
            // Animate door open
            Animator doorAnimator = door?.GetComponent<Animator>();
            if (doorAnimator != null)
            {
                doorAnimator.SetTrigger("Open");
                //yield return new WaitForSeconds(1f); // Wait for door animation
            }
            
            // Find a table
            yield return StartCoroutine(FindAndGoToTable());
        }
    }
    
    /// <summary>
    /// Find an available table and make the group go to it
    /// </summary>
    private IEnumerator FindAndGoToTable()
    {
        // Try to find an available table
        TableController table = npcManager.FindAvailableTable(this);
        
        if (table == null)
        {
            // No table available, exit the shop
            Debug.Log("No tables available, group is leaving");
            ExitShop();
            yield break;
        }
        
        // Assign this table to the group
        assignedTable = table;
        assignedTable.SetOccupiedBy(this);
        
        // Assign chairs to each NPC
        List<Chair> chairs = assignedTable.GetChairs();
        
        if (chairs.Count < npcs.Count)
        {
            Debug.LogError($"Not enough chairs ({chairs.Count}) for group of {npcs.Count}");
            ExitShop();
            yield break;
        }
        
        // Shuffle chairs to randomize seating
        chairs = ShuffleList(chairs);
        
        // Assign each NPC to a chair
        for (int i = 0; i < npcs.Count; i++)
        {
            npcs[i].AssignChair(chairs[i]);
        }
    }
    
    /// <summary>
    /// Called when an NPC in the group sits down
    /// </summary>
    public void OnNPCSatDown(NPC npc)
    {
        seatedCount++;
        Debug.Log($"NPC sat down. Seated count: {seatedCount}, Total NPCs: {npcs.Count}");
        
        // If all NPCs are seated, request a game
        if (seatedCount == npcs.Count)
        {
            Debug.Log("All NPCs are seated, requesting game now");
            allSeated = true;
            RequestGame();
        }
    }
    
    /// <summary>
    /// Called when an NPC in the group gets up
    /// </summary>
    public void OnNPCGotUp(NPC npc)
    {
        seatedCount--;
    }
    
    /// <summary>
    /// Request a game box for the table
    /// </summary>
    private void RequestGame()
    {
        Debug.Log($"[GROUP] Grup oyun istiyor: {requestedGame}");
        
        // Update UI or send a message to the player about the requested game
        
        // Start the group patience timer
        StartGroupPatienceTimer();
        
        // Force update the table to make sure the UI shows the request
        if (assignedTable != null)
        {
            Debug.Log($"[GROUP] Atanan masa: {assignedTable.name}, İstenen oyun: {requestedGame}");
            assignedTable.SetRequestedGame(requestedGame);
            
            // Masa referansını kaybetme ihtimaline karşı tag ile işaretleyelim
            gameObject.tag = requestedGame;
        }
        else
        {
            Debug.LogError("[GROUP] Oyun istenirken atanmış masa bulunamadı!");
        }
    }
    
    /// <summary>
    /// Called when a game box is given to the group
    /// </summary>
    public void ReceiveGameBox(string gameType)
    {
        Debug.Log($"Group received game box: {gameType}, Requested: {requestedGame}");
        
        if (gameType == requestedGame)
        {
            // Correct game provided
            Debug.Log($"Correct game provided: {gameType}");
            hasGameBox = true;
            receivedRequestedGame = true;
            
            // Add patience bonus for receiving the game
            AddGroupPatience(patienceBonusPerGame);
            
            // Set has game box for all NPCs
            foreach (NPC npc in npcs)
            {
                npc.SetHasGameBox(true);
                npc.StartPlaying();
            }
            
            // Request drinks after receiving the game
            StartCoroutine(OrderDrinksAfterDelay());
        }
        else
        {
            // Wrong game provided, maintain patience timer
            Debug.LogWarning($"Wrong game provided! Received: {gameType}, Expected: {requestedGame}");
        }
    }
    
    /// <summary>
    /// Order drinks after a short delay
    /// </summary>
    private IEnumerator OrderDrinksAfterDelay()
    {
        yield return new WaitForSeconds(3f); // Wait a bit before ordering
        
        // Each NPC orders a drink
        foreach (NPC npc in npcs)
        {
            npc.OrderDrink();
        }
    }
    
    /// <summary>
    /// Called when an NPC is served a drink
    /// </summary>
    public void OnNPCServedDrink(NPC npc, string servedDrink)
    {
        servedCount++;
        
        // Track that at least one drink was served
        receivedAtLeastOneDrink = true;
        
        // Add patience bonus for receiving a drink
        AddGroupPatience(patienceBonusPerDrink);
        
        Debug.Log($"NPC was served a drink: {servedDrink}. Group patience extended. Current patience: {currentGroupPatienceTime}");
        
        // Check if all NPCs have been served
        if (servedCount == npcs.Count)
        {
            allDrinksServed = true;
            receivedAllDrinks = true;
        }
    }
    
    /// <summary>
    /// Called when an NPC finishes drinking
    /// </summary>
    public void OnNPCFinishedDrinking(NPC npc)
    {
        finishedDrinkingCount++;
        
        // Check if all NPCs have finished drinking
        if (finishedDrinkingCount == npcs.Count)
        {
            allDrinksFinished = true;
            
            // Decide whether to order a second round
            if (!isSecondOrder && Random.value < secondDrinkChance)
            {
                isSecondOrder = true;
                StartCoroutine(OrderSecondRound());
            }
            else
            {
                // Prepare to leave
                StartCoroutine(PrepareToLeave());
            }
        }
    }
    
    /// <summary>
    /// Order a second round of drinks
    /// </summary>
    private IEnumerator OrderSecondRound()
    {
        yield return new WaitForSeconds(5f); // Wait a bit before ordering again
        
        // Reset drink counters
        servedCount = 0;
        finishedDrinkingCount = 0;
        allDrinksServed = false;
        allDrinksFinished = false;
        
        // Each NPC orders a new drink with "Tazele:" prefix
        foreach (NPC npc in npcs)
        {
            npc.OrderDrinkRefresh();
        }
    }
    
    /// <summary>
    /// Prepare the group to leave the shop
    /// </summary>
    private IEnumerator PrepareToLeave()
    {
        yield return new WaitForSeconds(5f); // Finish playing
        
        // Stop playing animations
        foreach (NPC npc in npcs)
        {
            npc.StopPlaying();
        }
        
        // Make all NPCs get up and exit
        ExitShop();
    }
    
    /// <summary>
    /// Make the group exit the shop
    /// </summary>
    public void ExitShop()
    {
        if (isLeaving) return;
        
        isLeaving = true;
        
        // Handle cups on the table
        MakeCupsInteractable();
        
        // Determine exit behavior based on scenarios
        bool shouldPayAtCashier = ShouldPayAtCashier();
        
        // Tell NPCs to exit based on determined behavior
        foreach (NPC npc in npcs)
        {
            if (npc.IsGroupLeader() && shouldPayAtCashier)
            {
                // Leader goes to cashier to pay
                npc.ExitShopThroughCashier();
            }
            else
            {
                // Others go directly to exit
                npc.ExitShopDirectly();
            }
        }
        
        // Make the table available again
        if (assignedTable != null)
        {
            assignedTable.SetAvailable();
            
            // Make the game box pickable again
            if (hasGameBox)
            {
                assignedTable.MakeGameBoxPickable();
            }
        }
    }
    
    /// <summary>
    /// Called when a NPC leaves the scene
    /// </summary>
    public void OnNPCLeft(NPC npc)
    {
        npcLeftCount++;
        
        // If all NPCs have left, notify the manager
        if (npcLeftCount == npcs.Count)
        {
            npcManager.OnGroupExit(this);
        }
    }
    
    /// <summary>
    /// Start the patience timer for the group
    /// </summary>
    private void StartGroupPatienceTimer()
    {
        // Stop any existing patience timer
        if (patienceCoroutine != null)
        {
            StopCoroutine(patienceCoroutine);
        }
        
        // Reset patience time to the base value
        currentGroupPatienceTime = groupPatienceTime;
        
        // Start a new patience timer
        patienceCoroutine = StartCoroutine(GroupPatienceCountdown());
    }
    
    /// <summary>
    /// Add time to the group's patience timer
    /// </summary>
    /// <param name="bonusTime">Amount of time to add in seconds</param>
    private void AddGroupPatience(float bonusTime)
    {
        // Add the bonus time to the current patience time
        currentGroupPatienceTime += bonusTime;
        
        Debug.Log($"Group patience increased by {bonusTime} seconds. New patience: {currentGroupPatienceTime}");
        
        // If the group is about to leave due to patience expiring, refresh their status
        if (isLeaving)
        {
            Debug.Log("Group was about to leave, but patience has been extended.");
            isLeaving = false;
        }
    }
    
    /// <summary>
    /// Coroutine to handle group patience countdown
    /// </summary>
    private IEnumerator GroupPatienceCountdown()
    {
        Debug.Log($"Group patience timer started: {currentGroupPatienceTime} seconds");
        
        while (currentGroupPatienceTime > 0)
        {
            yield return new WaitForSeconds(1f);
            currentGroupPatienceTime--;
            
            // Update the patience status for all NPCs to display it to the player
            foreach (NPC npc in npcs)
            {
                npc.UpdatePatienceFromGroup(currentGroupPatienceTime / groupPatienceTime);
            }
        }
        
        // Patience has expired, group is leaving
        Debug.Log("Group patience has expired, group is leaving");
        ExitShop();
    }
    
    /// <summary>
    /// Called when group patience timer expires
    /// </summary>
    public void OnGroupPatienceExpired()
    {
        // Set flag that patience expired
        patienceExpired = true;
        
        // Make the whole group leave
        if (!isLeaving)
        {
            Debug.Log("Group is leaving due to patience expiring");
            ExitShop();
        }
    }
    
    /// <summary>
    /// Get the requested game for this group
    /// </summary>
    public string GetRequestedGame()
    {
        Debug.Log($"[GROUP] GetRequestedGame çağrıldı: {requestedGame}");
        
        // Eğer requestedGame boşsa, tag'e bak
        if (string.IsNullOrEmpty(requestedGame) && !string.IsNullOrEmpty(gameObject.tag) && 
            (gameObject.tag == "Tavla" || gameObject.tag == "Iskambil" || gameObject.tag == "Okey"))
        {
            requestedGame = gameObject.tag;
            Debug.Log($"[GROUP] Tag'den oyun alındı: {requestedGame}");
        }
        
        return requestedGame;
    }
    
    /// <summary>
    /// Get the group leader NPC
    /// </summary>
    private NPC GetGroupLeader()
    {
        foreach (NPC npc in npcs)
        {
            if (npc.IsGroupLeader())
            {
                return npc;
            }
        }
        
        // Fallback to first NPC if no leader is found
        return npcs.Count > 0 ? npcs[0] : null;
    }
    
    /// <summary>
    /// Shuffle a list using Fisher-Yates algorithm
    /// </summary>
    private List<T> ShuffleList<T>(List<T> list)
    {
        List<T> shuffledList = new List<T>(list);
        
        for (int i = 0; i < shuffledList.Count; i++)
        {
            T temp = shuffledList[i];
            int randomIndex = Random.Range(i, shuffledList.Count);
            shuffledList[i] = shuffledList[randomIndex];
            shuffledList[randomIndex] = temp;
        }
        
        return shuffledList;
    }
    
    /// <summary>
    /// Determines if the group leader should pay at the cashier based on scenario
    /// </summary>
    private bool ShouldPayAtCashier()
    {
        // SENARYO 1 & 2: Oyun alınmadı veya hiç içecek gelmedi ve sabrı tükendiyse
        if (patienceExpired && (!receivedRequestedGame || !receivedAtLeastOneDrink))
        {
            // Grup lideri dahil herkes doğrudan çıkışa gider
            Debug.Log("Scenario: Patience expired without game or drinks - entire group leaves directly");
            return false;
        }
        
        // SENARYO 3, 4, 5: Oyun alındı ve en az bir içecek geldi
        // - Sabrı tükendi ama oyun ve en az bir içecek geldi
        // - Oyun ve tüm içecekler geldi ama tazelemede sabır tükendi
        // - Tüm istekler karşılandı
        if (receivedRequestedGame && receivedAtLeastOneDrink)
        {
            // Bu senaryolarda grup lideri kasaya gider, diğerleri çıkışa gider
            Debug.Log("Scenario: Received game and at least one drink - leader pays at cashier");
            return true;
        }
        
        // Varsayılan durum - hiçbir koşul karşılanmadıysa (bu duruma düşmemeli)
        Debug.LogWarning("No specific exit scenario matched - defaulting to direct exit");
        return false;
    }
    
    /// <summary>
    /// Makes cups on the table interactable when the group leaves
    /// </summary>
    private void MakeCupsInteractable()
    {
        if (assignedTable != null)
        {
            // Find all cups that might be children of the table
            foreach (Transform child in assignedTable.transform)
            {
                Tea_Cup teaCup = child.GetComponent<Tea_Cup>();
                if (teaCup != null)
                {
                    // Make sure it's on the interactable layer
                    child.gameObject.layer = LayerMask.NameToLayer("Interactable");
                    
                    // Make sure it has a proper collider
                    Collider collider = child.GetComponent<Collider>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                    
                    // Make sure it has proper physics
                    Rigidbody rb = child.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true; // Keep it from falling
                        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    }
                    
                    // Ensure it's marked as dirty
                    DirtyStatus dirtyStatus = child.GetComponent<DirtyStatus>();
                    if (dirtyStatus != null && !dirtyStatus.isDirty)
                    {
                        dirtyStatus.isDirty = true;
                    }
                }
            }
        }
    }
} 