using UnityEngine;

public class Player_RayCast : MonoBehaviour
{
    [SerializeField] private float rayCastRange = 5f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform holdPosition;

    private HeldObject heldObject;

    void Update()
    {
        if (heldObject == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, rayCastRange, layerMask))
            {
                var hitObjectInteract = hit.collider.GetComponent<IInteractable>();
                if (hitObjectInteract != null && Input.GetKeyDown(KeyCode.E))
                {
                    hitObjectInteract.interact(); // Küçük harfle çağır
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            heldObject.Drop();
            heldObject = null;
        }
    }

    public void SetHeldObject(HeldObject obj)
    {
        heldObject = obj;
    }
}