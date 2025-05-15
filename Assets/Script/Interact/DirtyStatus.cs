using UnityEngine;

public class DirtyStatus : MonoBehaviour
{
    public bool isDirty = false;
    [SerializeField] private Material dirtyMaterial;
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Renderer cupRenderer;
    
    private void Start()
    {
        // Find renderer if not assigned
        if (cupRenderer == null)
        {
            cupRenderer = GetComponentInChildren<Renderer>();
        }
        
        // Load materials if not assigned
        if (dirtyMaterial == null)
        {
            dirtyMaterial = Resources.Load<Material>("dirty_glass");
        }
        
        if (cleanMaterial == null)
        {
            cleanMaterial = cupRenderer?.material;
        }
    }
    
    private void Update()
    {
        // Update visual appearance based on dirty status
        if (cupRenderer != null)
        {
            if (isDirty && cupRenderer.material != dirtyMaterial && dirtyMaterial != null)
            {
                cupRenderer.material = dirtyMaterial;
            }
            else if (!isDirty && cupRenderer.material != cleanMaterial && cleanMaterial != null)
            {
                cupRenderer.material = cleanMaterial;
            }
        }
    }
    
    public void CleanDirt()
    {
        Debug.Log("Yıkandı");
        if(GetComponent<Tea_Cup>() != null)
        {
            GetComponent<Tea_Cup>().EmptyCup();
        }
        if(GetComponent<Kettle>() != null)
        {
            GetComponent<Kettle>().EmptyKettle();
            Debug.Log("Kettle yıkandı");
        }
        isDirty = false;
    }
}
