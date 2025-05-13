using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject patienceUI;
    [SerializeField] private GameObject orderBubble;
    [SerializeField] private GameObject drinkObject;
    
    [Header("State")]
    private NPCState currentState;
    private NPCManager manager;
    private NPCGroup group;
    private Table targetTable;
    private Chair assignedChair;
    private bool isLeader;
    private float patienceTimer;
    private string requestedDrink;
    private bool hasBeenServed = false;
    private Coroutine drinkConsumptionCoroutine;
    private Coroutine sittingCoroutine;
    
    // Event to notify when NPC is destroyed
    public System.Action<NPC> OnDestroyed;
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator found on NPC");
        }
    }
    
    public void Initialize(NPCManager npcManager, NPCGroup npcGroup, bool leader, Table table, Chair assignedChair)
    {
        if (npcManager == null)
        {
            Debug.LogError("NPCManager is null in Initialize!");
            return;
        }
        
        manager = npcManager;
        group = npcGroup;
        isLeader = leader;
        targetTable = table;
        this.assignedChair = assignedChair;
        
        patienceTimer = manager.GetInitialPatience();
        
        if (targetTable != null && this.assignedChair != null)
        {
            // Move to entrance first, then to table
            SetState(NPCState.MovingToEntrance);
        }
        else
        {
            // No table or chair available, move to exit
            SetState(NPCState.MovingToExit);
        }
    }
    
    void Update()
    {
        if (manager == null)
        {
            Debug.LogError("NPCManager is null in Update! NPC: " + gameObject.name);
            return;
        }
        
        UpdateState();
        UpdatePatience();
    }
    
    void UpdateState()
    {
        if (manager == null) return;
        
        switch (currentState)
        {
            case NPCState.MovingToEntrance:
                if (HasReachedDestination())
                {
                    // Play entrance animation
                    StartCoroutine(EnterCafe());
                }
                break;
                
            case NPCState.MovingToTable:
                if (HasReachedDestination() && assignedChair != null)
                {
                    SetState(NPCState.Sitting);
                }
                break;
                
            case NPCState.Sitting:
                // Behavior handled by state change events
                break;
                
            case NPCState.MovingToCashier:
                if (HasReachedDestination())
                {
                    SetState(NPCState.WaitingAtCashier);
                }
                break;
                
            case NPCState.WaitingAtCashier:
                // Waiting for player interaction
                break;
                
            case NPCState.MovingToExit:
                if (HasReachedDestination())
                {
                    Destroy(gameObject);
                }
                break;
        }
    }
    
    IEnumerator EnterCafe()
    {
        // Play door open animation here if needed
        yield return new WaitForSeconds(1.5f);
        
        if (targetTable != null && assignedChair != null)
        {
            SetState(NPCState.MovingToTable);
        }
        else
        {
            SetState(NPCState.MovingToExit);
        }
    }
    
    void UpdatePatience()
    {
        if (manager == null) return;
        
        if (currentState == NPCState.MovingToExit || currentState == NPCState.Destroyed)
        {
            return;
        }
        
        patienceTimer -= Time.deltaTime;
        UpdatePatienceUI();
        
        if (patienceTimer <= 0)
        {
            // Patience has run out
            LeaveTable();
        }
    }
    
    void UpdatePatienceUI()
    {
        if (patienceUI != null)
        {
            // Update UI to show patience status
            // Could be a progress bar or color change
        }
    }
    
    void SetState(NPCState newState)
    {
        if (manager == null)
        {
            Debug.LogError("NPCManager is null in SetState! NPC: " + gameObject.name);
            return;
        }
        
        if (currentState == newState) return;
        
        // Exit current state
        switch (currentState)
        {
            case NPCState.Sitting:
                if (assignedChair != null)
                {
                    assignedChair.SetOccupied(false);
                }
                StandUpAnimation();
                break;
                
            case NPCState.WaitingAtCashier:
                if (isLeader && group != null)
                {
                    manager.GroupLeaving(group);
                }
                break;
        }
        
        currentState = newState;
        
        // Enter new state
        switch (newState)
        {
            case NPCState.MovingToEntrance:
                if (manager.GetEntranceArea() == null)
                {
                    Debug.LogError("Entrance area is null! NPC: " + gameObject.name);
                    return;
                }
                Vector3 entrancePos = manager.GetRandomPositionInArea(manager.GetEntranceArea());
                agent.SetDestination(entrancePos);
                PlayWalkAnimation();
                break;
                
            case NPCState.MovingToTable:
                if (assignedChair != null)
                {
                    // Sandalyenin önüne git
                    Vector3 chairPos = assignedChair.transform.position;
                    Vector3 chairForward = assignedChair.transform.forward;
                    Vector3 targetPos = chairPos - (chairForward * 0.5f); // Sandalyenin 0.5 birim önü
                    
                    agent.SetDestination(targetPos);
                    PlayWalkAnimation();
                }
                break;
                
            case NPCState.Sitting:
                if (assignedChair != null)
                {
                    if (sittingCoroutine != null)
                    {
                        StopCoroutine(sittingCoroutine);
                    }
                    sittingCoroutine = StartCoroutine(SitOnChair());
                }
                break;
                
            case NPCState.MovingToCashier:
                if (manager.GetCashierArea() == null)
                {
                    Debug.LogError("Cashier area is null! NPC: " + gameObject.name);
                    return;
                }
                Vector3 cashierPos = manager.GetRandomPositionInArea(manager.GetCashierArea());
                agent.SetDestination(cashierPos);
                patienceTimer = manager.GetCashierPatience();
                PlayWalkAnimation();
                break;
                
            case NPCState.WaitingAtCashier:
                // Display waiting for payment UI
                PlayIdleAnimation();
                break;
                
            case NPCState.MovingToExit:
                if (manager.GetExitArea() == null)
                {
                    Debug.LogError("Exit area is null! NPC: " + gameObject.name);
                    return;
                }
                Vector3 exitPos = manager.GetRandomPositionInArea(manager.GetExitArea());
                agent.SetDestination(exitPos);
                PlayWalkAnimation();
                break;
        }
    }
    
    IEnumerator SitOnChair()
    {
        if (assignedChair == null) yield break;
        
        // NavMeshAgent'i devre dışı bırak
        if (agent != null)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
        
        // Sandalyeye doğru dön
        Vector3 lookDirection = assignedChair.transform.forward;
        transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // Oturma animasyonunu başlat
        PlaySitAnimation();
        
        // Animasyonun tamamlanmasını bekle
        yield return new WaitForSeconds(1.0f);
        
        // Tam oturma pozisyonuna geç
        transform.position = assignedChair.GetSittingPositionVector();
        transform.rotation = assignedChair.GetSittingRotation();
        
        // Sandalyeyi işgal et
        assignedChair.SetOccupied(true);
        
        // NavMeshAgent'i kalıcı olarak devre dışı bırak
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // Grup lideri ise ve oyun istenmemişse, oyun iste
        if (isLeader && !group.hasReceivedGame)
        {
            DisplayGameRequest();
        }
        else if (group.hasReceivedGame && !hasBeenServed)
        {
            OrderDrink();
        }
    }
    
    bool HasReachedDestination()
    {
        if (agent == null) return false;
        
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            return true;
        }
        return false;
    }
    
    void PlayWalkAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isSitting", false);
        }
    }
    
    void PlaySitAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isSitting", true);
        }
    }
    
    void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isSitting", false);
        }
    }
    
    void StandUpAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isSitting", false);
        }
        
        // NavMeshAgent'i tekrar etkinleştir
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
    }
    
    void DisplayGameRequest()
    {
        if (isLeader && !group.hasReceivedGame)
        {
            string gameRequest = group.requestedGameType;
            if (Player.Instance != null)
            {
                Player.Instance.ShowNPCInfo($"Group wants to play {gameRequest}");
            }
        }
    }
    
    void OrderDrink()
    {
        if (manager == null) return;
        
        requestedDrink = manager.GetRandomDrink();
        if (Player.Instance != null)
        {
            Player.Instance.ShowNPCInfo($"Customer wants {requestedDrink}");
        }
    }
    
    public void LeaveTable()
    {
        if (manager == null)
        {
            Debug.LogError("NPCManager is null in LeaveTable! NPC: " + gameObject.name);
            return;
        }
        
        if (hasBeenServed)
        {
            // If leader, go to cashier, otherwise go to exit
            if (isLeader)
            {
                SetState(NPCState.MovingToCashier);
            }
            else
            {
                SetState(NPCState.MovingToExit);
            }
        }
        else
        {
            // If not served, just leave
            SetState(NPCState.MovingToExit);
        }
    }
    
    public bool ReceiveGame(string gameType)
    {
        if (manager == null) return false;
        
        if (isLeader && currentState == NPCState.Sitting && !group.hasReceivedGame && gameType == group.requestedGameType)
        {
            group.hasReceivedGame = true;
            
            // Notify all group members to order drinks
            foreach (NPC member in group.members)
            {
                member.OrderDrink();
            }
            
            return true;
        }
        return false;
    }
    
    public bool ReceiveDrink(string drinkType)
    {
        if (manager == null) return false;
        
        if (currentState == NPCState.Sitting && group.hasReceivedGame && !hasBeenServed && drinkType == requestedDrink)
        {
            hasBeenServed = true;
            group.servedMembers++;
            
            // Show drink on the table
            if (drinkObject != null)
            {
                drinkObject.SetActive(true);
            }
            
            // Reset patience for other group members
            foreach (NPC member in group.members)
            {
                if (!member.hasBeenServed)
                {
                    member.ResetPatience();
                }
            }
            
            // Start drink consumption timer
            drinkConsumptionCoroutine = StartCoroutine(ConsumeDrink());
            
            return true;
        }
        return false;
    }
    
    IEnumerator ConsumeDrink()
    {
        if (manager == null) yield break;
        
        yield return new WaitForSeconds(manager.GetDrinkConsumptionTime());
        
        // Chance to order another drink
        if (manager.ShouldReorder())
        {
            hasBeenServed = false;
            OrderDrink();
        }
        else
        {
            // Check if everyone is done with their drinks
            bool allDone = true;
            foreach (NPC member in group.members)
            {
                if (!member.hasBeenServed || member.IsConsumingDrink())
                {
                    allDone = false;
                    break;
                }
            }
            
            if (allDone)
            {
                // Group is ready to leave
                foreach (NPC member in group.members)
                {
                    member.LeaveTable();
                }
            }
        }
    }
    
    public bool IsConsumingDrink()
    {
        return drinkConsumptionCoroutine != null;
    }
    
    public void ResetPatience()
    {
        if (manager == null) return;
        
        patienceTimer = manager.GetPatienceAfterServing();
    }
    
    public void ReceivePayment()
    {
        if (currentState == NPCState.WaitingAtCashier && isLeader)
        {
            // Payment received, now leave
            SetState(NPCState.MovingToExit);
        }
    }
    
    public void interact()
    {
        if (currentState == NPCState.WaitingAtCashier && isLeader)
        {
            ReceivePayment();
        }
    }
    
    public string GetRequestedGame()
    {
        if (isLeader && !group.hasReceivedGame)
        {
            return group.requestedGameType;
        }
        return null;
    }
    
    public string GetRequestedDrink()
    {
        if (group.hasReceivedGame && !hasBeenServed)
        {
            return requestedDrink;
        }
        return null;
    }
    
    public NPCState GetState()
    {
        return currentState;
    }
    
    public bool IsLeader()
    {
        return isLeader;
    }
    
    void OnDestroy()
    {
        if (OnDestroyed != null)
        {
            OnDestroyed(this);
        }
    }
    
    public string GetNPCInfo()
    {
        switch (currentState)
        {
            case NPCState.Sitting:
                if (isLeader && !group.hasReceivedGame)
                {
                    return $"Group wants to play {group.requestedGameType}";
                }
                else if (group.hasReceivedGame && !hasBeenServed)
                {
                    return $"Customer wants {requestedDrink}";
                }
                break;
                
            case NPCState.WaitingAtCashier:
                return "Press F to collect payment";
        }
        return null;
    }
    
    public Table GetAssignedTable()
    {
        return targetTable;
    }
    
    public Chair GetAssignedChair()
    {
        return assignedChair;
    }
}

public enum NPCState
{
    MovingToEntrance,
    MovingToTable,
    Sitting,
    MovingToCashier,
    WaitingAtCashier,
    MovingToExit,
    Destroyed
} 