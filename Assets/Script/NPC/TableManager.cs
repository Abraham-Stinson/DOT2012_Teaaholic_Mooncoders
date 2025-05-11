using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    [SerializeField] private Table[] tables;

    private void Awake()
    {
        // If tables are not assigned in inspector, find them automatically
        if (tables == null || tables.Length == 0)
        {
            tables = FindObjectsOfType<Table>();
        }
    }

    public Table FindAvailableTable(int groupSize)
    {
        foreach (Table table in tables)
        {
            if (table.IsAvailable && table.Capacity >= groupSize)
            {
                return table;
            }
        }
        
        return null;
    }

    public void ClaimTable(Table table, NPCGroup group)
    {
        table.Claim(group);
    }

    public void ReleaseTable(Table table)
    {
        table.Release();
    }
} 