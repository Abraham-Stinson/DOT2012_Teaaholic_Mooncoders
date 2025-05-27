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
    private bool isAnyNPCDrinking = false;

    // Hesap takibi
    private float totalBill = 0f;

    // Patience tracking
    private float currentGroupPatienceTime;
    private Coroutine patienceCoroutine;

    private Coroutine enterShopCoroutine;
    private Coroutine orderDrinksCoroutine;
    private Coroutine secondRoundCoroutine;
    private Coroutine prepareToLeaveCoroutine;

    // Initialize the group with manager reference and whether it's a 4-person group
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

    // Set the NPCs in this group
    public void SetNPCs(List<NPC> groupNpcs)
    {
        npcs = groupNpcs;
    }

    // Get the number of NPCs in the group
    public int GetNPCCount()
    {
        return npcs.Count;
    }

    // Check if all NPCs in the group are seated
    public bool IsFullySeated()
    {
        // Hiç NPC yoksa grup tam oturmuş sayılamaz
        if (npcs.Count == 0)
            return false;

        // allSeated değişkeni, tüm NPClerin oturduğunu gösterir
        return allSeated;
    }

    // Called to make the group enter the shop
    public void EnterShop(Transform entryArea, GameObject door)
    {
        if (enterShopCoroutine != null)
        {
            StopCoroutine(enterShopCoroutine);
        }
        enterShopCoroutine = StartCoroutine(EnterShopRoutine(entryArea, door));
    }

    // Coroutine to handle the shop entry process
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

    // Find an available table and make the group go to it
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
    // Called when an NPC in the group sits down
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
    // Called when an NPC in the group gets up
    public void OnNPCGotUp(NPC npc)
    {
        seatedCount--;
    }

    // Request a game box for the table
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

    // Called when a game box is given to the group
    public void ReceiveGameBox(string gameType)
    {
        Debug.Log($"Group received game box: {gameType}, Requested: {requestedGame}");

        if (gameType == requestedGame)
        {
            if (requestedGame == OKEY_GAME)
            {
                SoundManager.Instance.PlayOkey();
            }
            else if (requestedGame == TAVLA_GAME)
            {
                SoundManager.Instance.PlayTavla();
            }
            else if (requestedGame == ISKAMBIL_GAME)
            {
                SoundManager.Instance.PlayIskambil();
            }
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

    // Order drinks after a short delay
    private IEnumerator OrderDrinksAfterDelay()
    {
        if (orderDrinksCoroutine != null)
        {
            StopCoroutine(orderDrinksCoroutine);
        }

        yield return new WaitForSeconds(3f);

        foreach (NPC npc in npcs)
        {
            if (npc != null) // Null check ekle
            {
                npc.OrderDrink();
            }
        }
    }

    // Called when an NPC is served a drink
    public void OnNPCServedDrink(NPC npc, string drinkName)
    {
        servedCount++;

        // Track that at least one drink was served
        receivedAtLeastOneDrink = true;

        // Add patience bonus for receiving a drink
        AddGroupPatience(patienceBonusPerDrink);
        isAnyNPCDrinking = true;

        Debug.Log($"NPC was served a drink: {drinkName}. Group patience extended. Current patience: {currentGroupPatienceTime}");

        // Check if all NPCs have been served
        if (servedCount == npcs.Count)
        {
            allDrinksServed = true;
            receivedAllDrinks = true;
        }
    }

    // Called when an NPC finishes drinking
    public void OnNPCFinishedDrinking(NPC npc)
    {

        Debug.Log($"[NPCGroup] {npc.name} içecek içmeyi bitirdi, sayaçlar güncelleniyor");
        finishedDrinkingCount++;
        // Check if any NPCs are still drinking
        isAnyNPCDrinking = finishedDrinkingCount < servedCount;

        Debug.Log($"[NPCGroup] Grup içmeyi bitirenler: {finishedDrinkingCount}/{npcs.Count}");

        // Check if all NPCs have finished drinking
        if (finishedDrinkingCount >= npcs.Count)
        {
            Debug.Log("[NPCGroup] Tüm NPCler içmeyi bitirdi");
            allDrinksFinished = true;

            // Decide whether to order a second round
            if (!isSecondOrder && Random.value < secondDrinkChance)
            {
                Debug.Log("[NPCGroup] Grup ikinci tur içecek isteyecek");
                isSecondOrder = true;
                StartCoroutine(OrderSecondRound());
            }
            else
            {
                Debug.Log("[NPCGroup] Grup içmeyi bitirdi ve çıkışa hazırlanıyor");
                // Prepare to leave
                StartCoroutine(PrepareToLeave());
            }
        }
    }

    // Order a second round of drinks
    private IEnumerator OrderSecondRound()
    {
        if (secondRoundCoroutine != null)
        {
            StopCoroutine(secondRoundCoroutine);
        }

        Debug.Log("[NPCGroup] İkinci tur içecek siparişi için bekleniyor...");
        yield return new WaitForSeconds(5f); // Wait a bit before ordering again

        // Reset drink counters
        servedCount = 0;
        finishedDrinkingCount = 0;
        allDrinksServed = false;
        allDrinksFinished = false;

        Debug.Log("[NPCGroup] İkinci tur içecek siparişi veriliyor");
        // Each NPC orders a new drink with "Tazele:" prefix
        foreach (NPC npc in npcs)
        {
            if (npc != null)
            {
                npc.OrderDrinkRefresh();
            }
        }
    }

    // Prepare the group to leave the shop
    private IEnumerator PrepareToLeave()
    {
        if (prepareToLeaveCoroutine != null)
        {
            StopCoroutine(prepareToLeaveCoroutine);
        }

        Debug.Log("[NPCGroup] Grup çıkış için hazırlanıyor - 5 saniye bekleniyor");
        yield return new WaitForSeconds(5f); // Finish playing

        Debug.Log("[NPCGroup] Grup oyun oynamayı bırakıyor");
        // Stop playing animations
        foreach (NPC npc in npcs)
        {
            if (npc != null)
            {
                npc.StopPlaying();
            }
            if (npc.IsGroupLeader())
            {
                Debug.Log("[Adisyon] Grup lideri adisyonu güncelliyor");
                npc.UpdateAdisyon();
            }
        }

        Debug.Log("[NPCGroup] Grup çıkışa yönlendiriliyor");
        // Make all NPCs get up and exit
        ExitShop();
    }

    // Make the group exit the shop
    public void ExitShop()
    {
        if (isLeaving) return;

        Debug.Log("[NPCGroup] ExitShop çağrıldı - grup çıkış sürecine başlıyor");
        isLeaving = true;

        // Handle cups on the table
        MakeCupsInteractable();

        // Determine exit behavior based on scenarios
        bool shouldPayAtCashier = ShouldPayAtCashier();
        Debug.Log($"[NPCGroup] Kasada ödeme yapılacak mı: {shouldPayAtCashier}");

        // Grup lideri ve diğer üye sayısını kontrol et
        NPC groupLeader = GetGroupLeader();
        if (groupLeader == null)
        {
            Debug.LogError("[NPCGroup] Grup lideri bulunamadı! Çıkış süreci başarısız olabilir.");
            return;
        }

        // Tell NPCs to exit based on determined behavior
        foreach (NPC npc in npcs)
        {
            if (npc == null)
            {
                Debug.LogWarning("[NPCGroup] NPC null referansı, atlanıyor.");
                continue;
            }

            if (npc.IsGroupLeader() && shouldPayAtCashier)
            {
                // Lider kasaya gider
                Debug.Log($"[NPCGroup] {npc.name} kasaya gidiyor");
                npc.ExitShopThroughCashier();
            }
            else
            {
                // Diğerleri direkt çıkışa gider
                Debug.Log($"[NPCGroup] {npc.name} çıkışa gidiyor");
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
    // Called when a NPC leaves the scene
    public void OnNPCLeft(NPC npc)
    {
        if (npc == null)
        {
            Debug.LogWarning("[NPCGroup] OnNPCLeft çağrıldı ama NPC null!");
            return;
        }

        Debug.Log($"[NPCGroup] {npc.name} gruptan ayrıldı");
        npcLeftCount++;

        // If all NPCs have left, notify the manager
        if (npcLeftCount >= npcs.Count)
        {
            Debug.Log("[NPCGroup] Tüm NPCler gruptan ayrıldı, NPCManager'a bildirim yapılıyor");
            npcManager.OnGroupExit(this);
        }
    }
    // Start the patience timer for the group
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
    // Add time to the group's patience timer
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

    // Coroutine to handle group patience countdown
    private IEnumerator GroupPatienceCountdown()
    {
        Debug.Log($"Group patience timer started: {currentGroupPatienceTime} seconds");

        while (currentGroupPatienceTime > 0)
        {
            yield return new WaitForSeconds(1f);

            // Only decrease patience if no one is drinking
            if (!isAnyNPCDrinking)
            {
                currentGroupPatienceTime--;

                // Update the patience status for all NPCs to display it to the player
                foreach (NPC npc in npcs)
                {
                    npc.UpdatePatienceFromGroup(currentGroupPatienceTime / groupPatienceTime);
                }
            }
            else
            {
                Debug.Log("Patience timer paused - someone is drinking");
            }
        }

        // Patience has expired, group is leaving
        Debug.Log("Group patience has expired, group is leaving");
        ExitShop();
    }

    // Called when group patience timer expires
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
    // Get the requested game for this group
    public string GetRequestedGame()
    {
        //Debug.Log($"[GROUP] GetRequestedGame çağrıldı: {requestedGame}");

        // Eğer requestedGame boşsa, tag'e bak
        if (string.IsNullOrEmpty(requestedGame) && !string.IsNullOrEmpty(gameObject.tag) &&
            (gameObject.tag == "Tavla" || gameObject.tag == "Iskambil" || gameObject.tag == "Okey"))
        {
            requestedGame = gameObject.tag;
            //Debug.Log($"[GROUP] Tag'den oyun alındı: {requestedGame}");
        }

        return requestedGame;
    }
    // Get the group leader NPC
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
    // Shuffle a list using Fisher-Yates algorithm
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
    // Determines if the group leader should pay at the cashier based on scenario
    private bool ShouldPayAtCashier()
    {
        // SENARYO 1 & 2: Oyun alınmadı veya hiç içecek gelmedi ve sabrı tükendiyse
        if (patienceExpired && (!receivedRequestedGame || !receivedAtLeastOneDrink))
        {
            // Grup lideri dahil herkes doğrudan çıkışa gider
            Debug.Log("Senaryo: Sabır tükendi (oyun veya içecek gelmedi) - tüm grup doğrudan çıkar");
            return false;
        }

        // SENARYO 3, 4, 5: Oyun alındı ve en az bir içecek geldi
        // - Sabrı tükendi ama oyun ve en az bir içecek geldi
        // - Oyun ve tüm içecekler geldi ama tazelemede sabır tükendi
        // - Tüm istekler karşılandı
        if (receivedRequestedGame && receivedAtLeastOneDrink)
        {
            // Bu senaryolarda grup lideri kasaya gider, diğerleri çıkışa gider
            Debug.Log("Senaryo: Oyun ve en az bir içecek geldi - lider kasada ödeme yapar");
            return true;
        }

        // Varsayılan durum - hiçbir koşul karşılanmadıysa (bu duruma düşmemeli)
        Debug.LogWarning("Hiçbir çıkış senaryosu eşleşmedi - varsayılan olarak doğrudan çıkış");
        return false;
    }
    // Makes cups on the table interactable when the group leaves
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
    // Toplam hesaba içecek ücreti ekle
    public void AddToBill(float amount)
    {
        totalBill += amount;
        Debug.Log($"Grup hesabına {amount} TL eklendi. Toplam: {totalBill} TL");
    }

    // Toplam hesabı döndür
    public float GetTotalBill()
    {
        return totalBill;
    }

    // Hesabı sıfırla
    public void ResetBill()
    {
        totalBill = 0f;
        Debug.Log("Grup hesabı sıfırlandı");
    }

    private void OnDestroy()
    {
        // Tüm coroutine'leri durdur
        if (enterShopCoroutine != null) StopCoroutine(enterShopCoroutine);
        if (orderDrinksCoroutine != null) StopCoroutine(orderDrinksCoroutine);
        if (secondRoundCoroutine != null) StopCoroutine(secondRoundCoroutine);
        if (prepareToLeaveCoroutine != null) StopCoroutine(prepareToLeaveCoroutine);
        if (patienceCoroutine != null) StopCoroutine(patienceCoroutine);

        // NPC listesini temizle
        if (npcs != null)
        {
            npcs.Clear();
        }
    }
}