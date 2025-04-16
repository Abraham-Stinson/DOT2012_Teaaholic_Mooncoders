using UnityEngine;

public class PlacementPreview : MonoBehaviour
{
    public Material previewMaterial;
    public LayerMask placementLayer;
    public float maxPlacementDistance = 5f;

    private GameObject currentPreview;
    private HeldObject heldObject;

    private Rigidbody rb;
    private Rigidbody itemRB;
    private Collider itemCollider;
    private bool isPick;

    public bool HasValidPlacement { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (heldObject == null) return;

        UpdatePreviewPostition();

        if (Input.GetKeyDown(KeyCode.E) && currentPreview != null && currentPreview.activeSelf)
        {
            PlacementObject();
        }
    }

    public void SetHeldObject(HeldObject obj)
    {
        heldObject = obj;

        itemRB = heldObject.GetComponent<Rigidbody>();
        itemCollider = heldObject.GetComponent<Collider>();

        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        currentPreview = Instantiate(obj.gameObject);
        MakeTransparent(currentPreview);
    }

    private void UpdatePreviewPostition()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementLayer))
        {
            currentPreview.SetActive(true);
            currentPreview.transform.position = hit.point;
            currentPreview.transform.rotation = Quaternion.identity;
            HasValidPlacement = true;
        }
        else
        {
            currentPreview.SetActive(false);
            HasValidPlacement = false;
        }
    }

    private void PlacementObject()
    {
        heldObject.Place(currentPreview.transform.position, Quaternion.identity);

        Destroy(currentPreview);
        currentPreview = null;
        heldObject = null;
    }

    private void MakeTransparent(GameObject previewObj)
    {
        foreach (Renderer r in previewObj.GetComponentsInChildren<Renderer>())
        {
            r.material = previewMaterial;
        }

        foreach (Collider c in previewObj.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        rb.isKinematic = true;
    }

    public void Place(Vector3 position)
    {
        isPick = false;
        transform.parent = null;
        transform.position = position;
        transform.rotation = Quaternion.identity;

        itemRB.isKinematic = false;
        itemCollider.enabled = true;
    }
}
