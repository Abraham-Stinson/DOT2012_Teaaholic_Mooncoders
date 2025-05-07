using System;
using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using Mono.Cecil;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
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
    [Header("First Person Hand")]
    [SerializeField] private Transform firstPersonHand;//when pick up objects it will show on this transform
    [SerializeField] private GameObject inHandItem;//what we picked up
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
    void Start()
    {
        pickAndPutInput.action.performed += PickAndPut;
        //putDownInput.action.performed+=PutDown;
        useInput.action.performed += Use;
        useHoldInput.action.performed += UseHold;
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
            // Check for interactable objects
            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange, useableLayer) && !isPicked)
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.interact();
                    return; // Exit after interaction
                }
            }

            if (Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange) && hit.collider.CompareTag("Garbage_Bin"))
            {
                if (inHandItem != null && (inHandItem.layer == 6) && !(inHandItem.tag == "Mop" || inHandItem.tag == "Tray" || inHandItem.tag == "Kettle" || inHandItem.tag == "Garbage_Bin" || inHandItem.tag == "Garbage_Bag"))//ATILMAYACAK ESYALAR TAG TAG EKLENDI
                {
                    //var garbageBagScript = hit.collider.GetComponent<GarbageScript>();
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

            // Handle interactions based on the item in hand
            if (inHandItem != null)
            {
                HandleInHandItem(target);
            }
        }
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

            /*if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange,interactionLayer)&&!isPicked){
                isPicked=true;
                if(hit.collider.GetComponent<Tea_Cup>()!=null&&hit.collider.GetComponent<Tea_Cup>().isOnTray){//PİCK UP TEPSİDEN
                    
                    hit.collider.GetComponent<Tea_Cup>().isOnTray=false;
                    inHandItem=hit.collider.gameObject;
                    //inHandItem.transform.position=Vector3.zero;
                    inHandItem.transform.rotation=Quaternion.identity;
                    inHandItem.transform.SetParent(firstPersonHand.transform,false);
                }
                /*else if(DİĞER BARDAKLAR EKLENEBİLİR)//
                else{
                    inHandItem=hit.collider.gameObject;
                    inHandItem.transform.position=Vector3.zero;
                    inHandItem.transform.rotation=Quaternion.identity;
                    inHandItem.transform.SetParent(firstPersonHand.transform,false);
                }

                Rigidbody rb = inHandItem.GetComponent<Rigidbody>();
                
                if(rb !=null){
                    rb.useGravity=false;
                    rb.isKinematic=true;
                }
                return;
            }*/

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



                isPicked = true;
                inHandItem = target;
                inHandItem.transform.SetParent(firstPersonHand.transform, false);
                inHandItem.transform.localPosition = Vector3.zero;
                inHandItem.transform.localRotation = Quaternion.identity;

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
        bool didHit = Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange);

        if (lastHighlightedObject != null && (didHit == false || hit.collider.gameObject != lastHighlightedObject))
        {
            lastHighlightedObject.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
        }

        mainInfoUI.SetActive(false);


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

    public void CreateGarbageBag(GameObject garbageBagObj,Vector3 garbagePosition)
    {
        Debug.Log("Çöp üretti");
            Instantiate(garbageBagObj);
            garbageBagObj.transform.position=new Vector3(garbagePosition.x+3,garbagePosition.y,garbagePosition.z);
            /*inHandItem.transform.SetParent(firstPersonHand.transform, false);
            inHandItem.transform.localPosition = new Vector3(0, -1, 0);
            inHandItem.transform.localRotation = Quaternion.identity;
            isPicked = true;*/
    }
}