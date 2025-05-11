using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
    public Transform[] chairPositions;
    [Header("Game Placement")]
    [Tooltip("Transform position where games will be placed on this table")]
    public Transform gamePlacementPosition;
    [Tooltip("Height above the game placement position")]
    public float gameHeightOffset = 0.1f;
    
    private bool[] chairOccupied;
    private bool isOccupied = false;
    
    void Awake()
    {
        // Initialize chair occupancy array
        chairOccupied = new bool[chairPositions.Length];
        for (int i = 0; i < chairOccupied.Length; i++)
        {
            chairOccupied[i] = false;
        }
        
        // If game placement position is not set, create one at table center
        if (gamePlacementPosition == null)
        {
            GameObject placementPoint = new GameObject("GamePlacementPoint");
            placementPoint.transform.parent = this.transform;
            placementPoint.transform.position = new Vector3(
                transform.position.x,
                transform.position.y + 1.0f, // Default height above table
                transform.position.z
            );
            gamePlacementPosition = placementPoint.transform;
        }
    }
    
    public bool IsEmpty()
    {
        return !isOccupied;
    }
    
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        
        if (!occupied)
        {
            // Clear chair occupancy when table is released
            for (int i = 0; i < chairOccupied.Length; i++)
            {
                chairOccupied[i] = false;
            }
        }
    }
    
    public int ChairCount()
    {
        return chairPositions.Length;
    }
    
    public Transform GetAvailableChair()
    {
        for (int i = 0; i < chairPositions.Length; i++)
        {
            if (!chairOccupied[i])
            {
                return chairPositions[i];
            }
        }
        
        return null;
    }
    
    public void OccupyChair(Transform chair)
    {
        for (int i = 0; i < chairPositions.Length; i++)
        {
            if (chairPositions[i] == chair)
            {
                chairOccupied[i] = true;
                break;
            }
        }
    }
    
    public void ReleaseChair(Transform chair)
    {
        for (int i = 0; i < chairPositions.Length; i++)
        {
            if (chairPositions[i] == chair)
            {
                chairOccupied[i] = false;
                break;
            }
        }
    }
    
    // Gets the position where a game should be placed on this table
    public Vector3 GetGamePosition()
    {
        if (gamePlacementPosition != null)
        {
            return gamePlacementPosition.position + Vector3.up * gameHeightOffset;
        }
        
        // Fallback to table center if placement position is not set
        return new Vector3(
            transform.position.x,
            transform.position.y + 1.0f + gameHeightOffset,
            transform.position.z
        );
    }
}