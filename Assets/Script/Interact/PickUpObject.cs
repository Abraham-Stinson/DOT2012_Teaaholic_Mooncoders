using UnityEngine;

public class HeldObject : MonoBehaviour, IInteractable
{
    
    [Header("Pick Up")]
    [SerializeField] private Rigidbody itemRB;
    [SerializeField] private Collider itemCollider;

    private bool isPick = false;
    private Transform originalParent;

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
        transform.parent = GameObject.FindWithTag("Player").transform.Find("HoldPosition");
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        GameObject.FindWithTag("Player").GetComponent<Player_RayCast>().SetHeldObject(this);
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
        transform.position = position;
        transform.rotation = rotation;
    }
}
