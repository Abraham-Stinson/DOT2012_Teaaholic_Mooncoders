using System;
using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerCam;
    [SerializeField] [Min(1)] private float rayCastRange =10f;
    [SerializeField] private bool isPicked=false; //aaa
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
        pickAndPutInput.action.performed+=PickAndPut;
        //putDownInput.action.performed+=PutDown;
        useInput.action.performed+=Use;
        useHoldInput.action.performed+=UseHold;
    }

#region USE HOLD (THRASH)
    private void UseHold(InputAction.CallbackContext context)
    {
        if (inHandItem != null && inHandItem.CompareTag("Mop") && hit.collider.gameObject.tag=="Trash")
            {
                Destroy(hit.collider.gameObject);
            }
    }
#endregion
#region USE INPUT
    private void Use(InputAction.CallbackContext context)//F
    {
        if(!Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange)){
            return;
        }
        GameObject target = hit.collider.gameObject;

        if(hit.collider!=null){
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange,useableLayer)&&!isPicked&&hit.collider.GetComponent<IInteractable>()!=null){//INTERACT SİSTEMİ
                hit.collider.GetComponent<IInteractable>().interact();
            }

            else if(inHandItem!=null&&inHandItem.tag=="Kettle"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&&isPicked){//ÇAY DÖKME SİSTEMİ
                if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                    if(target.CompareTag("Tea_Cup")&&target.GetComponent<Tea_Cup>().isFullTea==false&&inHandItem.GetComponent<Kettle>().currentKettleMagazine>0){
                        hit.collider.GetComponent<Tea_Cup>().AddTea();
                        inHandItem.GetComponent<Kettle>().PourTea();
                        Debug.Log("Çay eklendi");
                    }
                    else{
                        Debug.Log("Dolduramazsın");
                    }
                }
            }
            else if(inHandItem!=null&&inHandItem.tag=="Tea_Cup"){
                if (Physics.Raycast(playerCam.position,playerCam.forward,out hit, rayCastRange)){
                    if(target.CompareTag("Hot_Water")){
                        if(inHandItem.GetComponent<Tea_Cup>().isFillTea){
                            inHandItem.GetComponent<Tea_Cup>().FillHotWater();
                        }
                        else{
                            Debug.Log("Sıcak su dolduramazsın Dolduramazsın");
                        }
                    }
                }
            }
            else if(inHandItem!=null&&inHandItem.GetComponent<DirtyStatus>()!=null/*DİĞER BARDAKLARDA OLABİLİR*/&&isPicked){
                if(Physics.Raycast(playerCam.position,playerCam.forward,out hit, rayCastRange)){
                    if(target.CompareTag("Water")){
                        Debug.Log("Water'ı gördü");
                        var isDirtyinHandItem = inHandItem.GetComponent<DirtyStatus>();
                        if(isDirtyinHandItem.isDirty){
                            
                            isDirtyinHandItem.CleanDirt();
                        }
                    }
                }
            }
            

        }
    }
#endregion
#region pick and put and tray
    private void PickAndPut(InputAction.CallbackContext context)//E
    {

        if(!Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange)){
            return;
        }
        GameObject target = hit.collider.gameObject;
        if(isPicked){
            //TEPSİ SİSTEMİ
            if(inHandItem.gameObject.tag=="Tea_Cup"&&target.tag=="Tray"){
                Tea_Cup teaCup = inHandItem.GetComponent<Tea_Cup>();
                if(teaCup!=null && !teaCup.isOnTray){
                    teaCup.isOnTray=true;
                    isPicked=false;
                    //Tepsiye sabitle
                    Transform trayTransform = hit.collider.transform.root;
                    inHandItem.transform.SetParent(trayTransform,true);
                    inHandItem.transform.rotation=Quaternion.identity;

                    SetItemPositionOnSurface(inHandItem, hit.point);

                    EnablePhysics(inHandItem,true);

                    DisablePhysics();

                    inHandItem=null;
                    return;
                }
            }
            
            if(((1<<target.layer)&placementLayer)!=0){//BIRAKMA SİSTEMİ  
                isPicked=false;

                inHandItem.transform.SetParent(null);
                SetItemPositionOnSurface(inHandItem, hit.point);
                
                EnablePhysics(inHandItem,true);
                inHandItem=null;
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

        else{
            if(((1<<target.layer)&interactionLayer)!=0){
                Tea_Cup teaCup=target.GetComponent<Tea_Cup>();

                if(teaCup!=null && teaCup.isOnTray){
                    teaCup.isOnTray=false;
                }

                isPicked = true;
                inHandItem = target;
                inHandItem.transform.SetParent(firstPersonHand.transform, false);
                inHandItem.transform.localPosition = Vector3.zero;
                inHandItem.transform.localRotation = Quaternion.identity;

                EnablePhysics(inHandItem, false);
            }
        }
    }
#endregion
    void Update()
    {

        UpdateUIAndHighlight();
        Debug.DrawRay(playerCam.position, playerCam.forward * rayCastRange, Color.red);
    }
#region UpdateUI
    void UpdateUIAndHighlight(){
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
            ShowUIMessage("Press E to Pick Up");
        }
         if (didHit && ((1 << hit.collider.gameObject.layer) & placementLayer.value) != 0 && isPicked)
        {
            hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
            ShowUIMessage("Press E to Put Down");
        }
         if (didHit && ((1 << hit.collider.gameObject.layer) & useableLayer.value) != 0 && !isPicked&&hit.collider.GetComponent<IInteractable>()!=null)
        {
            hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = hit.collider.gameObject;
            ShowUIMessage("Press F to Use");
        }

         if(didHit/*&&(inHandItem.tag=="Tea_Cup"/*BURAYA DİĞER BARDAKLARDA GELEBİLİR)*/&&isPicked){
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                if(inHandItem.gameObject.tag=="Tea_Cup"&&!inHandItem.GetComponent<Tea_Cup>().isOnTray&&hit.collider.gameObject.tag=="Tray")
                {
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("Press E to Put on Tray");
                }
                else{
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                    lastHighlightedObject = null;
                }
            }
        }

        if(didHit && inHandItem!=null&&inHandItem.tag=="Kettle"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&&isPicked){//KETTLE DAN ÇAY KOYMA UI
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                if(hit.collider.CompareTag("Tea_Cup")){
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("Press F to Pour Tea");
                }
            }
        }

        if(didHit && inHandItem!=null&&inHandItem.tag=="Tea_Cup"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&&isPicked){//KETTLE DAN ÇAY KOYMA UI
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                if(hit.collider.CompareTag("Hot_Water")){
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("Press F to Fill the Hot Water");
                }
            }
        }
        
         if(didHit && inHandItem!=null&&inHandItem.GetComponent<DirtyStatus>()!=null&&isPicked){
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                if(hit.collider.CompareTag("Water")&&inHandItem.GetComponent<DirtyStatus>().isDirty){
                    lastHighlightedObject = hit.collider.gameObject;
                    ShowUIMessage("Press F to Wash");
                }
            }
        }
         if(didHit&&hit.collider.gameObject.tag=="Trash"){//THRASH UI
            if(inHandItem!=null&&inHandItem.gameObject.tag=="Mop"){
                    ShowUIMessage("Hold the F to Clean Trash");
            }
            else{
                ShowUIMessage("You need mop to clean thrash");
            }

            hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
            lastHighlightedObject = hit.collider.gameObject;
        }
    }
#endregion

    void ShowUIMessage(string message){
        mainInfoUI.SetActive(true);
        mainInfoUIText.text=message;
    }

    private void SetItemPositionOnSurface(GameObject item, Vector3 hitPoint){
        Collider col = item.GetComponent<Collider>();
        if(col!=null){
            float heightOffset = col.bounds.extents.y;
            item.transform.position=hitPoint+Vector3.up* heightOffset;
        }

        else{
            item.transform.position=hitPoint;
        }
    }

    private void EnablePhysics(GameObject item, bool enable){
        Rigidbody rb = item.GetComponent<Rigidbody>();

        if(rb!=null){
            rb.useGravity=enable;
            rb.isKinematic=!enable;
        }
    }
    private void DisablePhysics(){
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
}