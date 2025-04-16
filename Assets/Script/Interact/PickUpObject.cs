using UnityEngine;

public class HeldObject : MonoBehaviour, IInteractable
{
    
    [Header("Pick Up")]
    [SerializeField] private Rigidbody itemRB;
    [SerializeField] private Collider itemCollider;

    private bool isPick = false;
    private Transform originalParent;

    void Start()
    {
        itemRB = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }
    public void interact()
    {
        if (!isPick)
        {
            PickUp();
        }
    }

    public void PickUp()
    {
        isPick = true;
        itemRB.isKinematic = true;
        itemCollider.enabled = false;
    

        originalParent = transform.parent; // Orijinal parent'ı sakla
        Transform holdPosition = GameObject.FindWithTag("Player").transform.Find("HoldPosition");
        if (holdPosition != null)
        {
            transform.parent = holdPosition;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            GameObject.FindWithTag("Player").GetComponent<Player_RayCast>().SetHeldObject(this);
        }
        else
        {
            Debug.LogError("HoldPosition not found!");
        }
    }

    public void Drop()
    {
        isPick = false;
        transform.parent = originalParent; // Orijinal parent'a geri dön
        itemRB.isKinematic = false;
        itemCollider.enabled = true;
    }
    public void Place(Vector3 position, Quaternion rotation)
    {
        isPick = false; // Artık elde değil
        transform.parent = null; // Parent'tan çıkar
        itemRB.isKinematic = false;
        itemCollider.enabled = true;

        transform.position = position;
        transform.rotation = rotation;
    }
}