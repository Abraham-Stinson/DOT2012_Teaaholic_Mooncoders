using UnityEngine;

/// <summary>
/// Represents a chair that NPCs can sit on
/// </summary>
public class Chair : MonoBehaviour
{
    [Header("Chair Settings")]
    [SerializeField] private Transform sitPosition; // Where the NPC should sit
    [SerializeField] private Transform tablePosition; // Position on the table in front of this chair
    [SerializeField] private Transform cupPosition; // Position for the cup on the table
    [SerializeField] private bool isOccupied = false;
    
    private TableController parentTable;
    
    private void Start()
    {
        // If sit position is not set, use this transform
        if (sitPosition == null)
        {
            GameObject sitPosObj = new GameObject("SitPosition");
            sitPosObj.transform.SetParent(transform);
            sitPosObj.transform.localPosition = Vector3.zero;
            sitPosObj.transform.localRotation = Quaternion.identity;
            sitPosition = sitPosObj.transform;
        }
        
        // If table position is not set, create one
        if (tablePosition == null)
        {
            GameObject tablePosObj = new GameObject("TablePosition");
            tablePosObj.transform.SetParent(transform);
            // Position in front of the chair, towards the table
            tablePosObj.transform.localPosition = transform.forward * 0.5f;
            tablePosObj.transform.localRotation = Quaternion.identity;
            tablePosition = tablePosObj.transform;
        }
        
        // If cup position is not set, create one
        if (cupPosition == null)
        {
            GameObject cupPosObj = new GameObject("CupPosition");
            cupPosObj.transform.SetParent(transform);
            // Position the cup slightly to the right of the table position
            // and slightly elevated to be on the table
            cupPosObj.transform.localPosition = transform.forward * 0.6f + transform.right * 0.2f;
            cupPosObj.transform.localRotation = Quaternion.identity;
            cupPosition = cupPosObj.transform;
        }
    }
    
    /// <summary>
    /// Get the position where an NPC should sit
    /// </summary>
    public Transform GetSitPosition()
    {
        return sitPosition;
    }
    
    /// <summary>
    /// Get the position on the table in front of this chair
    /// </summary>
    public Transform GetTablePosition()
    {
        return tablePosition;
    }
    
    /// <summary>
    /// Get the position where a cup should be placed for this chair
    /// </summary>
    public Transform GetCupPosition()
    {
        return cupPosition;
    }
    
    /// <summary>
    /// Set the parent table for this chair
    /// </summary>
    public void SetTable(TableController table)
    {
        parentTable = table;
    }
    
    /// <summary>
    /// Get the parent table
    /// </summary>
    public TableController GetTable()
    {
        return parentTable;
    }
    
    /// <summary>
    /// Set whether this chair is occupied
    /// </summary>
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }
    
    /// <summary>
    /// Check if this chair is occupied
    /// </summary>
    public bool IsOccupied()
    {
        return isOccupied;
    }
    
    /// <summary>
    /// Visualize the sit position and table position in the editor
    /// </summary>
    private void OnDrawGizmos()
    {
        if (sitPosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(sitPosition.position, 0.1f);
        }
        
        if (tablePosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(tablePosition.position, 0.05f);
        }
        
        if (cupPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(cupPosition.position, 0.03f);
        }
    }
} 