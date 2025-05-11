using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroupState
{
    Entering,
    SeekingTable,
    WaitingForGame,
    PlayingGame,
    OrderingDrinks,
    WaitingForDrinks,
    Enjoying,
    Leaving,
    AtCashier,
    ExitingVenue
}

public class NPCGroup : MonoBehaviour
{
    [Header("Patience Settings")]
    [SerializeField] private float gameWaitPatience = 60f;
    [SerializeField] private float drinkWaitPatience = 90f;
    [SerializeField] private float cashierWaitPatience = 45f;
    [SerializeField] private float enjoymentDuration = 180f;
    [SerializeField] [Range(0, 100)] private int chanceToOrderAgain = 40;

    [Header("Game Preferences")]
    [SerializeField] [Range(0, 100)] private int backgammonVsCardsChance = 50;

    private List<NPC> groupMembers = new List<NPC>();
    private NPC groupLeader;
    private Table assignedTable;
    private int groupSize;

    private TriggerArea entryArea;
    private TriggerArea exitArea;
    private TriggerArea cashierArea;
    private Animator doorAnimator;
    private TableManager tableManager;

    private GroupState currentState;
    private float stateTimer;
    private string requiredGame;
    private Dictionary<NPC, string> drinkOrders = new Dictionary<NPC, string>();
    private Dictionary<NPC, bool> drinksServed = new Dictionary<NPC, bool>();
    private bool allDrinksServed = false;

    public GroupState CurrentState => currentState;

    public void Initialize(int size, TriggerArea entry, TriggerArea exit, TriggerArea cashier, Animator door, TableManager tableMgr)
    {
        groupSize = size;
        entryArea = entry;
        exitArea = exit;
        cashierArea = cashier;
        doorAnimator = door;
        tableManager = tableMgr;
        currentState = GroupState.Entering;
    }

    public void AddNPC(NPC npc)
    {
        groupMembers.Add(npc);
        if (npc.IsGroupLeader)
        {
            groupLeader = npc;
        }
    }

    public void StartGroupBehavior()
    {
        StartCoroutine(GroupStateMachine());
    }

    private IEnumerator GroupStateMachine()
    {
        // Initial movement to entry area
        foreach (NPC npc in groupMembers)
        {
            Vector3 targetPos = entryArea.GetRandomPositionInArea();
            npc.MoveTo(targetPos);
        }

        // Wait for all NPCs to reach entry area
        yield return StartCoroutine(WaitForAllNPCsToReachDestination());

        // Play door animation
        doorAnimator.SetTrigger("MainDoor");
        yield return new WaitForSeconds(1.0f);

        // Change state to seeking table
        ChangeState(GroupState.SeekingTable);

        while (true)
        {
            switch (currentState)
            {
                case GroupState.SeekingTable:
                    yield return HandleSeekingTableState();
                    break;
                
                case GroupState.WaitingForGame:
                    yield return HandleWaitingForGameState();
                    break;
                
                case GroupState.PlayingGame:
                    yield return HandlePlayingGameState();
                    break;
                
                case GroupState.OrderingDrinks:
                    yield return HandleOrderingDrinksState();
                    break;
                
                case GroupState.WaitingForDrinks:
                    yield return HandleWaitingForDrinksState();
                    break;
                
                case GroupState.Enjoying:
                    yield return HandleEnjoyingState();
                    break;
                
                case GroupState.Leaving:
                    yield return HandleLeavingState();
                    break;
                
                case GroupState.AtCashier:
                    yield return HandleAtCashierState();
                    break;
                
                case GroupState.ExitingVenue:
                    // Move group to random positions in exit area
                    foreach (NPC npc in groupMembers)
                    {
                        Vector3 exitPos = exitArea.GetRandomPositionInArea();
                        npc.MoveTo(exitPos);
                    }
                    
                    yield return StartCoroutine(WaitForAllNPCsToReachDestination());
                    doorAnimator.SetTrigger("MainDoor");
                    yield return new WaitForSeconds(1.0f);
                    
                    // Cleanup and destroy group
                    foreach (NPC npc in groupMembers)
                    {
                        Destroy(npc.gameObject);
                    }
                    Destroy(gameObject);
                    yield break;
            }
            
            yield return null;
        }
    }

    private IEnumerator HandleSeekingTableState()
    {
        // Find available table
        assignedTable = tableManager.FindAvailableTable(groupSize);
        
        if (assignedTable == null)
        {
            // No table available, leave the venue
            ChangeState(GroupState.ExitingVenue);
            yield break;
        }
        
        // Claim the table
        tableManager.ClaimTable(assignedTable, this);
        
        // Direct NPCs to their seats
        for (int i = 0; i < groupMembers.Count; i++)
        {
            NPC npc = groupMembers[i];
            Transform chair = assignedTable.GetChairTransform(i);
            npc.MoveTo(chair.position);
        }
        
        // Wait for all NPCs to reach their chairs
        yield return StartCoroutine(WaitForAllNPCsToReachDestination());
        
        // All NPCs sit down
        foreach (NPC npc in groupMembers)
        {
            npc.Sit();
        }
        
        yield return new WaitForSeconds(1.0f);
        
        // Determine which game to request based on group size
        if (groupSize == 2)
        {
            requiredGame = (Random.Range(0, 100) < backgammonVsCardsChance) ? "Backgammon" : "Cards";
        }
        else
        {
            requiredGame = "Okey";
        }
        
        // Request game through table
        assignedTable.RequestGame(requiredGame);
        
        // Change state to waiting for game
        ChangeState(GroupState.WaitingForGame);
    }

    private IEnumerator HandleWaitingForGameState()
    {
        stateTimer = gameWaitPatience;
        
        while (stateTimer > 0 && !assignedTable.HasGame(requiredGame))
        {
            stateTimer -= Time.deltaTime;
            yield return null;
        }
        
        if (stateTimer <= 0)
        {
            // Patience ran out, leave
            ChangeState(GroupState.Leaving);
            yield break;
        }
        
        // Game was placed, change to playing state
        ChangeState(GroupState.PlayingGame);
    }

    private IEnumerator HandlePlayingGameState()
    {
        // Play game animation/state
        foreach (NPC npc in groupMembers)
        {
            npc.PlayGame();
        }
        
        yield return new WaitForSeconds(3.0f);
        
        ChangeState(GroupState.OrderingDrinks);
    }

    private IEnumerator HandleOrderingDrinksState()
    {
        drinkOrders.Clear();
        drinksServed.Clear();
        allDrinksServed = false;
        
        // Each NPC orders a random drink
        foreach (NPC npc in groupMembers)
        {
            string drink = npc.OrderRandomDrink();
            drinkOrders[npc] = drink;
            drinksServed[npc] = false;
            
            assignedTable.ShowDrinkOrder(npc, drink);
            yield return new WaitForSeconds(0.5f);
        }
        
        ChangeState(GroupState.WaitingForDrinks);
    }

    private IEnumerator HandleWaitingForDrinksState()
    {
        stateTimer = drinkWaitPatience;
        
        while (stateTimer > 0 && !allDrinksServed)
        {
            stateTimer -= Time.deltaTime;
            yield return null;
        }
        
        if (stateTimer <= 0)
        {
            // Patience ran out, leave
            ChangeState(GroupState.Leaving);
            yield break;
        }
        
        // All drinks served, start enjoying
        ChangeState(GroupState.Enjoying);
    }

    private IEnumerator HandleEnjoyingState()
    {
        // Start drinking animations
        foreach (NPC npc in groupMembers)
        {
            npc.DrinkAnimation();
        }
        
        stateTimer = enjoymentDuration;
        
        while (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
            yield return null;
        }
        
        // Chance to order again
        if (Random.Range(0, 100) < chanceToOrderAgain)
        {
            ChangeState(GroupState.OrderingDrinks);
        }
        else
        {
            ChangeState(GroupState.Leaving);
        }
    }

    private IEnumerator HandleLeavingState()
    {
        // Stand up animation
        foreach (NPC npc in groupMembers)
        {
            npc.StandUp();
        }
        
        yield return new WaitForSeconds(1.0f);
        
        // Leader goes to cashier, others to exit
        Vector3 cashierPos = cashierArea.GetRandomPositionInArea();
        groupLeader.MoveTo(cashierPos);
        
        for (int i = 1; i < groupMembers.Count; i++)
        {
            NPC npc = groupMembers[i];
            Vector3 exitPos = exitArea.GetRandomPositionInArea();
            npc.MoveTo(exitPos);
        }
        
        // Wait for leader to reach cashier
        while (!groupLeader.HasReachedDestination)
        {
            yield return null;
        }
        
        // Release the table
        tableManager.ReleaseTable(assignedTable);
        assignedTable = null;
        
        ChangeState(GroupState.AtCashier);
    }

    private IEnumerator HandleAtCashierState()
    {
        // Wait for payment interaction
        stateTimer = cashierWaitPatience;
        while (stateTimer > 0 && !groupLeader.HasPaid)
        {
            stateTimer -= Time.deltaTime;
            yield return null;
        }
        
        ChangeState(GroupState.ExitingVenue);
    }

    private IEnumerator WaitForAllNPCsToReachDestination()
    {
        bool allArrived = false;
        while (!allArrived)
        {
            allArrived = true;
            foreach (NPC npc in groupMembers)
            {
                if (!npc.HasReachedDestination)
                {
                    allArrived = false;
                    break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ChangeState(GroupState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    // Get drink order for specific NPC
    public string GetDrinkOrderForNPC(NPC npc)
    {
        if (drinkOrders.ContainsKey(npc))
        {
            return drinkOrders[npc];
        }
        return null;
    }

    // Mark drink as served for specific NPC
    public void ServeDrinkToNPC(NPC npc)
    {
        if (drinksServed.ContainsKey(npc))
        {
            drinksServed[npc] = true;
            CheckAllDrinksServed();
        }
    }

    private void CheckAllDrinksServed()
    {
        allDrinksServed = true;
        foreach (bool served in drinksServed.Values)
        {
            if (!served)
            {
                allDrinksServed = false;
                break;
            }
        }
    }
} 