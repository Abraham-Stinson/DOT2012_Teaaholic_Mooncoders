using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles individual NPC behavior, movement, and states
/// </summary>
public class NPC : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sitOffset = 0.5f; // How much to offset when sitting
    [SerializeField] private Animator animator;
    
    [Header("Drinking Settings")]
    [SerializeField] private float minDrinkDuration = 3f; // Minimum time to drink (seconds)
    [SerializeField] private float maxDrinkDuration = 8f; // Maximum time to drink (seconds)
    
    [Header("Order Settings")]
    [SerializeField] private string[] drinkOptions = new string[] {
        "Light_Tea", "Rabbit_Blood_Tea", "Brewed_Tea", 
        "Coffee_Drink", "Banana_Oralet", "Kiwi_Oralet", 
        "Orange_Oralet", "Strawberry_Oralet"
    };
    
    // State fields
    private NavMeshAgent navAgent;
    private NPCGroup group;
    private bool isGroupLeader = false;
    private bool isWaiting = false;
    private float patiencePercentage = 1.0f; // Normalized patience value from the group
    private Chair assignedChair;
    private string requestedDrink = "";
    private bool hasDrink = false;
    private bool hasGameBox = false;
    private bool isSeated = false;
    private bool isReady = false;
    private bool isExiting = false;
    private bool _isPaying = false;
    private Transform targetPosition;
    private Transform cashierPosition;
    private Transform exitPosition;
    private GameObject currentCup;
    
    // Public property for isPaying
    public bool isPaying
    {
        get { return _isPaying; }
    }
    
    // Animation parameters 
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsSitting = Animator.StringToHash("IsSitting");
    private static readonly int IsDrinking = Animator.StringToHash("IsDrinking");
    private static readonly int IsPlaying = Animator.StringToHash("IsPlaying");
    
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        navAgent.speed = walkSpeed;
    }
    
    /// <summary>
    /// Initialize the NPC with its group and leader status
    /// </summary>
    public void Initialize(NPCGroup npcGroup, bool leader)
    {
        group = npcGroup;
        isGroupLeader = leader;
        isReady = true;
    }
    
    /// <summary>
    /// Move to a specific position
    /// </summary>
    public void MoveTo(Transform destination, Action onComplete = null)
    {
        if (!isReady || navAgent == null || destination == null) return;
        
        targetPosition = destination;
        
        StartCoroutine(MoveToDestination(destination.position, onComplete));
    }
    
    /// <summary>
    /// Coroutine to move to a destination with a callback
    /// </summary>
    private IEnumerator MoveToDestination(Vector3 destination, Action onComplete)
    {
        animator?.SetBool(IsWalking, true);
        navAgent.SetDestination(destination);
        
        while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
        {
            yield return null;
        }
        
        animator?.SetBool(IsWalking, false);
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Assigns a chair to this NPC and moves them to sit on it
    /// </summary>
    public void AssignChair(Chair chair)
    {
        assignedChair = chair;
        chair.SetOccupied(true);
        
        // Move to the chair position
        StartCoroutine(SitOnChair());
    }
    
    /// <summary>
    /// Coroutine to handle the sitting process
    /// </summary>
    private IEnumerator SitOnChair()
    {
        if (assignedChair == null) yield break;
        
        // First, move to the chair
        animator?.SetBool(IsWalking, true);
        navAgent.SetDestination(assignedChair.GetSitPosition().position);
        
        while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
        {
            yield return null;
        }
        
        // Rotate to face the table
        transform.rotation = assignedChair.GetSitPosition().rotation;
        
        // Play sitting animation
        animator?.SetBool(IsWalking, false);
        animator?.SetBool(IsSitting, true);
        
        // Adjust position for sitting
        transform.position = new Vector3(
            assignedChair.GetSitPosition().position.x,
            transform.position.y - sitOffset,
            assignedChair.GetSitPosition().position.z
        );
        
        // Disable NavMeshAgent while sitting
        navAgent.enabled = false;
        isSeated = true;
        
        // Notify the group that this NPC has sat down
        group.OnNPCSatDown(this);
    }
    
    /// <summary>
    /// Make the NPC order a drink
    /// </summary>
    public void OrderDrink()
    {
        requestedDrink = drinkOptions[UnityEngine.Random.Range(0, drinkOptions.Length)];
        Debug.Log($"NPC ordered drink: {requestedDrink}");
        
        // Update the table UI
        if (assignedChair != null && assignedChair.GetTable() != null)
        {
            TableController table = assignedChair.GetTable();
            table.UpdateNPCRequest(this, requestedDrink);
        }
    }
    
    /// <summary>
    /// Make the NPC order a refreshed drink (second order)
    /// </summary>
    public void OrderDrinkRefresh()
    {
        // Pick a random drink
        string baseDrink = drinkOptions[UnityEngine.Random.Range(0, drinkOptions.Length)];
        
        // Add "Tazele:" prefix
        requestedDrink = "Tazele:" + baseDrink;
        
        Debug.Log($"NPC ordered refresh drink: {requestedDrink}");
        
        // Update the table UI
        if (assignedChair != null && assignedChair.GetTable() != null)
        {
            TableController table = assignedChair.GetTable();
            table.UpdateNPCRequest(this, requestedDrink);
        }
    }
    
    /// <summary>
    /// Sets the cashier position for payment
    /// </summary>
    public void SetCashierPosition(Transform cashier)
    {
        cashierPosition = cashier;
    }
    
    /// <summary>
    /// Sets the exit position for leaving
    /// </summary>
    public void SetExitPosition(Transform exit)
    {
        exitPosition = exit;
    }
    
    /// <summary>
    /// Update this NPC's patience level based on the group's patience
    /// </summary>
    /// <param name="groupPatiencePercentage">Normalized percentage (0-1) of group patience remaining</param>
    public void UpdatePatienceFromGroup(float groupPatiencePercentage)
    {
        patiencePercentage = groupPatiencePercentage;
    }
    
    /// <summary>
    /// Drink the beverage served to this NPC
    /// </summary>
    public IEnumerator DrinkBeverage()
    {
        if (!hasDrink) yield break;
        
        Debug.Log($"[NPC] {gameObject.name} içecek içmeye başladı");
        animator?.SetBool(IsDrinking, true);
        
        // Generate a random drinking duration between minDrinkDuration and maxDrinkDuration
        float drinkingTime = UnityEngine.Random.Range(minDrinkDuration, maxDrinkDuration);
        
        Debug.Log($"[NPC] {gameObject.name} {drinkingTime:F1} saniye içecek içecek");
        yield return new WaitForSeconds(drinkingTime); // Random drinking animation time
        
        Debug.Log($"[NPC] {gameObject.name} içecek içmeyi bitirdi");
        animator?.SetBool(IsDrinking, false);
        hasDrink = false; // Cup is now empty
        
        // Make the cup dirty
        if (currentCup != null)
        {
            Debug.Log($"[NPC] {gameObject.name} bardağı kirli olarak işaretliyor");
            // First mark the cup as dirty
            DirtyStatus dirtyStatus = currentCup.GetComponent<DirtyStatus>();
            bool wasDirty = false;
            
            if (dirtyStatus != null)
            {
                wasDirty = dirtyStatus.isDirty;
                dirtyStatus.isDirty = true;
            }
            
            // Empty the cup contents - NPC has drunk it
            Tea_Cup teaCup = currentCup.GetComponent<Tea_Cup>();
            if (teaCup != null)
            {
                // We'll manually handle the visual changes to keep the dirty state
                teaCup.isFillOraletorCoffee = false;
                teaCup.isFillTea = false;
                teaCup.isFullTea = false;
                teaCup.isFullOraletorCoffee = false;
                teaCup.inCup = "Empty";
                
                // Use EmptyCup to update the visuals
                teaCup.EmptyCup();
            }
            
            // Make sure the dirty status is maintained after EmptyCup
            if (dirtyStatus != null && !dirtyStatus.isDirty && wasDirty)
            {
                dirtyStatus.isDirty = true;
            }
            
            // Change cup layer back to interactable so player can pick it up
            currentCup.layer = LayerMask.NameToLayer("Interactable");
            
            // Make sure the cup stays on the table but can be picked up
            Rigidbody rb = currentCup.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Keep kinematic to prevent falling but enable collision detection
                rb.isKinematic = true;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }
            
            // Make sure the cup stays at its position on the table
            // We do not need to change its parent as it was already set to the table
        }
        
        // Notify group that drinking is complete
        Debug.Log($"[NPC] {gameObject.name} içmeyi bitirdi, gruba bildirim yapılıyor");
        if (group != null)
        {
            group.OnNPCFinishedDrinking(this);
        }
        else
        {
            Debug.LogError($"[NPC] {gameObject.name}: NPC'nin grup referansı yok! İçme bildirimi yapılamıyor.");
        }
    }
    
    /// <summary>
    /// Makes the NPC start playing a game
    /// </summary>
    public void StartPlaying()
    {
        if (!isSeated || !hasGameBox) return;
        
        animator?.SetBool(IsPlaying, true);
    }
    
    /// <summary>
    /// Makes the NPC stop playing a game
    /// </summary>
    public void StopPlaying()
    {
        animator?.SetBool(IsPlaying, false);
    }
    
    /// <summary>
    /// Makes the NPC go to the exit and leave
    /// </summary>
    public void ExitShop()
    {
        if (isExiting) return;
        
        Debug.Log($"[NPC] {gameObject.name} için ExitShop çağrıldı");
        isExiting = true;
        
        if (isSeated)
        {
            Debug.Log($"[NPC] {gameObject.name} oturuyordu, önce kalkacak");
            // First, stand up from the chair
            StartCoroutine(GetUpProcess(() => {
                // After getting up, decide where to go
                if (isGroupLeader && cashierPosition != null && !_isPaying)
                {
                    Debug.Log($"[NPC] {gameObject.name} (lider) kasaya gidiyor");
                    ShowMessageAboveNPC("Kasaya gidiyor");
                    MoveTo(cashierPosition, OnArrivedAtCashier);
                }
                else if (exitPosition != null)
                {
                    Debug.Log($"[NPC] {gameObject.name} çıkışa gidiyor");
                    ShowMessageAboveNPC("Dükkandan çıkıyor");
                    MoveTo(exitPosition, OnArrivedAtExit);
                }
                else
                {
                    Debug.LogError($"[NPC] {gameObject.name}: Çıkış pozisyonu bulunamadı!");
                    // Doğrudan grubu bilgilendir
                    group.OnNPCLeft(this);
                    Destroy(gameObject, 0.5f);
                }
            }));
        }
        else
        {
            Debug.Log($"[NPC] {gameObject.name} zaten ayakta, doğrudan gidebilir");
            // Already standing, decide where to go
            if (isGroupLeader && cashierPosition != null && !_isPaying)
            {
                Debug.Log($"[NPC] {gameObject.name} (lider) kasaya gidiyor");
                ShowMessageAboveNPC("Kasaya gidiyor");
                MoveTo(cashierPosition, OnArrivedAtCashier);
            }
            else if (exitPosition != null)
            {
                Debug.Log($"[NPC] {gameObject.name} çıkışa gidiyor");
                ShowMessageAboveNPC("Dükkandan çıkıyor");
                MoveTo(exitPosition, OnArrivedAtExit);
            }
            else
            {
                Debug.LogError($"[NPC] {gameObject.name}: Çıkış pozisyonu bulunamadı!");
                // Doğrudan grubu bilgilendir
                group.OnNPCLeft(this);
                Destroy(gameObject, 0.5f);
            }
        }
    }
    
    /// <summary>
    /// Exit through cashier (for group leaders who need to pay)
    /// </summary>
    public void ExitShopThroughCashier()
    {
        if (isExiting) return;
        
        Debug.Log($"[NPC] {gameObject.name} için ExitShopThroughCashier çağrıldı");
        isExiting = true;
        
        if (!isGroupLeader)
        {
            Debug.LogWarning($"[NPC] {gameObject.name}: ExitShopThroughCashier grup lideri olmayan NPC için çağrıldı");
            ExitShopDirectly(); // Fallback to direct exit
            return;
        }
        
        if (cashierPosition == null)
        {
            Debug.LogError($"[NPC] {gameObject.name}: Kasiyer pozisyonu tanımlanmamış!");
            ExitShopDirectly(); // Fallback to direct exit
            return;
        }
        
        if (isSeated)
        {
            Debug.Log($"[NPC] {gameObject.name} oturuyordu, önce kalkacak sonra kasaya gidecek");
            // First, stand up from the chair
            StartCoroutine(GetUpProcess(() => {
                Debug.Log($"[NPC] {gameObject.name} kalktı, şimdi kasaya gidiyor");
                ShowMessageAboveNPC("Kasaya gidiyor");
                MoveTo(cashierPosition, OnArrivedAtCashier);
            }));
        }
        else
        {
            // Already standing
            Debug.Log($"[NPC] {gameObject.name} zaten ayakta, doğrudan kasaya gidebilir");
            ShowMessageAboveNPC("Kasaya gidiyor");
            MoveTo(cashierPosition, OnArrivedAtCashier);
        }
    }
    
    /// <summary>
    /// Exit directly to exit point (skip cashier)
    /// </summary>
    public void ExitShopDirectly()
    {
        if (isExiting) return;
        
        Debug.Log($"[NPC] {gameObject.name} için ExitShopDirectly çağrıldı");
        isExiting = true;
        
        if (exitPosition == null)
        {
            Debug.LogError($"[NPC] {gameObject.name}: Çıkış pozisyonu tanımlanmamış!");
            // No good fallback here, just destroy the NPC
            group.OnNPCLeft(this);
            Destroy(gameObject, 0.5f);
            return;
        }
        
        if (isSeated)
        {
            Debug.Log($"[NPC] {gameObject.name} oturuyordu, önce kalkacak sonra çıkışa gidecek");
            // First, stand up from the chair
            StartCoroutine(GetUpProcess(() => {
                Debug.Log($"[NPC] {gameObject.name} kalktı, şimdi çıkışa gidiyor");
                ShowMessageAboveNPC("Dükkandan çıkıyor");
                MoveTo(exitPosition, OnArrivedAtExit);
            }));
        }
        else
        {
            // Already standing
            Debug.Log($"[NPC] {gameObject.name} zaten ayakta, doğrudan çıkışa gidebilir");
            ShowMessageAboveNPC("Dükkandan çıkıyor");
            MoveTo(exitPosition, OnArrivedAtExit);
        }
    }
    
    /// <summary>
    /// Display a message above the NPC
    /// </summary>
    private void ShowMessageAboveNPC(string message)
    {
        // Get Player reference to show message
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.ShowUIMessage(message);
        }
        
        // Also log the message for debugging
        Debug.Log($"NPC {gameObject.name}: {message}");
    }
    
    /// <summary>
    /// Called when the NPC arrives at the cashier
    /// </summary>
    private void OnArrivedAtCashier()
    {
        Debug.Log($"[NPC] {gameObject.name} kasiyere vardı");
        _isPaying = true;
        
        // Make NPC face the cashier (assuming cashier is facing -Z direction)
        // Look at the cashier (get position but ignore Y to keep NPC upright)
        if (cashierPosition != null)
        {
            Vector3 directionToCashier = cashierPosition.position - transform.position;
            directionToCashier.y = 0; // Keep NPC upright by ignoring Y component
            if (directionToCashier != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(directionToCashier);
            }
            Debug.Log($"[NPC] {gameObject.name} kasiyere döndü");
        }
        
        // Play idle animation when at cashier
        animator?.SetBool(IsWalking, false);
        animator?.SetBool(IsIdle, true);
        
        // Show message that NPC is paying
        ShowMessageAboveNPC("Ödeme yapıyor");
        
        // Start patience timer for payment - give a longer timeout for payment
        StartCoroutine(PatienceCountdown(() => {
            // Patience expired during payment, just leave
            Debug.Log($"[NPC] {gameObject.name} için ödeme süresi doldu, ödemeden çıkıyor");
            _isPaying = false;
            animator?.SetBool(IsIdle, false);
            ShowMessageAboveNPC("Ödemeden vazgeçti");
            MoveTo(exitPosition, OnArrivedAtExit);
        }, 40f)); // 40 saniye - ödemede daha uzun sabır süresi
    }
    
    /// <summary>
    /// Called when the NPC arrives at the exit
    /// </summary>
    private void OnArrivedAtExit()
    {
        Debug.Log($"[NPC] {gameObject.name} çıkış noktasına vardı");
        ShowMessageAboveNPC("Dükkandan çıktı");
        
        // Çıkış noktasında görsel bir efekt (fade-out) uygula
        StartCoroutine(FadeOutAndDestroy());
    }
    
    /// <summary>
    /// Fade out the NPC and destroy it
    /// </summary>
    private IEnumerator FadeOutAndDestroy()
    {
        // Eğer animator varsa, bir fade-out animasyonu tetiklenebilir
        if (animator != null)
        {
            animator.SetTrigger("FadeOut");
            yield return new WaitForSeconds(1f); // Animasyon süresi
        }
        else
        {
            // Animator yoksa, basit bir shader efekti veya scale azaltma
            float duration = 1.0f;
            float elapsed = 0;
            
            // Tüm renderer'ları bul
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            List<Material> materials = new List<Material>();
            List<Color> originalColors = new List<Color>();
            
            // Tüm materyallerin orijinal rengini kaydet
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.materials)
                {
                    materials.Add(mat);
                    originalColors.Add(mat.color);
                }
            }
            
            // Zamanla renklerinin alpha değerini azalt
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                
                for (int i = 0; i < materials.Count; i++)
                {
                    Color color = originalColors[i];
                    color.a = 1f - t;
                    materials[i].color = color;
                }
                
                // Boyutu da azalt
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t * 0.5f);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        // Eğer geçici bir çıkış noktası kullanıyorsak, onu temizle
        if (exitPosition != null && exitPosition.name.StartsWith("ExitPoint_"))
        {
            Destroy(exitPosition.gameObject);
        }
        
        // Notify the group
        group.OnNPCLeft(this);
        
        // Destroy NPC
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Interface method for player interaction
    /// </summary>
    public void interact()
    {
        // Handle player interactions with this NPC
        if (_isPaying)
        {
            // Process payment
            ProcessPayment();
        }
        else if (isSeated && !hasDrink)
        {
            // Check if player is giving a drink
            CheckDrinkServing();
        }
    }
    
    /// <summary>
    /// Process payment when player interacts with NPC at cashier
    /// </summary>
    private void ProcessPayment()
    {
        Debug.Log($"[NPC] {gameObject.name}: Ödeme işlemi tamamlandı!");
        ShowMessageAboveNPC("Ödeme tamamlandı");
        _isPaying = false;
        
        // Reset idle animation
        animator?.SetBool(IsIdle, false);
        
        // Go to exit
        if (exitPosition != null)
        {
            ShowMessageAboveNPC("Dükkandan çıkıyor");
            MoveTo(exitPosition, OnArrivedAtExit);
        }
        else
        {
            Debug.LogError($"[NPC] {gameObject.name}: Çıkış pozisyonu tanımlanmamış! NPC çıkamıyor.");
            // Fallback - doğrudan grup bilgilendirme
            group.OnNPCLeft(this);
            Destroy(gameObject, 0.5f);
        }
    }
    
    /// <summary>
    /// Check if player is giving the correct drink
    /// </summary>
    private void CheckDrinkServing()
    {
        Debug.Log("[NPC] CheckDrinkServing() çağrıldı - İçecek kontrolü başlıyor");
        
        // Get reference to the player
        Player player = FindObjectOfType<Player>();
        
        if (player == null)
        {
            Debug.LogError("[NPC] Player referansı bulunamadı!");
            return;
        }
        
        if (player.inHandItem == null)
        {
            Debug.Log("[NPC] Oyuncunun elinde bir şey yok");
            return;
        }
        
        Debug.Log($"[NPC] Oyuncunun elinde: {player.inHandItem.name}");
        
        Tea_Cup teaCup = player.inHandItem.GetComponent<Tea_Cup>();
        
        if (teaCup == null)
        {
            Debug.Log("[NPC] Eldeki nesne bir Tea_Cup değil");
            return;
        }
        
        // Get the actual drink type by removing "Tazele:" prefix if present
        string actualRequestedDrink = requestedDrink;
        if (requestedDrink.StartsWith("Tazele:"))
        {
            actualRequestedDrink = requestedDrink.Substring(7); // Remove "Tazele:" prefix
        }
        
        Debug.Log($"[NPC] Bardaktaki içerik: {teaCup.inCup}, NPC'nin istediği: {requestedDrink} (Saf içerik: {actualRequestedDrink})");
        
        if (teaCup.inCup == actualRequestedDrink)
        {
            // Correct drink served
            Debug.Log("[NPC] DOĞRU İÇECEK SERVISI BAŞARILI!");
            hasDrink = true;
            player.ShowUIMessage("İçecek başarıyla servis edildi!");
            
            // Müşteri içeceği aldığında UI'dan isteği kaldırmak için requestedDrink'i temizle
            string servedDrink = requestedDrink;
            requestedDrink = "";
            
            // Atanmış masadaki içecek talebini güncelle
            if (assignedChair != null && assignedChair.GetTable() != null)
            {
                TableController table = assignedChair.GetTable();
                table.UpdateNPCRequest(this, "");
            }
            
            // Place the cup on the table
            PlaceCupOnTable(player.inHandItem);
            
            // Start drinking after a short delay
            StartCoroutine(DrinkBeverage());
            
            // Notify group
            group.OnNPCServedDrink(this, servedDrink);
        }
        else
        {
            // Wrong drink
            Debug.Log($"[NPC] YANLIŞ İÇECEK! İstenilen: {requestedDrink}, Verilen: {teaCup.inCup}");
            player.ShowUIMessage($"Yanlış içecek - müşteri {requestedDrink} istiyor");
        }
    }
    
    /// <summary>
    /// Place the cup on the table in front of the NPC
    /// </summary>
    private void PlaceCupOnTable(GameObject cup)
    {
        if (cup == null || assignedChair == null) return;
        
        // Get the cup position for this chair
        Transform cupPosition = assignedChair.GetCupPosition();
        
        if (cupPosition != null)
        {
            // Get table reference
            TableController table = assignedChair.GetTable();
            
            // Set the cup as a child of the table
            if (table != null)
            {
                cup.transform.SetParent(table.transform);
            }
            else
            {
                // If table reference is not available, make it a child of the chair
                cup.transform.SetParent(assignedChair.transform);
            }
            
            // Place the cup at the specific cup position for this chair
            cup.transform.position = cupPosition.position;
            cup.transform.rotation = cupPosition.rotation;
            
            // Store the cup reference for later use
            currentCup = cup;
            
            // Disable physics and make it non-interactable during NPC use
            Rigidbody rb = cup.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
            
            // Make it non-interactable until NPC leaves
            cup.layer = LayerMask.NameToLayer("Default");
            
            // Get player reference and clear their hand
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.inHandItem = null;
                player.SetPickedStatus(false);
            }
        }
        else
        {
            Debug.LogWarning("Cup position not found for chair - using fallback table position");
            
            // Fallback to the table position if cup position is not available
            Transform tablePosition = assignedChair.GetTablePosition();
            
            if (tablePosition != null)
            {
                // Get table reference
                TableController table = assignedChair.GetTable();
                
                // Set the cup as a child of the table
                if (table != null)
                {
                    cup.transform.SetParent(table.transform);
                }
                else
                {
                    // If table reference is not available, make it a child of the chair
                    cup.transform.SetParent(assignedChair.transform);
                }
                
                cup.transform.position = tablePosition.position;
                cup.transform.rotation = tablePosition.rotation;
                
                // Store the cup reference for later use
                currentCup = cup;
                
                // Disable physics and make it non-interactable during NPC use
                Rigidbody rb = cup.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                
                // Make it non-interactable until NPC leaves
                cup.layer = LayerMask.NameToLayer("Default");
                
                // Get player reference and clear their hand
                Player player = FindObjectOfType<Player>();
                if (player != null)
                {
                    player.inHandItem = null;
                    player.SetPickedStatus(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the requested drink for this NPC
    /// </summary>
    public string GetRequestedDrink()
    {
        Debug.Log($"NPC requested drink: {requestedDrink}");
        return requestedDrink;
    }
    
    /// <summary>
    /// Get the NPC's patience as a string (low, medium, high)
    /// </summary>
    public string GetPatienceLevel()
    {
        string level;
        if (patiencePercentage < 0.33f)
            level = "az";
        else if (patiencePercentage < 0.66f)
            level = "orta";
        else
            level = "fazla";
        
        Debug.Log($"NPC patience: {level} ({patiencePercentage:P0})");
        return level;
    }
    
    /// <summary>
    /// Returns whether this NPC has a drink
    /// </summary>
    public bool HasDrink()
    {
        return hasDrink;
    }
    
    /// <summary>
    /// Sets whether this NPC has a game box
    /// </summary>
    public void SetHasGameBox(bool has)
    {
        hasGameBox = has;
    }
    
    /// <summary>
    /// Returns whether this NPC is the group leader
    /// </summary>
    public bool IsGroupLeader()
    {
        return isGroupLeader;
    }
    
    /// <summary>
    /// Coroutine to handle patience countdown
    /// </summary>
    private IEnumerator PatienceCountdown(Action onExpired, float waitTime = 20f)
    {
        isWaiting = true;
        
        // Belirtilen süre kadar bekle - varsayılan 20 saniye, ama ödemede daha uzun
        yield return new WaitForSeconds(waitTime);
        
        isWaiting = false;
        onExpired?.Invoke();
    }
    
    /// <summary>
    /// Handles getting up from the chair
    /// </summary>
    public void GetUpFromChair(System.Action onComplete = null)
    {
        if (!isSeated) 
        {
            onComplete?.Invoke();
            return;
        }
        
        StartCoroutine(GetUpProcess(onComplete));
    }
    
    /// <summary>
    /// Coroutine to handle getting up process
    /// </summary>
    private IEnumerator GetUpProcess(System.Action onComplete = null)
    {
        ShowMessageAboveNPC("Kalkıyor");
        
        // Play getting up animation
        animator?.SetBool(IsSitting, false);
        
        yield return new WaitForSeconds(1f); // Animation time
        
        // Re-enable NavMeshAgent
        navAgent.enabled = true;
        isSeated = false;
        
        // Adjust position back
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + sitOffset,
            transform.position.z
        );
        
        // Free the chair
        if (assignedChair != null)
        {
            assignedChair.SetOccupied(false);
            assignedChair = null;
        }
        
        group.OnNPCGotUp(this);
        onComplete?.Invoke();
    }
} 