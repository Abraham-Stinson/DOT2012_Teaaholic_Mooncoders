using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    SpecialNPC specialNPC;
    PlayerMovementScript playerMovementScript;
    public DayNightCycleController dayNightCycleControllerScript;
    public GarbageScript garbageScript;
    [Header("Player")]
    [SerializeField] public Transform playerCam;
    [SerializeField][Min(1)] public float rayCastRange = 10f;
    [SerializeField] private bool isPicked = false; //aaa
    [Header("UI")]
    [SerializeField] private GameObject handbookUI;
    [SerializeField] private GameObject f_interact;
    [SerializeField] private GameObject e_interact;
    [SerializeField] private GameObject mainInfoUI;
    [SerializeField] private TextMeshProUGUI mainInfoUIText;

    [Header("Layers")]
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] public LayerMask useableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] public LayerMask npcLayer; // Layer for NPC interaction
    [Header("First Person Hand")]
    [SerializeField] private Transform firstPersonHand;//when pick up objects it will show on this transform
    [SerializeField] public GameObject inHandItem;//what we picked up
    public RaycastHit hit;
    private GameObject lastHighlightedObject;

    [Header("Inputs")]
    [SerializeField] private InputActionReference pickAndPutInput;
    [SerializeField] private InputActionReference useInput;
    [SerializeField] private InputActionReference useHoldInput;
    [SerializeField] private InputActionReference escapeInput;
    [SerializeField] private MonoBehaviour[] playerControlScripts;


    [Header("Trash Clean")]
    [SerializeField] private float cleaningTime = 3f;
    [SerializeField] private float cleaningRadius = 2f;
    private Coroutine cleaningCoroutine;
    private bool isCleaning = false;

    [Header("Hot Water")]
    [SerializeField] private Animator hotWaterAnimator;
    [SerializeField] private ParticleSystem tapSteamParticle;
    [SerializeField] private GameObject tapSteamParticleGO;


    // Raycast cache
    private RaycastHit cachedHit;
    private bool hitCacheValid = false;
    private float lastRaycastTime = 0f;
    private const float RAYCAST_INTERVAL = 0.1f; // 10 kez/saniye yeterli

    private Coroutine currentMessageCoroutine;
    private bool isContextualMessage = false; // Flag to track if current message is contextual (based on what player is looking at)

    void Start()
    {
        // Initialize playerMovementScript to avoid null references
        playerMovementScript = GetComponent<PlayerMovementScript>();
        if (playerMovementScript == null)
        {
            playerMovementScript = FindObjectOfType<PlayerMovementScript>();
        }
        pickAndPutInput.action.performed += PickAndPut;
        useInput.action.performed += Use;
        useHoldInput.action.performed += UseHold;
        inHandItem = null;
        // Find the NPC system in the scene
    }
    void Update()
    {

        UpdateUIAndHighlight();
        Debug.DrawRay(playerCam.position, playerCam.forward * rayCastRange, Color.red);
    }

    #region USE HOLD (THRASH)
    private void UseHold(InputAction.CallbackContext context)
    {
        if (inHandItem == null)
            return;

        if (inHandItem.CompareTag("Mop") && Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.gameObject.tag == "Trash")
        {
            Destroy(hit.collider.gameObject);
            garbageScript.AddThrashToGarbage();
        }
    }
    #endregion
    #region USE INPUT
    private void Use(InputAction.CallbackContext context) // F
    {
        if (playerCam == null)
        {
            Debug.LogError("Player camera is null! Cannot perform raycast.");
            return;
        }

        if (!Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
        {
            Debug.Log("F key pressed but no object hit by raycast");
            return;
        }

        if (hit.collider.GetComponent<DeliveryBox>() != null)
        {
            Debug.Log("Delivery Box ile etkileşime geçildi");
            hit.collider.GetComponent<DeliveryBox>().interact();
            return;
        }

        GameObject target = hit.collider.gameObject;
        Debug.Log($"F key pressed, hit object: {target.name}, layer: {LayerMask.LayerToName(target.layer)}");

        if (hit.collider != null)
        {
            // NPC kontrolü - öncelikli olarak kontrol edelim
            NPC npc = hit.collider.GetComponent<NPC>();
            if (npc == null && hit.collider.transform.parent != null)
            {
                npc = hit.collider.transform.parent.GetComponent<NPC>();
            }

            if (npc != null)
            {
                Debug.Log($"NPC bulundu: {npc.name}, interact() çağrılıyor...");
                npc.interact();
                return;
            }

            // Check for Special NPC
            specialNPC = hit.collider.GetComponent<SpecialNPC>();
            if (specialNPC == null && hit.collider.transform.parent != null)
            {
                specialNPC = hit.collider.transform.parent.GetComponent<SpecialNPC>();
            }

            if (specialNPC != null)
            {
                Debug.Log($"Özel NPC bulundu: {specialNPC.GetNPCName()}, interact() çağrılıyor...");
                specialNPC.interact();
                return;
            }

            if (hit.collider.GetComponent<Adisyon>() != null)
            {
                Debug.Log($"Adisyon bulundu: {hit.collider.name}, interact() çağrılıyor...");
                if (playerMovementScript.adisyonScript == null)
                {
                    playerMovementScript.adisyonScript = hit.collider.GetComponent<Adisyon>();
                }
                hit.collider.GetComponent<Adisyon>().interact();
                return;
            }

            if (hit.collider.GetComponent<DoorTrigger>() != null)
            {
                Debug.Log($"Kapı ile etkileşime geçildi");
                hit.collider.GetComponent<DoorTrigger>().interact();
                return;
            }



            // Masa kontrolü
            TableController table = hit.collider.GetComponent<TableController>();

            // Eğer doğrudan objede TableController yoksa, parent'ını kontrol et
            if (table == null && hit.collider.transform.parent != null)
            {
                table = hit.collider.transform.parent.GetComponent<TableController>();
                Debug.Log($"Parent'tan TableController kontrol edildi: {(table != null ? "Bulundu" : "Bulunamadı")}");
            }

            // Eğer masa bulundu ve elinde bir şey varsa
            if (table != null && inHandItem != null)
            {
                Debug.Log($"Masa bulundu '{table.name}', elinde '{inHandItem.name}' var. Table.interact() çağrılıyor...");
                table.interact();
                return;
            }

            // Handbook kontrolü (toggle + cursor + timescale)
            if (hit.collider.CompareTag("Handbook"))
            {
                Debug.Log("Handbook objesiyle etkileşime girildi.");

                if (handbookUI != null)
                {
                    bool isActive = handbookUI.activeSelf;
                    handbookUI.SetActive(!isActive);
                    SoundManager.Instance.OpenBook();

                    // Cursor ve Time.timeScale kontrolü
                    if (!isActive)
                    {
                        // Açıldı
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        Time.timeScale = 0f;

                        escapeInput.action.Disable();

                        foreach (var script in playerControlScripts)
                        {
                            if (script != null)
                                script.enabled = false;
                        }


                        Debug.Log("Handbook UI açıldı, oyun duraklatıldı, imleç aktif.");
                    }
                    else
                    {
                        // Kapatıldı
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        Time.timeScale = 1f;

                        escapeInput.action.Enable();

                        foreach (var script in playerControlScripts)
                        {
                            if (script != null)
                                script.enabled = true;
                        }
                        SoundManager.Instance.CloseBook();
                        Debug.Log("Handbook UI kapatıldı, oyun devam ediyor, imleç gizlendi.");
                    }
                }
                else
                {
                    Debug.LogWarning("handbookUI referansı atanmadı!");
                }

                return;
            }


            // Normal useable layer kontrolü
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange, useableLayer) && !isPicked)
            {
                Debug.Log($"Hit object in useable layer: {hit.collider.gameObject.name}");
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"Object has IInteractable, calling interact()");
                    interactable.interact();
                    return;
                }
                else
                {
                    Debug.LogWarning($"Object {hit.collider.gameObject.name} is in useable layer but doesn't implement IInteractable!");
                }
            }

            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.CompareTag("Garbage_Bin"))
            {
                if (inHandItem != null && (inHandItem.layer == 6) && !(inHandItem.tag == "Mop" || inHandItem.tag == "Tray" || inHandItem.tag == "Kettle" || inHandItem.tag == "Garbage_Bin" || inHandItem.tag == "Garbage_Bag" || inHandItem.tag == "Iskambil" || inHandItem.tag == "Tavla" || inHandItem.tag == "Okey"))//ATILMAYACAK ESYALAR TAG TAG EKLENDI
                {
                    Destroy(inHandItem);
                    isPicked = false;
                    inHandItem = null;
                    garbageScript.AddThrashToGarbage();
                    Debug.Log("Eşya çöpe atıldı ve destroy edildi");
                }
                else
                {
                    Debug.Log("Çöpe atılamaz");
                }
            }

            if (inHandItem != null)
            {
                HandleInHandItem(target);
            }
        }
    }

    private void HandleInHandItem(GameObject target)
    {
        if (inHandItem == null)
            return;

        switch (inHandItem.tag)
        {
            case "Kettle":
                HandleKettleInteraction(target);
                break;

            case "Tea_Cup":
                HandleTeaCupInteraction(target);
                break;

            case "Other_Products":
                HandleOtherProductsInteraction(target);
                break;

            case "Tea_Can":
                HandleTeaCanInteraction(target);
                break;

            case "Garbage_Bag":
                HandleGarbageBagInteraction(target);
                break;

            default:
                Debug.Log("Unhandled item type.");
                break;
        }
    }

    private void HandleKettleInteraction(GameObject target)
    {
        if (inHandItem == null || !inHandItem.CompareTag("Kettle"))
            return;

        Kettle kettleScript = inHandItem.GetComponent<Kettle>();
        if (kettleScript == null)
            return;

        if (target.CompareTag("Tea_Cup"))
        {
            var teaCupScript = target.GetComponent<Tea_Cup>();
            var dirtyStatusScript = target.GetComponent<DirtyStatus>();
            if (teaCupScript == null)
                return;
            if (dirtyStatusScript.isDirty)
                return;

            if (!teaCupScript.isFullTea && kettleScript.currentKettleMagazine > 0 && !teaCupScript.isFillOraletorCoffee && kettleScript.isBrewed)
            {
                kettleScript.PourTea(teaCupScript);
            }
            else if (!kettleScript.isBrewed)
            {
                ShowUIMessage("Çay henüz demlenmemiş", false);
                Debug.Log("Çay henüz demlenmemiş");
            }
            else
            {
                ShowUIMessage("Bardak doldurulamıyor", false);
                Debug.Log("Dolduramazsın");
            }
        }

        if (target.CompareTag("Hot_Water"))
        {
            if (!kettleScript.isHaveHotWater)
            {
                kettleScript.AddHotWater();
                StartCoroutine(PlayHotWaterTapAnimation());
                ShowUIMessage("Kettle'a sıcak su eklendi", false);
            }
            else
            {
                ShowUIMessage("Kettle zaten sıcak su içeriyor", false);
            }
        }

        DirtyStatus dirtyStatus = inHandItem.GetComponent<DirtyStatus>();
        if (dirtyStatus != null && isPicked)
        {
            if (target.CompareTag("Water"))
            {
                Debug.Log("Water'ı gördü");
                dirtyStatus.CleanDirt();
            }
        }
    }

    // HandleTeaCupInteraction metodunda değişiklik:

    private void HandleTeaCupInteraction(GameObject target)
    {
        if (inHandItem == null || !inHandItem.CompareTag("Tea_Cup"))
            return;

        Tea_Cup teaCupScript = inHandItem.GetComponent<Tea_Cup>();
        if (teaCupScript == null)
            return;

        // Check if cup is dirty before handling hot water interaction
        DirtyStatus dirtyStatus = inHandItem.GetComponent<DirtyStatus>();
        bool isDirty = dirtyStatus != null && dirtyStatus.isDirty;

        if (target.CompareTag("Hot_Water"))
        {
            if (isDirty)
            {
                ShowUIMessage("Bardak kirli, önce yıkamalısın!", false);
                return;
            }

            bool canFillTea = teaCupScript.isFillTea && !teaCupScript.isFullTea;
            bool canFillOraletCoffee = teaCupScript.isFillOraletorCoffee && !teaCupScript.isFullOraletorCoffee;
            bool canFillKettle = false;

            Kettle kettleScript = inHandItem.GetComponent<Kettle>();
            if (kettleScript != null && !kettleScript.isHaveHotWater)
            {
                canFillKettle = true;
            }

            // Sadece bir işlem yapılabilecekse animasyonu çal
            if (canFillTea || canFillOraletCoffee || canFillKettle)
            {
                StartCoroutine(PlayHotWaterTapAnimation());

                if (canFillTea)
                {
                    teaCupScript.FillHotWaterToTea();
                }
                else if (canFillOraletCoffee)
                {
                    teaCupScript.FillHotWaterToCoffeeOrOralet();
                }
                else if (canFillKettle)
                {
                    kettleScript.isHaveHotWater = true;
                }
            }
            else
            {
                Debug.Log("Sıcak su dolduramazsın");
            }
        }

        if (dirtyStatus != null && isPicked)
        {
            if (target.CompareTag("Water"))
            {
                Debug.Log("Water'ı gördü");
                dirtyStatus.CleanDirt();
                //inHandItem.GetComponent<Tea_Cup>().EmptyCup();
            }
        }
    }

    private void HandleOtherProductsInteraction(GameObject target)
    {
        if (inHandItem == null || !inHandItem.CompareTag("Other_Products"))
            return;

        OraletAndCoffee productScript = inHandItem.GetComponent<OraletAndCoffee>();
        if (productScript == null)
            return;

        if (target.CompareTag("Tea_Cup"))
        {
            Tea_Cup teaCupScript = target.GetComponent<Tea_Cup>();
            if (teaCupScript == null)
                return;

            // Check if cup is dirty
            DirtyStatus dirtyStatus = target.GetComponent<DirtyStatus>();
            if (dirtyStatus != null && dirtyStatus.isDirty)
            {
                ShowUIMessage("Bardak kirli, önce yıkamalısın!", false);
                return;
            }

            if (!teaCupScript.isFillOraletorCoffee && !teaCupScript.isFillTea &&
                !teaCupScript.isFullTea && productScript.currentMagazine > 0)
            {
                teaCupScript.AddOraletOrCoffee(productScript.typeOfProduct);
                productScript.reduceProduct();
            }
            else
            {
                Debug.Log("Oralet veya kahve dolduramazsın");
            }
        }
    }

    private void HandleTeaCanInteraction(GameObject target)
    {
        if (inHandItem == null || !inHandItem.CompareTag("Tea_Can"))
            return;

        TeaCanScript teaCanScript = inHandItem.GetComponent<TeaCanScript>();
        if (teaCanScript == null)
            return;

        if (target.CompareTag("Kettle"))
        {
            Kettle kettleScript = target.GetComponent<Kettle>();
            if (kettleScript == null)
                return;

            // Check if kettle is dirty
            DirtyStatus dirtyStatus = target.GetComponent<DirtyStatus>();
            if (dirtyStatus != null && dirtyStatus.isDirty)
            {
                ShowUIMessage("Çaydanlık kirli, önce yıkamalısın!", false);
                return;
            }

            if (!kettleScript.isHaveTea && teaCanScript.currentTeaCanMagazine > 0)
            {
                kettleScript.AddTea();
                teaCanScript.ReduceTeaOnCan();
            }
            else
            {
                if (teaCanScript.currentTeaCanMagazine <= 0)
                {
                    Debug.Log("Çay Kutusu Boş");
                }
                else
                {
                    Debug.Log("Çaydanlıkta Zaten Çay Var");
                }
            }
        }
    }

    private void HandleGarbageBagInteraction(GameObject target)
    {
        if (inHandItem == null || !inHandItem.CompareTag("Garbage_Bag"))
            return;

        if (target.CompareTag("Garbage_Container"))
        {
            Destroy(inHandItem);
            isPicked = false;
            inHandItem = null;
        }
    }
    #endregion
    #region Pick and put and tray
    private void PickAndPut(InputAction.CallbackContext context)//E
    {
        if (playerCam == null)
        {
            Debug.LogError("Player camera is null! Cannot perform raycast.");
            return;
        }

        if (!Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
        {
            return;
        }
        GameObject target = hit.collider.gameObject;

        if (isPicked)
        {
            // Eğer inHandItem null ise işleme devam etmemeliyiz
            if (inHandItem == null)
            {
                Debug.LogWarning("isPicked true ama inHandItem null!");
                isPicked = false;
                return;
            }

            //TEPSİ SİSTEMİ
            if (inHandItem.gameObject.tag == "Tea_Cup" && target.tag == "Tray")
            {
                Tea_Cup teaCup = inHandItem.GetComponent<Tea_Cup>();
                if (teaCup != null && !teaCup.isOnTray)
                {
                    teaCup.isOnTray = true;
                    isPicked = false;
                    //Tepsiye sabitle
                    Transform trayTransform = hit.collider.transform.root;
                    inHandItem.transform.SetParent(trayTransform, true);
                    inHandItem.transform.rotation = Quaternion.identity;

                    SetItemPositionOnSurface(inHandItem, hit.point);

                    EnablePhysics(inHandItem, true);

                    DisablePhysics();

                    inHandItem = null;
                    return;
                }
            }

            if (((1 << target.layer) & placementLayer) != 0)
            {//BIRAKMA SİSTEMİ  
                if (inHandItem.tag != "Mop" && inHandItem.tag != "Garbage_Bag")
                {
                    isPicked = false;

                    if (inHandItem.tag == "Other_Products")
                    {
                        inHandItem.GetComponent<OraletAndCoffee>().CoverPutAndRemove(false);
                    }

                    if (inHandItem.tag == "Tea_Can")
                    {
                        inHandItem.GetComponent<TeaCanScript>().CoverPutAndRemove(false);
                    }

                    inHandItem.transform.SetParent(null);
                    SetItemPositionOnSurface(inHandItem, hit.point);

                    EnablePhysics(inHandItem, true);
                    inHandItem = null;
                    return;
                }
            }

            if (((1 << target.layer) & groundLayer) != 0 && (inHandItem.tag == "Mop" || inHandItem.tag == "Garbage_Bag"))
            {
                isPicked = false;

                inHandItem.transform.SetParent(null);
                SetItemPositionOnSurface(inHandItem, hit.point);

                EnablePhysics(inHandItem, true);
                inHandItem = null;
                return;
            }
        }

        else
        {
            if (((1 << target.layer) & interactionLayer) != 0)
            {
                Tea_Cup teaCup = target.GetComponent<Tea_Cup>();

                if (teaCup != null && teaCup.isOnTray)
                {
                    teaCup.isOnTray = false;
                }

                // Masa kontrolü
                TableController table = hit.collider.GetComponent<TableController>();
                if (table != null)
                {
                    // Masa kontrolü ekledik ama burada işlem yapmıyoruz
                    // Masa etkileşimi Use metodu ile (F tuşu) gerçekleştirilecek
                    return;
                }

                // Check if this is a tea cup with dirty status
                DirtyStatus dirtyStatus = target.GetComponent<DirtyStatus>();
                if (teaCup != null && dirtyStatus != null && dirtyStatus.isDirty)
                {
                    ShowUIMessage("This cup is dirty! Take it to the sink to clean", true);
                }

                // Normal eşya alma mekaniği
                isPicked = true;
                inHandItem = target;
                inHandItem.transform.SetParent(firstPersonHand.transform, false);
                inHandItem.transform.localPosition = Vector3.zero;
                inHandItem.transform.localRotation = Quaternion.identity;

                Debug.Log($"Picked up regular object: {target.name} with scale: {inHandItem.transform.localScale}");

                if (inHandItem.tag == "Other_Products")
                {
                    inHandItem.GetComponent<OraletAndCoffee>().CoverPutAndRemove(true);
                }
                if (inHandItem.tag == "Tea_Can")
                {
                    inHandItem.GetComponent<TeaCanScript>().CoverPutAndRemove(true);
                }

                EnablePhysics(inHandItem, false);
            }
        }
    }
    #endregion
    #region UpdateUI
    void UpdateUIAndHighlight()
    {
        // Default state - hide interaction UI elements if not needed
        e_interact.SetActive(false);
        f_interact.SetActive(false);

        // Reset contextual message flag at the start of each frame
        // This will allow non-contextual messages to stay on screen until their timer expires
        if (isContextualMessage)
        {
            isContextualMessage = false;
            mainInfoUI.SetActive(false);
        }

        // Raycast'i her frame yapmak yerine interval'da yap
        float currentTime = Time.time;
        if (currentTime - lastRaycastTime >= RAYCAST_INTERVAL)
        {
            hitCacheValid = Physics.Raycast(playerCam.position, playerCam.forward, out cachedHit, rayCastRange);
            lastRaycastTime = currentTime;

            // If no hit, clear any previous highlight
            if (!hitCacheValid && lastHighlightedObject != null)
            {
                lastHighlightedObject.GetComponent<HighLight>()?.ToggleHighLight(false);
                lastHighlightedObject = null;
            }
        }

        // Cached hit sonucunu kullan
        if (hitCacheValid)
        {
            hit = cachedHit;

            if (lastHighlightedObject != null && (hitCacheValid == false || cachedHit.collider.gameObject != lastHighlightedObject))
            {
                lastHighlightedObject.GetComponent<HighLight>()?.ToggleHighLight(false);
                lastHighlightedObject = null;
            }

            // Check for NPC first
            if (hitCacheValid)
            {
                // Check for NPC
                NPC npc = cachedHit.collider.GetComponent<NPC>();
                if (npc != null)
                {
                    string npcType = npc.IsGroupLeader() ? "Grup Lideri" : "Müşteri";
                    string status = npc.HasDrink() ? "İçiyor" : "Bekliyor";
                    string request = string.IsNullOrEmpty(npc.GetRequestedDrink()) ? "Yok" : npc.GetRequestedDrink();
                    string patience = "Sabır: " + npc.GetPatienceLevel();

                    Debug.Log($"[UI] NPC bilgisi: Tip={npcType}, Durum={status}, İstek={request}, Sabır={patience}");

                    if (npc.isPaying)
                    {
                        ShowUIMessage($"{npcType}\nÖdeme için F tuşuna basın", true);
                        f_interact.SetActive(true);
                    }
                    else
                    {
                        ShowUIMessage($"{npcType}\nDurum: {status}\nİstek: {request}\n{patience}", true);
                    }

                    return;
                }

                // Check for Table - DÜZELTME: TableController bulamazsak, transform.parent'tan kontrol edeceğiz
                TableController table = cachedHit.collider.GetComponent<TableController>();

                // Eğer doğrudan objede TableController yoksa, parent'ını kontrol et
                if (table == null)
                {
                    if (cachedHit.collider.transform.parent != null)
                    {
                        table = cachedHit.collider.transform.parent.GetComponent<TableController>();
                    }
                }

                if (table != null)
                {
                    // Eğer masada grup var ve grup oturmuşsa
                    if (table.HasGroup() && table.IsGroupSeated())
                    {
                        Debug.Log($"[UI] Masa bulundu ve grubu oturmuş durumda: {table.name}");
                        string gameRequest = table.GetRequestedGameType();

                        // Eğer oyun isteği yoksa, tekrar deneyeceğiz
                        if (string.IsNullOrEmpty(gameRequest) && table.HasGroup())
                        {
                            NPCGroup group = FindObjectOfType<NPCGroup>();
                            if (group != null)
                            {
                                gameRequest = group.GetRequestedGame();
                            }
                        }

                        if (string.IsNullOrEmpty(gameRequest))
                        {
                            gameRequest = "Yok";
                        }

                        ShowUIMessage($"Masa\nDurum: Dolu\nOyun İsteği: {gameRequest}", true);
                    }
                    else if (table.HasGroup() && !table.IsGroupSeated())
                    {
                        // Masa dolu ama henüz tüm grup oturmamış
                        ShowUIMessage($"Masa\nDurum: Dolu\nMüşteriler oturuyor...", true);
                    }
                    else
                    {
                        // Masa boş
                        ShowUIMessage($"Masa\nDurum: Boş", true);
                    }
                    return;
                }

                // Special NPC kontrolünü NPC kontrolünden sonra ekleyelim
                specialNPC = cachedHit.collider.GetComponent<SpecialNPC>();
                if (specialNPC == null && cachedHit.collider.transform.parent != null)
                {
                    specialNPC = cachedHit.collider.transform.parent.GetComponent<SpecialNPC>();
                }

                // Sadece hasReachedWaitingPosition true ise ve daha önce etkileşime girilmediyse UI mesajını göster
                if (specialNPC != null && !specialNPC.hasInteracted && specialNPC.hasReachedWaitingPosition)
                {
                    ShowUIMessage($"{specialNPC.GetNPCName()}\nKonuşmak için F tuşuna basın", true);
                    cachedHit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = cachedHit.collider.gameObject;
                    f_interact.SetActive(true);
                    return;
                }
            }
        }

        // Original UI logic continues from here
        if (hitCacheValid && ((1 << cachedHit.collider.gameObject.layer) & interactionLayer.value) != 0 && !isPicked)
        {
            cachedHit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
            lastHighlightedObject = cachedHit.collider.gameObject;
            if (/*Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange)&&*/cachedHit.collider.CompareTag("Kettle"))
            {
                var kettleScript = cachedHit.collider.GetComponent<Kettle>();
                if (kettleScript.currentKettleMagazine > 0)
                {
                    ShowUIMessage("", true);
                    e_interact.SetActive(true);
                }
                else
                {
                    if (kettleScript.isHaveTea && !kettleScript.isHaveHotWater)
                    {
                        ShowUIMessage("", true);
                        e_interact.SetActive(true);
                    }
                    else if (!kettleScript.isHaveTea && kettleScript.isHaveHotWater)
                    {
                        ShowUIMessage("", true);
                        e_interact.SetActive(true);
                    }
                    else if (kettleScript.isHaveTea && kettleScript.isHaveHotWater)
                    {
                        if (kettleScript.CheckIsOnKettleBase())
                        {
                            ShowUIMessage("Demleniyor\n" + (int)kettleScript.currentBrewTimeOfTea + " saniye kaldı", true);
                        }
                        else
                        {
                            ShowUIMessage("", true);
                            e_interact.SetActive(true);
                        }
                    }
                    else if (!kettleScript.isHaveTea && !kettleScript.isHaveHotWater)
                    {
                        ShowUIMessage("", true);
                        e_interact.SetActive(true);
                    }
                }
            }
            else
            {
                ShowUIMessage("", true);
                e_interact.SetActive(true);
            }
        }

        if (hitCacheValid && ((1 << cachedHit.collider.gameObject.layer) & placementLayer.value) != 0 && isPicked && inHandItem != null &&
            !(inHandItem.tag == "Mop" || inHandItem.tag == "Garbage_Bag"))
        {
            cachedHit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
            ShowUIMessage("", true);
            e_interact.SetActive(true);
        }
        if (hitCacheValid && ((1 << cachedHit.collider.gameObject.layer) & groundLayer.value) != 0 && isPicked && inHandItem != null &&
            (inHandItem.tag == "Mop" || inHandItem.tag == "Garbage_Bag"))
        {
            cachedHit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
            ShowUIMessage("", true);
            e_interact.SetActive(true);
        }




        if (hitCacheValid && ((1 << cachedHit.collider.gameObject.layer) & useableLayer.value) != 0 && !isPicked && cachedHit.collider.GetComponent<IInteractable>() != null)
        {
            if (cachedHit.collider.GetComponent<DoorTrigger>())
            {
                if (dayNightCycleControllerScript.isDayFinished && !dayNightCycleControllerScript.IsThereAnyNPC())
                {
                    f_interact.SetActive(true);
                }
                return;
            }

            else if (cachedHit.collider.CompareTag("Lamb_Switch"))
            {
                lastHighlightedObject = cachedHit.collider.gameObject;
                ShowUIMessage("", true);
                f_interact.SetActive(true);
                return;
            }

            else if (cachedHit.collider.CompareTag("Handbook"))
            {
                lastHighlightedObject = cachedHit.collider.gameObject;
                ShowUIMessage("", true);
                f_interact.SetActive(true);
                return;
            }
            else
            {
                cachedHit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                lastHighlightedObject = cachedHit.collider.gameObject;
                ShowUIMessage("", true);
                f_interact.SetActive(true);
                return;
            }
        }

        if (hitCacheValid/*&&(inHandItem.tag=="Tea_Cup"/*BURAYA DİĞER BARDAKLARDA GELEBİLİR)*/&& isPicked)
        {
            if (inHandItem != null)
            {
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (inHandItem.gameObject.tag == "Tea_Cup" && inHandItem.GetComponent<Tea_Cup>() != null &&
                        !inHandItem.GetComponent<Tea_Cup>().isOnTray && hit.collider.gameObject.tag == "Tray")
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("", true);
                        e_interact.SetActive(true);
                    }
                    else
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                        lastHighlightedObject = null;
                    }
                }
            }
        }

        if (hitCacheValid && inHandItem != null && inHandItem.tag == "Tea_Cup" && isPicked)
        {//TEA CUP KONTROLÜ
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
            {
                if (hit.collider.CompareTag("Hot_Water") && (inHandItem.GetComponent<Tea_Cup>().isFillOraletorCoffee || inHandItem.GetComponent<Tea_Cup>().isFillTea))
                {
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("", true);
                    f_interact.SetActive(true);
                    return;
                }
            }
        }

        if (hitCacheValid && inHandItem != null && inHandItem.tag == "Kettle"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&& isPicked)
        {//KETTLE DAN ÇAY KOYMA UI
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
            {
                if (hit.collider.CompareTag("Tea_Cup"))
                {
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("", true);
                    f_interact.SetActive(true);
                }
            }
        }

        if (hitCacheValid && inHandItem != null && inHandItem.tag == "Other_Products"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&& isPicked)
        {//bardağa Kahve veya oralet koyma
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
            {
                if (hit.collider.CompareTag("Tea_Cup"))
                {
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("", true);
                    f_interact.SetActive(true);
                }
            }
        }

        if (hitCacheValid && inHandItem != null && inHandItem.tag == "Tea_Cup"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&& isPicked)
        {//KETTLE DAN ÇAY KOYMA UI
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
            {
                if (hit.collider.CompareTag("Hot_Water"))
                {
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("", true);
                    f_interact.SetActive(true);
                }
            }

            // Bu kısım aslında Tea_Cup kontrolü içinde olmamalı, dışarı alalım
        }

        // Tea Can kontrolünü ayrı bir blok olarak yazalım
        if (hitCacheValid && inHandItem != null && inHandItem.tag == "Tea_Can" && isPicked)
        {
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
            {
                if (hit.collider.CompareTag("Kettle"))
                {
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("", true);
                    f_interact.SetActive(true);
                }
            }
        }

        // DirtyStatus kontrolünü ayrı bir blok olarak yazalım
        if (hitCacheValid && inHandItem != null && inHandItem.GetComponent<DirtyStatus>() != null && isPicked)//WASH UI
        {
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
            {
                if (hit.collider.CompareTag("Water") && inHandItem.GetComponent<DirtyStatus>())
                {
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("", true);
                    f_interact.SetActive(true);
                }
            }
        }

        // Garbage Bin kontrolünü ayrı bir blok olarak yazalım
        if (hitCacheValid && Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.CompareTag("Garbage_Bin") && isPicked)
        {
            if (inHandItem != null && (inHandItem.layer == 6))//ATILMAYACAK ESYALAR TAG TAG EKLENDI
            {
                if (!(inHandItem.tag == "Mop" || inHandItem.tag == "Tray" || inHandItem.tag == "Kettle" || inHandItem.tag == "Garbage_Bin" || inHandItem.tag == "Garbage_Bag"))
                {
                    ShowUIMessage("", true);
                    f_interact.SetActive(true);
                }
                else
                {
                    ShowUIMessage("Bu eşya çöpe atılamaz", true);
                }
            }
        }

        // Garbage Container kontrolünü ayrı bir blok olarak yazalım
        if (hitCacheValid && Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.CompareTag("Garbage_Container") && isPicked)
        {
            if (inHandItem != null && inHandItem.tag == "Garbage_Bag")
            {
                ShowUIMessage("", true);
                f_interact.SetActive(true);
            }
        }
    }


    #endregion

    public void ShowUIMessage(string message, bool isContextual = false)
    {
        // Set the contextual flag
        isContextualMessage = isContextual;

        // If a previous non-contextual message coroutine is running and this is not a contextual message, stop it
        if (currentMessageCoroutine != null && !isContextualMessage)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }

        // If message is empty, just hide the UI
        if (string.IsNullOrEmpty(message))
        {
            mainInfoUI.SetActive(false);
            return;
        }

        // Show message
        mainInfoUI.SetActive(true);
        mainInfoUIText.text = message;

        // Only start the hide coroutine for non-contextual messages
        if (!isContextual && currentMessageCoroutine == null)
        {
            currentMessageCoroutine = StartCoroutine(HideMessageAfterDelay(3.0f));
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Only hide if it's still not a contextual message
        if (!isContextualMessage && mainInfoUI != null)
        {
            mainInfoUI.SetActive(false);
        }

        currentMessageCoroutine = null;
    }

    private void SetItemPositionOnSurface(GameObject item, Vector3 hitPoint)
    {
        Collider col = item.GetComponent<Collider>();
        if (col != null/*&&inHandItem.tag!="Tea_Cup"*/)
        {
            float bottomY = col.bounds.min.y;
            float offsetY = item.transform.position.y - bottomY;

            item.transform.position = hitPoint + Vector3.up * offsetY; ;
        }

        else
        {
            item.transform.position = hitPoint;
        }
    }

    private void EnablePhysics(GameObject item, bool enable)
    {
        Rigidbody rb = item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = enable;
            rb.isKinematic = !enable;
        }
    }
    private void DisablePhysics()
    {
        // İlk olarak inHandItem'in null olup olmadığını kontrol et
        if (inHandItem == null)
            return;

        Rigidbody rb = inHandItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        /*Collider col = inHandItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }*/
    }

    public void CreateGarbageBag(GameObject garbageBagObj, Vector3 garbagePosition)
    {
        Debug.Log("Çöp üretti");
        Instantiate(garbageBagObj);
        garbageBagObj.transform.position = new Vector3(garbagePosition.x, garbagePosition.y, garbagePosition.z - 3);
        /*inHandItem.transform.SetParent(firstPersonHand.transform, false);
        inHandItem.transform.localPosition = new Vector3(0, -1, 0);
        inHandItem.transform.localRotation = Quaternion.identity;
        isPicked = true;*/
    }

    /// <summary>
    /// Sets the picked status of the player
    /// </summary>
    public void SetPickedStatus(bool status)
    {
        isPicked = status;
    }

    IEnumerator PlayHotWaterTapAnimation()
    {
        if (tapSteamParticle != null)
        {
            tapSteamParticle.Clear();
            tapSteamParticle.Play();
            hotWaterAnimator.SetBool("isUseHotWater", true);
            yield return new WaitForSeconds(1f);
            tapSteamParticle.Stop();
            hotWaterAnimator.SetBool("isUseHotWater", false);
        }
        else
        {
            Debug.LogError("Tap Steam Particle System is not assigned!");
        }
    }

    private void OnDestroy()
    {
        // Clean up coroutines to prevent memory leaks
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }

        if (cleaningCoroutine != null)
        {
            StopCoroutine(cleaningCoroutine);
            cleaningCoroutine = null;
        }

        // Unsubscribe from input events to prevent memory leaks
        if (pickAndPutInput != null && pickAndPutInput.action != null)
        {
            pickAndPutInput.action.performed -= PickAndPut;
        }

        if (useInput != null && useInput.action != null)
        {
            useInput.action.performed -= Use;
        }

        if (useHoldInput != null && useHoldInput.action != null)
        {
            useHoldInput.action.performed -= UseHold;
        }
    }
}