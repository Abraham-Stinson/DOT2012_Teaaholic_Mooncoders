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

    private Rigidbody itemRB;
    private Collider itemCollider;

    public bool HasValidPlacement { get; private set; }

    void Update()
    {
        if (heldObject == null) return;

        UpdatePreviewPosition();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (HasValidPlacement)
            {
                PlacementObject(); // Sadece geçerli bir yer varsa yerleştir
            }
            else
            {
                // Geçerli yer yoksa hiçbir şey yapma (DropObject çağırma!)
                Debug.Log("Geçerli bir yerleştirme alanı yok!");
            }
        }
    }

    public void SetHeldObject(HeldObject obj)
    {
        heldObject = obj;

        if (heldObject != null)
        {
            itemRB = heldObject.GetComponent<Rigidbody>();
            itemCollider = heldObject.GetComponent<Collider>();
        }

        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        currentPreview = Instantiate(obj.gameObject);
        MakeTransparent(currentPreview);
    }

    private void UpdatePreviewPosition()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxPlacementDistance, placementLayer))
        {
            currentPreview.SetActive(true);

            // Nesnenin boyunun yarısını al (Y ekseninde)
            Bounds bounds = GetObjectBounds(currentPreview);
            Vector3 adjustedPosition = hit.point + new Vector3(0, bounds.extents.y, 0);

            currentPreview.transform.position = adjustedPosition;
            currentPreview.transform.rotation = Quaternion.identity;
            HasValidPlacement = true;

            UpdatePreviewColor(validColor);
        }
        else
        {
            currentPreview.SetActive(true);

            Bounds bounds = GetObjectBounds(currentPreview);
            Vector3 fallbackPosition = ray.origin + ray.direction * 3f + new Vector3(0, bounds.extents.y, 0);

            currentPreview.transform.position = fallbackPosition;
            currentPreview.transform.rotation = Quaternion.identity;
            HasValidPlacement = false;

            UpdatePreviewColor(invalidColor);
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


    private void PlacementObject()
    {
        if (heldObject != null)
        {
            heldObject.Place(currentPreview.transform.position, Quaternion.identity);
            Destroy(currentPreview);
            currentPreview = null;

            // 🔧 Player'a da haber ver ki heldObject null olsun
            GameObject.FindWithTag("Player").GetComponent<Player_RayCast>().ClearHeldObject();

            heldObject = null;
        }
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

    private void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.Drop();

            // 🔧 Player tarafındaki heldObject'u da temizle
            GameObject.FindWithTag("Player").GetComponent<Player_RayCast>().ClearHeldObject();

            heldObject = null;
        }
    }

    private void UpdatePreviewColor(Color color)
    {
        foreach (Renderer r in currentPreview.GetComponentsInChildren<Renderer>())
        {
            r.material.color = color;
        }
    }


}