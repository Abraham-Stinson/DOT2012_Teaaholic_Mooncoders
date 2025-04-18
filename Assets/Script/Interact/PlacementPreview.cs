using UnityEngine;

public class PlacementPreview : MonoBehaviour
{
    private Renderer[] previewRenderers;
    private Color validColor = new Color(0f, 1f, 0f, 0.5f);   // Yeşil, yarı şeffaf
    private Color invalidColor = new Color(1f, 0f, 0f, 0.5f);

    public Material previewMaterial;
    public LayerMask placementLayer;
    public float maxPlacementDistance = 5f;

    private GameObject currentPreview;
    private HeldObject heldObject;

    public bool HasValidPlacement { get; private set; }

    void Update()
    {
        if (heldObject == null) return;

        if (currentPreview != null)
        {
            currentPreview.SetActive(true);
        }
    }

    public void SetHeldObject(HeldObject obj)
    {
        heldObject = obj;

        if (heldObject != null)
        {
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            currentPreview = Instantiate(obj.gameObject);
            MakeTransparent(currentPreview);
        }
    }

    public void UpdatePreviewPosition(Vector3 position, Vector3 normal)
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, maxPlacementDistance, placementLayer))
        {
            Bounds bounds = GetObjectBounds(currentPreview);
            Vector3 adjustedPosition = hit.point + normal * bounds.extents.y;

            currentPreview.transform.position = adjustedPosition;
            currentPreview.transform.rotation = Quaternion.LookRotation(normal);
            HasValidPlacement = true;
            UpdatePreviewColor(validColor);
        }
        else
        {
            HasValidPlacement = false;
            UpdatePreviewColor(invalidColor);
        }
    }

    public void PlaceObject()
    {
        if (heldObject != null)
        {
            heldObject.Place(currentPreview.transform.position, currentPreview.transform.rotation);
            ClearPreview();
        }
    }

    public void ClearPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
        heldObject = null;
    }

    private void MakeTransparent(GameObject previewObj)
    {
        previewRenderers = previewObj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in previewRenderers)
        {
            r.material = new Material(previewMaterial); // Instance yarat
            r.material.color = invalidColor; // İlk başta geçersiz say
        }

        foreach (Collider c in previewObj.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
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
        foreach (Renderer r in currentPreview.GetComponentsInChildren<Renderer>())
        {
            r.material.color = color;
        }
    }

    public void HidePreview()
    {
        if (currentPreview != null)
        {
            currentPreview.SetActive(false);
        }
    }
}
