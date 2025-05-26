using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour, IInteractable
{
    private Coroutine currentMovementCoroutine;
    private Coroutine currentDrinkingCoroutine;
    private Coroutine currentSittingCoroutine;
    private Coroutine currentGetUpCoroutine;
    private Coroutine currentPatienceCoroutine;

    [Header("NPC Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sitOffset = 0.5f; // How much to offset when sitting
    [SerializeField] private Animator animator;

    [Header("Drinking Settings")]
    [SerializeField] private float minDrinkDuration = 3f; // Minimum time to drink (seconds)
    [SerializeField] private float maxDrinkDuration = 8f; // Maximum time to drink (seconds)

    [Header("Order Settings")]
    [SerializeField]
    private string[] drinkOptions = new string[] { "Light_Tea", "Rabbit_Blood_Tea", "Brewed_Tea", "Coffee_Drink", "Banana_Oralet", "Kiwi_Oralet", "Orange_Oralet", "Strawberry_Oralet" };

    [Header("Drink Prices")]
    [SerializeField]
    private float[] drinkPrices = new float[] { 15f, 25f, 20f, 30f, 20f, 20f, 20f, 20f }; // Her içeceğin fiyatı
    private Dictionary<string, float> drinkPriceMap = new Dictionary<string, float>();
    private float totalBill = 0f; // Grup için toplam hesap
    // İçecek isimlerinin Türkçe karşılıkları
    private Dictionary<string, string> drinkNameTranslations = new Dictionary<string, string>()
    {
        { "Light_Tea", "Açık Çay" },
        { "Rabbit_Blood_Tea", "Tavşan Kanı Çay" },
        { "Brewed_Tea", "Demli Çay" },
        { "Coffee_Drink", "Kahve" },
        { "Banana_Oralet", "Muzlu Oralet" },
        { "Kiwi_Oralet", "Kivi Oralet" },
        { "Orange_Oralet", "Portakallı Oralet" },
        { "Strawberry_Oralet", "Çilekli Oralet" }
    };

    // State fields
    private NavMeshAgent navAgent;
    private NPCGroup group;
    [SerializeField] private bool isGroupLeader = false;
    private bool isWaiting = false;
    private float patiencePercentage = 1.0f;

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
    [SerializeField] private float adisyonFee;
    [SerializeField] private Chair assignedChair;
    [SerializeField] public TableController table;

    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private WearManager wearManager;
    Adisyon tableAdisyon;

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
        moneyManager = FindObjectOfType<MoneyManager>();
        wearManager = FindObjectOfType<WearManager>();
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

        // İçecek fiyat haritasını oluştur
        for (int i = 0; i < drinkOptions.Length; i++)
        {
            if (i < drinkPrices.Length)
            {
                drinkPriceMap[drinkOptions[i]] = drinkPrices[i];
            }
        }
    }

    public void Initialize(NPCGroup npcGroup, bool leader)
    {
        group = npcGroup;
        isGroupLeader = leader;
        isReady = true;
    }

    public void MoveTo(Transform destination, Action onComplete = null)
    {
        if (!isReady || navAgent == null || destination == null) return;

        // Önceki movement coroutine'ini durdur
        if (currentMovementCoroutine != null)
        {
            StopCoroutine(currentMovementCoroutine);
        }

        targetPosition = destination;
        currentMovementCoroutine = StartCoroutine(MoveToDestination(destination.position, onComplete));
    }


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
    public void AssignChair(Chair chair)
    {
        assignedChair = chair;
        chair.SetOccupied(true);

        StartCoroutine(SitOnChair());// Move to the chair position
    }

    private IEnumerator SitOnChair()
    {
        // Önceki sitting coroutine'ini durdur
        if (currentSittingCoroutine != null)
        {
            StopCoroutine(currentSittingCoroutine);
        }
        yield return null;

        if (assignedChair == null) yield break;
        animator?.SetBool(IsWalking, true);// First, move to the chair
        navAgent.SetDestination(assignedChair.GetSitPosition().position);

        while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
        {
            yield return null;
        }
        transform.rotation = assignedChair.GetSitPosition().rotation;// Rotate to face the table

        animator?.SetBool(IsWalking, false);// Play sitting animation
        animator?.SetBool(IsSitting, true);

        transform.position = new Vector3(// Adjust position for sitting
            assignedChair.GetSitPosition().position.x,
            transform.position.y - sitOffset,
            assignedChair.GetSitPosition().position.z
        );

        navAgent.enabled = false;// Disable NavMeshAgent while sitting
        isSeated = true;

        group.OnNPCSatDown(this);// Notify the group that this NPC has sat down
    }

    public void OrderDrink()
    {
        requestedDrink = drinkOptions[UnityEngine.Random.Range(0, drinkOptions.Length)];
        Debug.Log($"NPC ordered drink: {requestedDrink}");

        // Update the table UI
        if (assignedChair != null && assignedChair.GetTable() != null)
        {
            table = assignedChair.GetTable();
            Debug.Log($"Table found for NPC: {table.tableName}");
            table.UpdateNPCRequest(this, requestedDrink);
        }
    }

    public void OrderDrinkRefresh()
    {
        string baseDrink = drinkOptions[UnityEngine.Random.Range(0, drinkOptions.Length)];// Pick a random drink
        requestedDrink = "Tazele:" + baseDrink;// Add "Tazele:" prefix
        Debug.Log($"NPC ordered refresh drink: {requestedDrink}");

        if (assignedChair != null && assignedChair.GetTable() != null)// Update the table UI
        {
            table = assignedChair.GetTable();
            table.UpdateNPCRequest(this, requestedDrink);
        }

        // Tazeleme için de ücret ekle (aynı içeceğin ücreti)
        if (drinkPriceMap.TryGetValue(baseDrink, out float price))
        {
            if (group != null)
            {
                group.AddToBill(price);
            }
            else
            {
                totalBill += price;
            }
            Debug.Log($"NPC ordered refresh {baseDrink} for {price} TL");
        }
    }
    public void SetCashierPosition(Transform cashier)
    {
        cashierPosition = cashier;
    }

    public void SetExitPosition(Transform exit)
    {
        exitPosition = exit;
    }

    // Update this NPC's patience level based on the group's patience
    public void UpdatePatienceFromGroup(float groupPatiencePercentage)
    {
        patiencePercentage = groupPatiencePercentage;
    }
    // Drink the beverage served to this NPC
    public IEnumerator DrinkBeverage()
    {
        // Önceki drinking coroutine'ini durdur
        if (currentDrinkingCoroutine != null)
        {
            StopCoroutine(currentDrinkingCoroutine);
        }

        if (!hasDrink) yield break;

        animator?.SetBool(IsDrinking, true);

        // Generate a random drinking duration between minDrinkDuration and maxDrinkDuration
        float drinkingTime = UnityEngine.Random.Range(minDrinkDuration, maxDrinkDuration);

        yield return new WaitForSeconds(drinkingTime); // Random drinking animation time

        animator?.SetBool(IsDrinking, false);
        hasDrink = false; // Cup is now empty

        // Make the cup dirty
        if (currentCup != null)
        {
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
                teaCup.isFillOraletorCoffee = false;
                teaCup.isFillTea = false;
                teaCup.isFullTea = false;
                teaCup.isFullOraletorCoffee = false;
                teaCup.inCup = "Empty";

                teaCup.EmptyCup();
                
                // Call ChangeMeshOfDirtyTea to update the dirty visual overlay
                teaCup.ChangeMeshOfDirtyTea();
            }

            // Make sure the dirty status is maintained after EmptyCup
            if (dirtyStatus != null && !dirtyStatus.isDirty && wasDirty)
            {
                dirtyStatus.isDirty = true;
                
                // Make sure visual is updated if status was reset
                if (teaCup != null)
                {
                    teaCup.ChangeMeshOfDirtyTea();
                }
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
        if (group != null)
        {
            group.OnNPCFinishedDrinking(this);
        }
        yield return null;
    }
    // Makes the NPC start playing a game
    public void StartPlaying()
    {
        if (!isSeated || !hasGameBox) return;

        animator?.SetBool(IsPlaying, true);
    }

    // Makes the NPC stop playing a game
    public void StopPlaying()
    {
        animator?.SetBool(IsPlaying, false);
    }
    // Makes the NPC go to the exit and leave
    public void ExitShop()
    {
        if (isExiting) return;

        Debug.Log($"[NPC] {gameObject.name} için ExitShop çağrıldı");
        isExiting = true;

        if (isSeated)
        {
            // First, stand up from the chair
            StartCoroutine(GetUpProcess(() =>
            {
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
    // Exit through cashier (for group leaders who need to pay)
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
            StartCoroutine(GetUpProcess(() =>
            {
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
    // Exit directly to exit point (skip cashier)
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
            StartCoroutine(GetUpProcess(() =>
            {
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
    
    // Display a message above the NPC
    private void ShowMessageAboveNPC(string message)
    {
        // Get Player reference to show message
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.ShowUIMessage(message, false); // Not a contextual message - will auto-hide
        }
    }
    
    // Called when the NPC arrives at the cashier
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

        // Toplam hesabı göster
        float finalBill = group != null ? group.GetTotalBill() : totalBill;
        ShowMessageAboveNPC($"{finalBill} TL ödeme yapıyor");

        // Start patience timer for payment - give a longer timeout for payment
        StartCoroutine(PatienceCountdown(() =>
        {
            // Patience expired during payment, just leave
            Debug.Log($"[NPC] {gameObject.name} için ödeme süresi doldu, ödemeden çıkıyor");
            _isPaying = false;
            animator?.SetBool(IsIdle, false);
            ShowMessageAboveNPC("Ödemeden vazgeçti");
            MoveTo(exitPosition, OnArrivedAtExit);
        }, 40f)); // 40 saniye - ödemede daha uzun sabır süresi
    }
    // Called when the NPC arrives at the exit
    private void OnArrivedAtExit()
    {
        Debug.Log($"[NPC] {gameObject.name} çıkış noktasına vardı");
        ShowMessageAboveNPC("Dükkandan çıktı");

        // Çıkış noktasında görsel bir efekt (fade-out) uygula
        StartCoroutine(FadeOutAndDestroy());
    }
    // Fade out the NPC and destroy it
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
    // Interface method for player interaction
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
    // Process payment when player interacts with NPC at cashier
    private void ProcessPayment()
    {
        /*if (adisyonFee <= 0)
        {
            _isPaying = false;
            Debug.Log($"[NPC] {gameObject.name}: Ödeme yapılacak bir hesap yok, direkt çıkışa gidiyor!");
            ShowMessageAboveNPC("Hesap yok, çıkıyor");

            // Direkt çıkışa git
            if (exitPosition != null)
            {
                ShowMessageAboveNPC("Dükkandan çıkıyor");
                MoveTo(exitPosition, OnArrivedAtExit);
            }
            else
            {
                Debug.LogError($"[NPC] {gameObject.name}: Çıkış pozisyonu tanımlanmamış! NPC çıkamıyor.");
                group.OnNPCLeft(this);
                Destroy(gameObject, 0.5f);
            }
            return;
        }*/

        float finalBill = group != null ? group.GetTotalBill() : totalBill;
        Debug.Log($"[NPC] {gameObject.name}: {finalBill} TL ödeme işlemi tamamlandı!");
        ShowMessageAboveNPC($"{finalBill} TL ödeme tamamlandı");
        _isPaying = false;
        // Reset idle animation
        animator?.SetBool(IsIdle, false);

        // Calculate the percentage difference between adisyonFee and finalBill
        float percentageDifference = ((adisyonFee - finalBill) / finalBill) * 100f;
        float randomChance = UnityEngine.Random.Range(0f, 100f);
         Debug.Log($"[NPC] Adisyon: {adisyonFee} TL, Final Bill: {finalBill} TL, Fark: %{percentageDifference}");
        if (adisyonFee <= finalBill)
        {
            moneyManager.AddMoney(finalBill);
            wearManager.AddWear(0);
        }
        if (adisyonFee > finalBill)
        {
            if (percentageDifference > 0 && percentageDifference <= 20)
            {
                if (randomChance <= 80)
                {
                    moneyManager.AddMoney(adisyonFee);
                    wearManager.AddWear(2);
                }
                else
                {
                    moneyManager.AddMoney(finalBill);
                    wearManager.AddWear(4);
                }
            }
            else if (percentageDifference > 20 && percentageDifference <= 40)
            {
                if (randomChance <= 60)
                {
                    moneyManager.AddMoney(adisyonFee);
                    wearManager.AddWear(4);
                }
                else
                {
                    moneyManager.AddMoney(finalBill);
                    wearManager.AddWear(8);
                }
            }
            else if (percentageDifference > 40 && percentageDifference <= 60)
            {
                if (randomChance <= 40)
                {
                    moneyManager.AddMoney(adisyonFee);
                    wearManager.AddWear(6);
                }
                else
                {
                    moneyManager.AddMoney(finalBill);
                    wearManager.AddWear(12);
                }
            }
            else if (percentageDifference > 60 && percentageDifference <= 80)
            {
                if (randomChance <= 20)
                {
                    moneyManager.AddMoney(adisyonFee);
                    wearManager.AddWear(8);
                }
                else
                {
                    moneyManager.AddMoney(finalBill);
                    wearManager.AddWear(16);
                }
            }
            else if (percentageDifference > 80 && percentageDifference <= 100)
            {
                if (randomChance <= 10)
                {
                    moneyManager.AddMoney(adisyonFee);
                    wearManager.AddWear(10);
                }
                else
                {
                    moneyManager.AddMoney(finalBill);
                    wearManager.AddWear(20);
                }
            }
            else // percentageDifference > 100
            {
                if (randomChance <= 0) // This will never happen, but included for completeness
                {
                    moneyManager.AddMoney(adisyonFee);
                    wearManager.AddWear(12);
                }
                else
                {
                    moneyManager.AddMoney(finalBill);
                    wearManager.AddWear(25);
                }
            }
        }
        // Hesabı sıfırla
        if (group != null)
        {
            group.ResetBill();
        }
        tableAdisyon.ResetAdisyon();
        totalBill = 0f;

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
    // Check if player is giving the correct drink
    private void CheckDrinkServing()
    {
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

        // Check if player is holding a cup directly
        Tea_Cup teaCup = player.inHandItem.GetComponent<Tea_Cup>();

        // Check if player is holding a tray
        bool isTray = player.inHandItem.CompareTag("Tray");
        GameObject cupToServe = null;
        GameObject originalTray = null;

        // Get the actual drink type by removing "Tazele:" prefix if present
        string actualRequestedDrink = requestedDrink;
        if (requestedDrink.StartsWith("Tazele:"))
        {
            actualRequestedDrink = requestedDrink.Substring(7); // Remove "Tazele:" prefix
        }

        if (teaCup == null && isTray)
        {
            Debug.Log("[NPC] Oyuncunun elinde tepsi var, tepsi üzerindeki içecekler kontrol ediliyor");
            originalTray = player.inHandItem;

            // Check if any of the tray's children is a cup with the requested drink
            for (int i = 0; i < player.inHandItem.transform.childCount; i++)
            {
                Transform child = player.inHandItem.transform.GetChild(i);
                Tea_Cup childTeaCup = child.GetComponent<Tea_Cup>();
                bool isDirty = child.GetComponent<DirtyStatus>().isDirty;

                if (childTeaCup != null && childTeaCup.inCup == actualRequestedDrink && !isDirty)
                {
                    Debug.Log($"[NPC] Tepsideki {childTeaCup.inCup} içecek bulundu!");
                    teaCup = childTeaCup;
                    cupToServe = child.gameObject;
                    break;
                }
            }

            if (teaCup == null)
            {
                Debug.Log("[NPC] Tepsideki içeceklerden hiçbiri NPC'nin isteğine uygun değil");
                return;
            }
        }
        else if (teaCup == null)
        {
            Debug.Log("[NPC] Eldeki nesne bir Tea_Cup veya tepsi değil");
            return;
        }
        else
        {
            cupToServe = player.inHandItem;
        }

        Debug.Log($"[NPC] Bardaktaki içerik: {teaCup.inCup}, NPC'nin istediği: {requestedDrink} (Saf içerik: {actualRequestedDrink})");

        if (teaCup.inCup == actualRequestedDrink)
        {
            // Correct drink served
            Debug.Log("[NPC] DOĞRU İÇECEK SERVISI BAŞARILI!");
            hasDrink = true;
            ShowMessageAboveNPC("İçecek başarıyla servis edildi!");


            // İçecek ücretini ekle
            float drinkPrice = GetDrinkPrice(actualRequestedDrink);
            if (group != null)
            {
                group.AddToBill(drinkPrice);
            }
            else
            {
                AddToBill(drinkPrice);
            }
            Debug.Log($"[NPC] {gameObject.name}: {drinkPrice} TL içecek ücreti eklendi");


            // Müşteri içeceği aldığında UI'dan isteği kaldırmak için requestedDrink'i temizle
            string servedDrink = requestedDrink;
            requestedDrink = "";

            // Atanmış masadaki içecek talebini güncelle
            if (assignedChair != null && assignedChair.GetTable() != null)
            {
                table = assignedChair.GetTable();
                table.UpdateNPCRequest(this, "");
            }

            // If serving from tray, detach the cup from the tray first
            if (isTray && cupToServe != null && originalTray != null)
            {
                Debug.Log("[NPC] Bardak tepsiden alınıyor...");
                // Store the tray in player's hand for later reassignment
                cupToServe.transform.SetParent(null);

                // Make sure the player still has the tray in hand
                player.inHandItem = originalTray;
                Debug.Log($"[NPC] Oyuncunun elindeki tepsi korunuyor: {player.inHandItem.name}");
            }

            // Place the cup on the table
            PlaceCupOnTable(cupToServe, isTray, originalTray, player);

            // Start drinking after a short delay
            StartCoroutine(DrinkBeverage());

            // Notify group
            group.OnNPCServedDrink(this, servedDrink);
        }
        else
        {
            // Wrong drink
            Debug.Log($"[NPC] YANLIŞ İÇECEK! İstenilen: {requestedDrink}, Verilen: {teaCup.inCup}");
            ShowMessageAboveNPC($"Yanlış içecek - müşteri {requestedDrink} istiyor");
        }
    }
    // Place the cup on the table in front of the NPC
    private void PlaceCupOnTable(GameObject cup, bool isFromTray = false, GameObject tray = null, Player player = null)
    {
        if (cup == null || assignedChair == null) return;

        // Get the cup position for this chair
        Transform cupPosition = assignedChair.GetCupPosition();

        // If player is null, try to get the reference
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        Debug.Log($"[NPC] PlaceCupOnTable - isFromTray: {isFromTray}, cup: {cup.name}");

        if (player != null && isFromTray)
        {
            Debug.Log($"[NPC] Tepsi üzerinden bardak servis ediliyor. Tray: {(tray != null ? tray.name : "null")}");
        }

        if (cupPosition != null)
        {
            // Get table reference
            table = assignedChair.GetTable();

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

            // Only clear player's hand if the cup was directly held (not from tray)
            if (player != null && isFromTray)
            {
                Debug.Log("[NPC] Tepsi üzerinden servis edildi - tepsi oyuncunun elinde kalıyor");

                // Make sure player still has the tray
                if (tray != null)
                {
                    player.inHandItem = tray;
                }
            }
            else if (player != null && !isFromTray)
            {
                Debug.Log("[NPC] Doğrudan servis edildi - oyuncunun eli boşaltılıyor");
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
                table = assignedChair.GetTable();

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

                if (player != null && isFromTray)
                {
                    Debug.Log("[NPC] Tepsi üzerinden servis edildi (fallback) - tepsi oyuncunun elinde kalıyor");

                    // Make sure player still has the tray
                    if (tray != null)
                    {
                        player.inHandItem = tray;
                    }
                }
                // Only clear player's hand if the cup was directly held (not from tray)
                else if (player != null && !isFromTray)
                {
                    Debug.Log("[NPC] Doğrudan servis edildi (fallback) - oyuncunun eli boşaltılıyor");
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
        string displayName = requestedDrink;

        // Eğer "Tazele:" prefix'i varsa, onu kaldır ve Türkçe ismi bul
        if (displayName.StartsWith("Tazele:"))
        {
            string baseDrink = displayName.Substring(7);
            if (drinkNameTranslations.TryGetValue(baseDrink, out string translatedName))
            {
                displayName = "Tazele: " + translatedName;
            }
        }
        else if (drinkNameTranslations.TryGetValue(displayName, out string translatedName))
        {
            displayName = translatedName;
        }

        Debug.Log($"NPC requested drink: {displayName}");
        return displayName;
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
    // Returns whether this NPC has a drink
    public bool HasDrink()
    {
        return hasDrink;
    }
    // Sets whether this NPC has a game box
    public void SetHasGameBox(bool has)
    {
        hasGameBox = has;
    }
    // Returns whether this NPC is the group leader
    public bool IsGroupLeader()
    {
        return isGroupLeader;
    }
    // Coroutine to handle patience countdown
    private IEnumerator PatienceCountdown(Action onExpired, float waitTime = 20f)
    {
        // Önceki patience coroutine'ini durdur
        if (currentPatienceCoroutine != null)
        {
            StopCoroutine(currentPatienceCoroutine);
        }

        isWaiting = true;

        // Belirtilen süre kadar bekle - varsayılan 20 saniye, ama ödemede daha uzun
        yield return new WaitForSeconds(waitTime);

        isWaiting = false;
        onExpired?.Invoke();
        yield return null;
    }
    // Handles getting up from the chair
    public void GetUpFromChair(System.Action onComplete = null)
    {

        if (!isSeated)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(GetUpProcess(onComplete));
    }
    //Coroutine to handle getting up process
    private IEnumerator GetUpProcess(System.Action onComplete = null)
    {
        // Önceki get up coroutine'ini durdur
        if (currentGetUpCoroutine != null)
        {
            StopCoroutine(currentGetUpCoroutine);
        }

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
        yield return null;
    }

    // Get drink price by name
    public float GetDrinkPrice(string drinkName)
    {
        // "Tazele:" prefix'ini kaldır
        if (drinkName.StartsWith("Tazele:"))
        {
            drinkName = drinkName.Substring(7);
        }

        if (drinkPriceMap.TryGetValue(drinkName, out float price))
        {
            return price;
        }
        return 0f; // Fiyat bulunamazsa
    }

    // Add to the bill
    public void AddToBill(float amount)
    {
        totalBill += amount;
        Debug.Log($"[NPC] {gameObject.name}: Hesaba {amount} TL eklendi, toplam: {totalBill} TL");
    }

    // Get total bill
    public float GetTotalBill()
    {
        return totalBill;
    }

    // Reset bill
    public void ResetBill()
    {

    }

    public void UpdateAdisyon()
    {
        // Önce sandalye kontrol
        if (assignedChair == null)
        {
            Debug.LogError("[AdisyonÜcreti] NPC henüz bir sandalyeye atanmamış!");
            return;
        }

        // Önce NPC'nin bağlı olduğu masayı bul
        table = assignedChair.GetComponentInParent<TableController>();
        if (table == null)
        {
            Debug.LogError("[AdisyonÜcreti] Masa bulunamadı!");
            return;
        }

        // Masanın adisyon scriptini bul
        tableAdisyon = table.GetComponentInChildren<Adisyon>();
        if (tableAdisyon == null)
        {
            Debug.LogError("[AdisyonÜcreti] Masada Adisyon scripti bulunamadı! Masa: " + table.tableName);
            return;
        }

        adisyonFee = tableAdisyon.GetTotalPrice();
        Debug.Log($"[AdisyonÜcreti] {gameObject.name} için adisyon güncellendi - Tutar: {adisyonFee} TL");
    }
    private void OnDestroy()
    {
        // Tüm coroutine'leri durdur
        if (currentMovementCoroutine != null) StopCoroutine(currentMovementCoroutine);
        if (currentDrinkingCoroutine != null) StopCoroutine(currentDrinkingCoroutine);
        if (currentSittingCoroutine != null) StopCoroutine(currentSittingCoroutine);
        if (currentGetUpCoroutine != null) StopCoroutine(currentGetUpCoroutine);
        if (currentPatienceCoroutine != null) StopCoroutine(currentPatienceCoroutine);

        // NavMeshAgent'ı temizle
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
    }

}