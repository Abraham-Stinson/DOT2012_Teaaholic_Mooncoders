using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private NPCManager npcManager;
    [SerializeField] private TableManager tableManager;
    
    [Header("Game Settings")]
    [SerializeField] private GameObject[] gameObjects; // Backgammon, Cards, Okey
    [SerializeField] private Transform[] gameSpawnPoints; // Where games appear when not in use
    
    [Header("Customer Statistics")]
    [SerializeField] private int totalCustomersServed = 0;
    [SerializeField] private int totalMoneyEarned = 0;
    [SerializeField] private int totalCustomersLost = 0;
    
    // Singleton pattern
    private static CustomerManager _instance;
    public static CustomerManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        
        // Find managers if not assigned
        if (npcManager == null)
        {
            npcManager = FindObjectOfType<NPCManager>();
        }
        
        if (tableManager == null)
        {
            tableManager = FindObjectOfType<TableManager>();
        }
    }
    
    private void Start()
    {
        // Spawn board games at their designated positions
        //SpawnGames();
    }
    
    /*private void SpawnGames()
    {
        if (gameObjects.Length != gameSpawnPoints.Length)
        {
            Debug.LogError("Game objects and spawn points arrays must have the same length!");
            return;
        }
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] != null && gameSpawnPoints[i] != null)
            {
                Instantiate(gameObjects[i], gameSpawnPoints[i].position, gameSpawnPoints[i].rotation);
            }
        }
    }*/
    
    // Called by the NPCGroup when payments are processed
    public void AddPayment(float amount)
    {
        totalMoneyEarned += Mathf.RoundToInt(amount);
        totalCustomersServed++;
    }
    
    // Called when customers leave without paying
    public void AddLostCustomer()
    {
        totalCustomersLost++;
    }
    
    // For UI display
    public int GetTotalCustomersServed()
    {
        return totalCustomersServed;
    }
    
    public int GetTotalMoneyEarned()
    {
        return totalMoneyEarned;
    }
    
    public int GetTotalCustomersLost()
    {
        return totalCustomersLost;
    }
} 