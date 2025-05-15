using Unity.VisualScripting;
using UnityEngine;

public class HeldObject : MonoBehaviour, IInteractable
{
    public bool isMop = false;
    [Header("References")]
    [SerializeField] private Rigidbody itemRB;
    [SerializeField] private Collider itemCollider;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private float maxPlacementDistance = 5f;

    private Transform originalParent;
    private GameObject previewObject;
    private Renderer[] previewRenderers;
    private Color validColor = new Color(0f, 1f, 0f, 0.5f);
    private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);
    private bool isPicked = false;
    private bool hasValidPlacement = false;

    void Start()
    {
        if (itemRB == null) itemRB = GetComponent<Rigidbody>();
        if (itemCollider == null) itemCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (!isPicked || previewObject == null) return;

        UpdatePreview();

        if (Input.GetMouseButtonDown(1) && hasValidPlacement)
        {
            Place(previewObject.transform.position, previewObject.transform.rotation);
            Destroy(previewObject);
            previewObject = null;
        }
    }

    public void interact()
    {
        if (!isPicked)
        {
            PickUp();
        }
    }

    public void PickUp()
    {
        isPicked = true;

        itemRB.isKinematic = true;
        itemCollider.enabled = false;

        originalParent = transform.parent;
        Transform holdPos = GameObject.FindWithTag("Player").transform.Find("HoldPosition");

        if (holdPos != null)
        {
            transform.parent = holdPos;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            GameObject.FindWithTag("Player").GetComponent<Player_RayCast>().SetHeldObject(this);
            CreatePreview();
        }
        else
        {
            Debug.LogError("HoldPosition not found!");
        }
    }

    public void Drop()
    {
        isPicked = false;
        transform.parent = originalParent;
        itemRB.isKinematic = false;
        itemCollider.enabled = true;

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        gameObject.layer = LayerMask.NameToLayer("Interactable");

        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }

    public void Place(Vector3 position, Quaternion rotation)
    {
        isPicked = false;

        transform.parent = null;
        transform.position = position;
        transform.rotation = rotation;

        itemRB.isKinematic = false;
        itemCollider.enabled = true;

        gameObject.layer = LayerMask.NameToLayer("Interactable");

        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        tag = "Interactable";

        Debug.Log("Yerleştirildi. Layer: " + gameObject.layer);
    }

    private void CreatePreview()
    {
        previewObject = Instantiate(gameObject);
        Destroy(previewObject.GetComponent<HeldObject>());

        foreach (Collider c in previewObject.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        previewRenderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in previewRenderers)
        {
            r.material = new Material(previewMaterial);
            r.material.color = invalidColor;
        }
    }

    private void UpdatePreview()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementLayer))
        {
            Bounds bounds = GetObjectBounds(previewObject);
            Vector3 adjustedPos = hit.point + hit.normal * bounds.extents.y;

            previewObject.transform.position = adjustedPos;
            previewObject.transform.rotation = Quaternion.LookRotation(hit.normal);

            UpdatePreviewColor(validColor);
            hasValidPlacement = true;
        }
        else
        {
            UpdatePreviewColor(invalidColor);
            hasValidPlacement = false;
        }
    }

    private Bounds GetObjectBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    private void UpdatePreviewColor(Color color)
    {
        foreach (Renderer r in previewRenderers)
        {
            r.material.color = color;
        }
    }
}
