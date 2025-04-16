using UnityEngine;

public class Player_RayCast : MonoBehaviour
{
    [SerializeField] private PlacementPreview placementPreview;
    [SerializeField] private float rayCastRange = 5f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform holdPosition;
    

    private HeldObject heldObject;

    void Update()
    {
        if (heldObject == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, rayCastRange, layerMask))
            {
                var hitObjectInteract = hit.collider.GetComponent<IInteractable>();
                if (hitObjectInteract != null && Input.GetKeyDown(KeyCode.E))
                {
                    hitObjectInteract.interact();
                }
            }
        }
        else
        {
            // Eğer bir nesne tutuluyorsa, etkileşim yapılmamalı
        }
    }
    public void SetHeldObject(HeldObject obj)
    {
        placementPreview.SetHeldObject(obj);
        heldObject = obj;
    }

    public void ClearHeldObject()
    {
        heldObject = null;
    }
}