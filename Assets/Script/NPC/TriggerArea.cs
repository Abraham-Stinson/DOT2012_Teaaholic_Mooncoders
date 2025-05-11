using UnityEngine;

public enum TriggerAreaType
{
    Entry,
    Exit,
    Cashier,
    Table
}

public class TriggerArea : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] public TriggerAreaType areaType;
    [SerializeField] private Vector2 areaSize = new Vector2(3f, 3f); // X and Z size of the area
    [SerializeField] public int tableIndex = -1; // -1 for non-table areas
    
    [Header("Debug Visualization")]
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);
    
    public TriggerAreaType AreaType => areaType;
    public int TableIndex => tableIndex;
    
    public Vector3 GetRandomPositionInArea()
    {
        float randomX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float randomZ = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
        
        Vector3 randomPosition = transform.position + new Vector3(randomX, 0f, randomZ);
        return randomPosition;
    }
    
    private void OnDrawGizmos()
    {
        // Draw area visualization in editor
        Gizmos.color = gizmoColor;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(areaSize.x, 0.1f, areaSize.y);
        Gizmos.DrawCube(center, size);
    }
} 