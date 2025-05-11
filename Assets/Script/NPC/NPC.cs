using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPC : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float waitingPatience = 90f;
    [SerializeField] private string[] possibleDrinks;

    private NavMeshAgent agent;
    private NPCGroup group;
    private bool isGroupLeader;
    private Vector3 targetPosition;
    private bool isSitting = false;
    private bool hasPaid = false;
    private string currentAnimation = "";

    public bool IsGroupLeader => isGroupLeader;
    public bool HasPaid => hasPaid;
    public bool HasReachedDestination => !agent.pathPending && 
                                       agent.remainingDistance <= agent.stoppingDistance && 
                                       (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);

    private bool isInTargetArea = false;
    public string currentAreaType = "None";
    public int currentTableIndex = -1;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Default drinks if not set in inspector
        if (possibleDrinks == null || possibleDrinks.Length == 0)
        {
            possibleDrinks = new string[]
            {
                "Light_Tea", "Rabbit_Blood_Tea", "Brewed_Tea",
                "Coffee_Drink",
                "Orange_Oralet", "Banana_Oralet", "Kiwi_Oralet", "Strawberry_Oralet"
            };
        }
    }

    public void Initialize(bool leader)
    {
        isGroupLeader = leader;
    }

    public void MoveTo(Vector3 position)
    {
        isInTargetArea = false; // Yeni hedefe giderken reset
        targetPosition = position;
        agent.SetDestination(position);
        PlayAnimation("Walking");
    }

    public bool IsInTargetArea()
    {
        return isInTargetArea;
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerArea area = other.GetComponent<TriggerArea>();
        if (area != null)
        {
            isInTargetArea = true;
            currentAreaType = area.areaType;
            currentTableIndex = area.tableIndex;
            
            Debug.Log($"NPC {gameObject.name} entered {area.areaType} area");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerArea area = other.GetComponent<TriggerArea>();
        if (area != null && area.areaType == currentAreaType)
        {
            isInTargetArea = false;
        }
    }

    public void Sit()
    {
        isSitting = true;
        PlayAnimation("Sitting");
    }

    public void StandUp()
    {
        isSitting = false;
        PlayAnimation("StandingUp");
    }

    public void PlayGame()
    {
        PlayAnimation("PlayingGame");
    }

    public string OrderRandomDrink()
    {
        PlayAnimation("Ordering");
        return possibleDrinks[Random.Range(0, possibleDrinks.Length)];
    }

    public void DrinkAnimation()
    {
        PlayAnimation("Drinking");
    }

    public void SetPaid(bool paid)
    {
        hasPaid = paid;
    }

    private void PlayAnimation(string animName)
    {
        if (animator != null && currentAnimation != animName)
        {
            // Reset all animation parameters
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsSitting", false);
            animator.SetBool("IsPlayingGame", false);
            animator.SetBool("IsOrdering", false);
            animator.SetBool("IsDrinking", false);

            // Set the appropriate parameter
            switch (animName)
            {
                case "Walking":
                    animator.SetBool("IsWalking", true);
                    break;
                case "Sitting":
                    animator.SetBool("IsSitting", true);
                    break;
                case "PlayingGame":
                    animator.SetBool("IsPlayingGame", true);
                    break;
                case "Ordering":
                    animator.SetBool("IsOrdering", true);
                    break;
                case "Drinking":
                    animator.SetBool("IsDrinking", true);
                    break;
                case "StandingUp":
                    // Just need to disable IsSitting
                    break;
            }

            currentAnimation = animName;
        }
    }
}