using UnityEngine;

public class Player_RayCast : MonoBehaviour
{
    [SerializeField] private float rayCastRange = 5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform holdPosition;

    private HeldObject heldObject;

    void Update()
    {
        if (heldObject == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, rayCastRange, interactableLayer))
            {
                Debug.Log("Ray hit: " + hit.collider.gameObject.name); // Debug ekleyelim

                var hitObjectInteract = hit.collider.GetComponent<IInteractable>();
                if (hitObjectInteract != null && Input.GetKeyDown(KeyCode.E)) // E tuşuna basıldığında etkileşim
                {
                    hitObjectInteract.interact(); // objeyi etkile
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E)) // E tuşuna basılınca objeyi bırak
            {
                heldObject.Drop();
                ClearHeldObject();
            }
        }
    }

    public void SetHeldObject(HeldObject obj)
    {
        heldObject = obj;
    }

    public void ClearHeldObject()
    {
        heldObject = null;
    }
}
