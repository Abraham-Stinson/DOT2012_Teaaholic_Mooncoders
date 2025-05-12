using UnityEngine;

public class Chair : MonoBehaviour
{
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private Transform sittingPosition;
    [SerializeField] private Transform sittingLookAt; // NPC'nin bakacağı yön
    
    public bool IsOccupied
    {
        get { return isOccupied; }
    }
    
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }
    
    public Transform GetSittingPosition()
    {
        return sittingPosition != null ? sittingPosition : transform;
    }
    
    public Transform GetSittingLookAt()
    {
        return sittingLookAt != null ? sittingLookAt : transform;
    }
    
    public Vector3 GetSittingPositionVector()
    {
        return sittingPosition != null ? sittingPosition.position : transform.position;
    }
    
    public Quaternion GetSittingRotation()
    {
        if (sittingLookAt != null)
        {
            Vector3 direction = sittingLookAt.position - transform.position;
            direction.y = 0; // Y ekseninde dönmeyi engelle
            return Quaternion.LookRotation(direction);
        }
        return transform.rotation;
    }
} 