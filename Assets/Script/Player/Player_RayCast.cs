using UnityEngine;

public class Player_RayCast : MonoBehaviour
{
    [SerializeField] private float rayCastRange = 5f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform holdPosition;

    private HeldObject heldObject; // Elinde tutulan objeyi takip etmek için

    void Update()
    {
        if (heldObject == null) // Eğer elinde obje yoksa Raycast yap
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, rayCastRange, layerMask))
            {
                var hitObjectInteract = hit.collider.GetComponent<IInteractable>();
                if (hitObjectInteract != null && Input.GetKeyDown(KeyCode.E))
                {
                    hitObjectInteract.interact();
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E)) // Eğer elinde obje varsa bırak
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
