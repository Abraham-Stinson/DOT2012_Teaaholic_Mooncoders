using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GarbageScript garbageScript;
    [Header("Player")]
    [SerializeField] private Transform playerCam;
    [SerializeField][Min(1)] private float rayCastRange = 10f;
    [SerializeField] private bool isPicked = false; //aaa
    [Header("UI")]
    [SerializeField] private GameObject mainInfoUI;
    [SerializeField] private TextMeshProUGUI mainInfoUIText;
    /*[SerializeField] private GameObject pickUpUI;
    [SerializeField] private GameObject putDownUI;
    [SerializeField] private GameObject useUI;
    [SerializeField] private GameObject pourUI;
    [SerializeField] private GameObject putOnTray;
    [SerializeField] private GameObject cleanTrashUI;
    [SerializeField] private GameObject CleanTheDirt;
    public UnityEngine.UI.Image thrashCleanProgressBar;*/

    [Header("Layers")]
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private LayerMask useableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask npcLayer; // Layer for NPC interaction
    [Header("First Person Hand")]
    [SerializeField] private Transform firstPersonHand;//when pick up objects it will show on this transform
    [SerializeField] public GameObject inHandItem;//what we picked up
    private RaycastHit hit;
    private GameObject lastHighlightedObject;

    [Header("Inputs")]
    [SerializeField] private InputActionReference pickAndPutInput;
    [SerializeField] private InputActionReference useInput;
    [SerializeField] private InputActionReference useHoldInput;

    [Header("Trash Clean")]
    [SerializeField] private float cleaningTime = 3f;
    [SerializeField] private float cleaningRadius = 2f;
    private Coroutine cleaningCoroutine;
    private bool isCleaning = false;

    // Reference to the currently highlighted NPC for serving drinks
    private NPC currentNPC = null;
    // Reference to the currently highlighted table for placing games
    private Table currentTable = null;
    // Reference to table game being interacted with
    private TableGame currentTableGame = null;
    // Reference to the cashier NPC for payment
    private NPC cashierNPC = null;
    [SerializeField] private float paymentAmount = 15.0f; // Default payment amount

    void Start()
    {
        pickAndPutInput.action.performed += PickAndPut;
        useInput.action.performed += Use;
        useHoldInput.action.performed += UseHold;

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
        if (inHandItem != null && inHandItem.CompareTag("Mop") && hit.collider.gameObject.tag == "Trash")
        {
            Destroy(hit.collider.gameObject);
            garbageScript.AddThrashToGarbage();
        }
    }
    #endregion
    #region USE INPUT
    private void Use(InputAction.CallbackContext context) // F
    {
        if (!Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
        {
            return;
        }

        GameObject target = hit.collider.gameObject;

        if (hit.collider != null)
        {
            // Check for NPC interaction
            NPC npc = target.GetComponent<NPC>();
            if (npc != null && inHandItem != null && inHandItem.CompareTag("Tea_Cup"))
            {
                // Handle serving drinks to NPC
                HandleDrinkServing(npc);
                return;
            }
            
            // Handle cashier payment
            if (npc != null && npc.IsGroupLeader && !npc.HasPaid && Vector3.Distance(npc.transform.position, GameObject.FindWithTag("CashierSpot").transform.position) < 1.5f)
            {
                HandlePayment(npc);
                return;
            }
            
            // Handle table game placement
            TableGame tableGame = null;
            if (inHandItem != null)
            {
                tableGame = inHandItem.GetComponent<TableGame>();
            }
            
            Table table = hit.collider.GetComponent<Table>();
            if (table != null && tableGame != null && isPicked)
            {
                PlaceTableGame(tableGame, table);
                return;
            }

            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange, useableLayer) && !isPicked)
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.interact();
                    return;
                }
            }

            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.CompareTag("Garbage_Bin"))
            {
                if (inHandItem != null && (inHandItem.layer == 6) && !(inHandItem.tag == "Mop" || inHandItem.tag == "Tray" || inHandItem.tag == "Kettle" || inHandItem.tag == "Garbage_Bin" || inHandItem.tag == "Garbage_Bag"))//ATILMAYACAK ESYALAR TAG TAG EKLENDI
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

    private void HandleDrinkServing(NPC npc)
    {
        if (!isPicked || inHandItem == null || !inHandItem.CompareTag("Tea_Cup"))
            return;
            
        Tea_Cup teaCup = inHandItem.GetComponent<Tea_Cup>();
        if (teaCup == null)
            return;
            
        // Get the NPC's group and check if this drink matches their order
        NPCGroup group = npc.gameObject.GetComponentInParent<NPCGroup>();
        if (group == null)
            return;
            
        string inCupDrink = teaCup.inCup;
        string orderedDrink = group.GetDrinkOrderForNPC(npc);
        
        // Check if the drink matches the order
        if (inCupDrink == orderedDrink)
        {
            // Find the table this NPC is sitting at
            Table table = FindTableForNPC(npc);
            if (table != null)
            {
                // Find the NPC's index position at the table
                int npcIndex = FindNPCIndexAtTable(npc, table);
                if (npcIndex >= 0)
                {
                    // Place the cup on the table in front of the NPC
                    table.PlaceDrink(inHandItem, npcIndex);
                    
                    // Mark the drink as served
                    group.ServeDrinkToNPC(npc);
                    
                    // Release the cup
                    isPicked = false;
                    inHandItem = null;
                    
                    Debug.Log($"Successfully served {inCupDrink} to NPC");
                }
            }
        }
        else
        {
            Debug.Log($"Wrong drink! NPC ordered {orderedDrink} but you're serving {inCupDrink}");
        }
    }

    private void HandlePayment(NPC npc)
    {
        // Process payment
        npc.SetPaid(true);
        
        // In a real game, you would add money to the player's account here
        // Example: playerMoney += paymentAmount;
        
        Debug.Log($"Payment of ${paymentAmount} received from customer");
    }

    private void PlaceTableGame(TableGame tableGame, Table table)
    {
        if (table.IsAvailable || table.HasGame(tableGame.GetGameType()))
            return;
            
        tableGame.PlaceOnTable(table);
        
        // Release the game from hand
        isPicked = false;
        inHandItem = null;
    }

    private Table FindTableForNPC(NPC npc)
    {
        // Find the closest table to this NPC
        Table[] tables = FindObjectsOfType<Table>();
        Table closestTable = null;
        float minDistance = float.MaxValue;
        
        foreach (Table table in tables)
        {
            float distance = Vector3.Distance(npc.transform.position, table.transform.position);
            if (distance < minDistance && distance < 3f) // Only consider tables within 3 units
            {
                minDistance = distance;
                closestTable = table;
            }
        }
        
        return closestTable;
    }

    private int FindNPCIndexAtTable(NPC npc, Table table)
    {
        // Find which chair position the NPC is sitting at
        for (int i = 0; i < 4; i++) // Assuming max 4 chairs per table
        {
            Transform chairTransform = table.GetChairTransform(i);
            if (chairTransform != null && Vector3.Distance(npc.transform.position, chairTransform.position) < 0.5f)
            {
                return i;
            }
        }
        
        return -1;
    }

    private void HandleInHandItem(GameObject target)
    {
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
        if (target.CompareTag("Tea_Cup"))
        {
            var teaCupScript = target.GetComponent<Tea_Cup>();
            if (!teaCupScript.isFullTea && inHandItem.GetComponent<Kettle>().currentKettleMagazine > 0 && !teaCupScript.isFillOraletorCoffee)
            {
                teaCupScript.AddTea();
                inHandItem.GetComponent<Kettle>().PourTea();
                Debug.Log("Çay eklendi");
            }
            else
            {
                Debug.Log("Dolduramazsın");
            }
        }

        if (target.CompareTag("Hot_Water"))
        {
            if (!inHandItem.GetComponent<Kettle>().isHaveHotWater/*&&inHandItem.GetComponent<Kettle>().isHaveTea*/)
            {
                inHandItem.GetComponent<Kettle>().isHaveHotWater = true;
                Debug.Log("Kettle Sıcak su eklendi");
                inHandItem.GetComponent<Kettle>().ChangeBrewTime();
            }
        }

        if (inHandItem.GetComponent<DirtyStatus>() != null && isPicked)
        {
            if (target.CompareTag("Water"))
            {
                Debug.Log("Water'ı gördü");
                var isDirtyinHandItem = inHandItem.GetComponent<DirtyStatus>();
                isDirtyinHandItem.CleanDirt();
            }
        }
    }

    private void HandleTeaCupInteraction(GameObject target)
    {
        if (target.CompareTag("Hot_Water"))
        {
            if (inHandItem.GetComponent<Tea_Cup>().isFillTea)
            {
                inHandItem.GetComponent<Tea_Cup>().FillHotWaterToTea();
            }
            else if (inHandItem.GetComponent<Tea_Cup>().isFillOraletorCoffee)
            {
                inHandItem.GetComponent<Tea_Cup>().FillHotWaterToCoffeeOrOralet();
            }
            else if (!inHandItem.GetComponent<Kettle>().isHaveHotWater)
            {
                inHandItem.GetComponent<Kettle>().isHaveHotWater = true;
            }
            else
            {
                Debug.Log("Sıcak su dolduramazsın");
            }
        }

        else if (inHandItem.GetComponent<DirtyStatus>() != null && isPicked)
        {
            if (target.CompareTag("Water"))
            {
                Debug.Log("Water'ı gördü");
                var isDirtyinHandItem = inHandItem.GetComponent<DirtyStatus>();
                isDirtyinHandItem.CleanDirt();
                //inHandItem.GetComponent<Tea_Cup>().EmptyCup();
            }
        }
    }

    private void HandleOtherProductsInteraction(GameObject target)
    {
        if (target.CompareTag("Tea_Cup"))
        {
            var teaCupScript = target.GetComponent<Tea_Cup>();
            if (!teaCupScript.isFillOraletorCoffee && !teaCupScript.isFillTea && !teaCupScript.isFullTea && inHandItem.GetComponent<OraletAndCoffee>().currentMagazine > 0)
            {
                teaCupScript.AddOraletOrCoffee(inHandItem.GetComponent<OraletAndCoffee>().typeOfProduct);
                inHandItem.GetComponent<OraletAndCoffee>().reduceProduct();
            }
            else
            {
                Debug.Log("Oralet veya kahve dolduramazsın");
            }
        }

    }

    private void HandleTeaCanInteraction(GameObject target)
    {
        if (target.CompareTag("Kettle"))
        {
            var kettleScript = target.GetComponent<Kettle>();
            if (!kettleScript.isHaveTea && inHandItem.GetComponent<TeaCanScript>().currentTeaCanMagazine > 0)
            {
                inHandItem.GetComponent<TeaCanScript>().ReduceTeaOnCan();
                kettleScript.isHaveTea = true;
                Debug.Log("Çaya dem veridli");
            }
        }
    }

    private void HandleGarbageBagInteraction(GameObject target)
    {
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
        if (!Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
        {
            return;
        }
        GameObject target = hit.collider.gameObject;
        
        // Check for table game interaction when not holding anything
        if (!isPicked)
        {
            TableGame tableGame = target.GetComponent<TableGame>();
            if (tableGame != null && !tableGame.IsPlacedOnTable())
            {
                // Pick up the game
                isPicked = true;
                inHandItem = target;
                inHandItem.transform.SetParent(firstPersonHand.transform, false);
                inHandItem.transform.localPosition = Vector3.zero;
                inHandItem.transform.localRotation = Quaternion.identity;
                EnablePhysics(inHandItem, false);
                return;
            }
        }
        
        if (isPicked)
        {
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

                // Check if this is a game on the table
                else
                {
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
    }
    #endregion
    #region UpdateUI
    void UpdateUIAndHighlight()
    {
        bool didHit = Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange);

        if (lastHighlightedObject != null && (didHit == false || hit.collider.gameObject != lastHighlightedObject))
        {
            lastHighlightedObject.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
            currentNPC = null;
            currentTable = null;
        }

        mainInfoUI.SetActive(false);
        currentNPC = null;
        currentTable = null;

        if (didHit)
        {
            // Check for NPC with drink order
            NPC npc = hit.collider.gameObject.GetComponent<NPC>();
            if (npc != null && isPicked && inHandItem != null && inHandItem.CompareTag("Tea_Cup"))
            {
                currentNPC = npc;
                NPCGroup group = npc.gameObject.GetComponentInParent<NPCGroup>();
                
                if (group != null)
                {
                    Tea_Cup teaCup = inHandItem.GetComponent<Tea_Cup>();
                    if (teaCup != null)
                    {
                        string inCupDrink = teaCup.inCup;
                        string orderedDrink = group.GetDrinkOrderForNPC(npc);
                        
                        if (inCupDrink == orderedDrink)
                        {
                            ShowUIMessage($"Press F to serve {inCupDrink}");
                        }
                        else
                        {
                            ShowUIMessage($"Wrong item! Customer wants {orderedDrink}");
                        }
                    }
                }
            }
            
            // Check for cashier payment
            if (npc != null && npc.IsGroupLeader && !npc.HasPaid && 
                Vector3.Distance(npc.transform.position, GameObject.FindWithTag("CashierSpot").transform.position) < 1.5f)
            {
                cashierNPC = npc;
                ShowUIMessage($"Press F to collect payment (${paymentAmount})");
            }
            
            // Check for table game placement
            Table table = hit.collider.gameObject.GetComponent<Table>();
            if (table != null && !table.IsAvailable && isPicked && inHandItem != null)
            {
                TableGame tableGame = inHandItem.GetComponent<TableGame>();
                if (tableGame != null)
                {
                    currentTable = table;
                    ShowUIMessage($"Press F to place {tableGame.GetGameType()} on table");
                }
            }

            if (didHit && ((1 << hit.collider.gameObject.layer) & interactionLayer.value) != 0 && !isPicked)
            {
                hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                lastHighlightedObject = hit.collider.gameObject;
                if (/*Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange)&&*/hit.collider.CompareTag("Kettle"))
                {
                    var kettleScript = hit.collider.GetComponent<Kettle>();
                    if (kettleScript.currentKettleMagazine > 0)
                    {
                        ShowUIMessage("Press E to Pick Up\n" + kettleScript.currentKettleMagazine + " tea left");
                    }
                    else
                    {
                        if (kettleScript.isHaveTea && !kettleScript.isHaveHotWater)
                        {
                            ShowUIMessage("Press E to Pick Up\nInside: Tea");
                        }
                        else if (!kettleScript.isHaveTea && kettleScript.isHaveHotWater)
                        {
                            ShowUIMessage("Press E to Pick Up\nInside: Hot Water");
                        }
                        else if (kettleScript.isHaveTea && kettleScript.isHaveHotWater)
                        {
                            if (kettleScript.CheckIsOnKettleBase())
                            {
                                ShowUIMessage("It's brewing\n" + (int)kettleScript.currentBrewTimeOfTea + "second(s) left");
                            }
                            else
                            {
                                ShowUIMessage("Press E to Pick Up\nInside: Tea and Hot Water Put On Kettle Base to Brew");
                            }
                        }
                        else if (!kettleScript.isHaveTea && !kettleScript.isHaveHotWater)
                        {
                            ShowUIMessage("Press E to Pick Up\nInside: Empty");
                        }
                    }


                }

                else
                {
                    ShowUIMessage("Press E to Pick Up");
                }

            }
            if (didHit && ((1 << hit.collider.gameObject.layer) & placementLayer.value) != 0 && isPicked && !(inHandItem.tag == "Mop" || inHandItem.tag == "Garbage_Bag"))
            {
                hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                lastHighlightedObject = null;
                ShowUIMessage("Press E to Put Down");
            }
            if (didHit && ((1 << hit.collider.gameObject.layer) & groundLayer.value) != 0 && isPicked && (inHandItem.tag == "Mop" || inHandItem.tag == "Garbage_Bag"))
            {
                hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                lastHighlightedObject = null;
                ShowUIMessage("Press E to Put Down");
            }
            if (didHit && ((1 << hit.collider.gameObject.layer) & useableLayer.value) != 0 && !isPicked && hit.collider.GetComponent<IInteractable>() != null)
            {
                hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                lastHighlightedObject = hit.collider.gameObject;
                ShowUIMessage("Press F to Use");
            }

            if (didHit/*&&(inHandItem.tag=="Tea_Cup"/*BURAYA DİĞER BARDAKLARDA GELEBİLİR)*/&& isPicked)
            {
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (inHandItem.gameObject.tag == "Tea_Cup" && !inHandItem.GetComponent<Tea_Cup>().isOnTray && hit.collider.gameObject.tag == "Tray")
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("Press E to Put on Tray");
                    }
                    else
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                        lastHighlightedObject = null;
                    }
                }
            }

            if (didHit && inHandItem != null && inHandItem.tag == "Kettle"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&& isPicked)
            {//KETTLE DAN ÇAY KOYMA UI
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (hit.collider.CompareTag("Tea_Cup"))
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("Press F to Pour Tea");
                    }
                }

                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (hit.collider.CompareTag("Hot_Water") /*&& inHandItem.GetComponent<Kettle>().isHaveTea*/)
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("Press F to Fill Hot Water to Kettle");
                    }
                }
            }

            if (didHit && inHandItem != null && inHandItem.tag == "Other_Products"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&& isPicked)
            {//bardağa Kahve veya oralet koyma
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (hit.collider.CompareTag("Tea_Cup"))
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("Press F to Pour " + inHandItem.GetComponent<OraletAndCoffee>().typeOfProduct);
                    }
                }
            }

            if (didHit && inHandItem != null && inHandItem.tag == "Tea_Cup"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&& isPicked)
            {//KETTLE DAN ÇAY KOYMA UI
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (hit.collider.CompareTag("Hot_Water"))
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("Press F to Fill the Hot Water");
                    }
                }
            }

            if (didHit && inHandItem != null && inHandItem.tag == "Tea_Can" && isPicked)
            {
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (hit.collider.CompareTag("Kettle"))
                    {
                        hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("Press F to Put Tea To Kettle");
                    }
                }
            }


            if (didHit && inHandItem != null && inHandItem.GetComponent<DirtyStatus>() != null && isPicked)//WASH UI
            {
                if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange))
                {
                    if (hit.collider.CompareTag("Water") && inHandItem.GetComponent<DirtyStatus>())
                    {
                        lastHighlightedObject = hit.collider.gameObject;
                        ShowUIMessage("Press F to Wash");
                    }
                }
            }

            if (didHit && Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.CompareTag("Garbage_Bin") && isPicked)
            {
                if (inHandItem != null && (inHandItem.layer == 6))//ATILMAYACAK ESYALAR TAG TAG EKLENDI
                {
                    if (!(inHandItem.tag == "Mop" || inHandItem.tag == "Tray" || inHandItem.tag == "Kettle" || inHandItem.tag == "Garbage_Bin" || inHandItem.tag == "Garbage_Bag"))
                    {
                        ShowUIMessage("Press F to Throw the Item in the Garbage");
                    }
                    else
                    {
                        ShowUIMessage("You can't throw this item to Garbage");
                    }

                }

            }

            if (didHit && Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.CompareTag("Garbage_Container") && isPicked)
            {
                if (inHandItem != null && inHandItem.tag == "Garbage_Bag")
                {
                    ShowUIMessage("Press F to Throw the Garbage in the Container");
                }
            }

            if (didHit && hit.collider.gameObject.tag == "Trash")
            {//THRASH UI
                if (inHandItem != null && inHandItem.gameObject.tag == "Mop")
                {
                    ShowUIMessage("Hold the F to Clean Trash");
                }
                else
                {
                    ShowUIMessage("You need mop to clean thrash");
                }

                hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                lastHighlightedObject = hit.collider.gameObject;
            }
        }
    }
    #endregion

    void ShowUIMessage(string message)
    {
        mainInfoUI.SetActive(true);
        mainInfoUIText.text = message;
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
}