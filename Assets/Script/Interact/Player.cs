using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerCam;
    [SerializeField] [Min(1)] private float rayCastRange =10f;
    [SerializeField] private bool isPicked=false; //aaa
    [Header("UI")]
    [SerializeField] private GameObject pickUpUI;
    [SerializeField] private GameObject putDownUI;
    [SerializeField] private GameObject useUI;
    [SerializeField] private GameObject pourUI;
    [SerializeField] private GameObject putOnTray;
    
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
    [SerializeField] private InputActionReference pickUpInput, putDownInput, useInput;
    void Start()
    {
        pickUpInput.action.performed+=PickUp;
        putDownInput.action.performed+=PutDown;
        useInput.action.performed+=Use;
    }

    private void Use(InputAction.CallbackContext context)
    {
        if(hit.collider!=null){
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange,useableLayer)&&!isPicked){//INTERACT SİSTEMİ
                hit.collider.GetComponent<IInteractable>().interact();
            }

            else if(inHandItem!=null&&inHandItem.tag=="Kettle"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&&isPicked){//ÇAY DÖKME SİSTEMİ
                if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                    if(hit.collider.CompareTag("Tea_Cup")){
                        hit.collider.GetComponent<Tea_Cup>().AddTea();
                        inHandItem.GetComponent<Kettle>().PourTea();
                    }
                }
            }

        }

    }

    private void PutDown(InputAction.CallbackContext context)
    {
        
        if(hit.collider!=null){
            if (Physics.Raycast(playerCam.position,playerCam.forward,out hit, rayCastRange)&&isPicked){//TEPSİ SİSTEMİ
                if(inHandItem.gameObject.tag=="Tea_Cup"&&hit.collider.gameObject.tag=="Tray"){
                    Debug.Log("aaaaaa");
                    isPicked=false;
                    if(inHandItem.GetComponent<Tea_Cup>()!=null&&!inHandItem.GetComponent<Tea_Cup>().isOnTray){
                        Debug.Log("Elinde bardakla Tepsiye bakıyor");
                        inHandItem.GetComponent<Tea_Cup>().isOnTray=true;
                        Transform trayTransform = hit.collider.transform.root;
                        inHandItem.transform.SetParent(trayTransform,true);
                        //inHandItem.transform.position=Vector3.zero;
                        inHandItem.transform.rotation=Quaternion.identity;
                    }
                    /*else if(DİĞER BARDAKLAR EKLENEBİLİR)*/
                    else{
                        Debug.Log("YOOOOOK");
                    }

                    Collider col = inHandItem.GetComponent<Collider>();
                    if (col != null)
                    {
                        float heightOffset = col.bounds.extents.y;
                        inHandItem.transform.position = hit.point + Vector3.up * heightOffset;
                    }
                    else
                    {
                        inHandItem.transform.position = hit.point;
                    }
                    
                    //inHandItem.transform.SetParent(null);
                    
                    Rigidbody rb = inHandItem.GetComponent<Rigidbody>();
                    if(rb!=null){
                        rb.useGravity=true;
                        rb.isKinematic=false;
                    } 
                    inHandItem=null;
                    return;
                }
            }
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange,placementLayer)&&isPicked){   //BIRAKMA SİSTEMİ  
                isPicked=false;
                Collider col = inHandItem.GetComponent<Collider>();
                if (col != null)
                {
                    float heightOffset = col.bounds.extents.y;
                    inHandItem.transform.position = hit.point + Vector3.up * heightOffset;
                }
                else
                {
                    inHandItem.transform.position = hit.point;
                }
                inHandItem.transform.SetParent(null);
                
                Rigidbody rb = inHandItem.GetComponent<Rigidbody>();
                if(rb!=null){
                    rb.useGravity=true;
                    rb.isKinematic=false;
                } 
                inHandItem=null;
                return;
            }
            
        }
    }

    private void PickUp(InputAction.CallbackContext context)//PİCK UP
    {
        //Debug.Log(hit.collider.gameObject.name);
        if(hit.collider!=null){
            
            
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange,interactionLayer)&&!isPicked){
                isPicked=true;
                if(hit.collider.GetComponent<Tea_Cup>()!=null&&hit.collider.GetComponent<Tea_Cup>().isOnTray){//PİCK UP TEPSİDEN
                    
                    hit.collider.GetComponent<Tea_Cup>().isOnTray=false;
                    inHandItem=hit.collider.gameObject;
                    //inHandItem.transform.position=Vector3.zero;
                    inHandItem.transform.rotation=Quaternion.identity;
                    inHandItem.transform.SetParent(firstPersonHand.transform,false);

                    
                    
                }
                /*else if(DİĞER BARDAKLAR EKLENEBİLİR)*/
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
            }
        }

    }

    void Update()
    {
        /*if(inHandItem!=null){
            Debug.Log("Elimde: "+inHandItem.gameObject.name);
        }
        
        if(hit.collider!=null){
            Debug.Log("Görüyorum: "+hit.collider.gameObject.name);
        }*/
        Debug.DrawRay(playerCam.position, playerCam.forward * rayCastRange, Color.red);

        bool didHit = Physics.Raycast(playerCam.position, playerCam.forward, out hit, rayCastRange);

        if (lastHighlightedObject != null && (didHit == false || hit.collider.gameObject != lastHighlightedObject))
        {
            lastHighlightedObject.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
        }

        pickUpUI.SetActive(false);
        putDownUI.SetActive(false);
        useUI.SetActive(false);
        pourUI.SetActive(false);
        putOnTray.SetActive(false);

        if (didHit && ((1 << hit.collider.gameObject.layer) & interactionLayer.value) != 0 && !isPicked)
        {
            hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
            lastHighlightedObject = hit.collider.gameObject;
            pickUpUI.SetActive(true);
        }
        else if (didHit && ((1 << hit.collider.gameObject.layer) & placementLayer.value) != 0 && isPicked)
        {
            hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
            putDownUI.SetActive(true);
        }
        else if (didHit && ((1 << hit.collider.gameObject.layer) & useableLayer.value) != 0 && !isPicked)
        {
            hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
            lastHighlightedObject = null;
            useUI.SetActive(true);
        }
        else if(didHit && inHandItem!=null&&inHandItem.tag=="Kettle"/*&&hit.collider.gameObject.tag=="Tea_Cup"*/&&isPicked){
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                if(hit.collider.CompareTag("Tea_Cup")){
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = null;
                    pourUI.SetActive(true);
                }
            }
        }
        else if(didHit/*&&(inHandItem.tag=="Tea_Cup"/*BURAYA DİĞER BARDAKLARDA GELEBİLİR)*/&&isPicked){
            if(Physics.Raycast(playerCam.position,playerCam.forward,out hit,rayCastRange)){
                if(inHandItem.gameObject.tag=="Tea_Cup"&&!inHandItem.GetComponent<Tea_Cup>().isOnTray&&hit.collider.gameObject.tag=="Tray")
                {
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(true);
                    lastHighlightedObject = null;
                    putOnTray.SetActive(true);
                    Debug.Log("Parlıyorum");
                }
                else{
                    Debug.Log("Söndüm");
                    hit.collider.GetComponent<HighLight>()?.ToggleHighLight(false);
                    lastHighlightedObject = null;
                    putOnTray.SetActive(false);
                }
            }

        }
    }
}