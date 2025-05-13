using System.Collections;
using UnityEngine;

public class Kettle : MonoBehaviour
{
    [Header("About Kettle")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool isPourAnimation;
    [SerializeField] private float maxKettleMagazine = 10f;
    [SerializeField] private float minKettleMagazine = 0f;
    [SerializeField] public float currentKettleMagazine;
    [SerializeField] public bool isHaveTea;
    [SerializeField] public bool isHaveHotWater;
    [SerializeField] public bool isBrewed;
    private RaycastHit bottomOfKettle;
    [SerializeField] private float brewTimeOfTea = 10f;
    [SerializeField] public float currentBrewTimeOfTea;

    [Header("CoolDown")]
    [SerializeField] private float coolDownTime = 1f;
    private bool isOnCoolDown = false;
    
    void Start()
    {
        currentKettleMagazine = minKettleMagazine;
        isHaveTea = false;
        isHaveHotWater = false;
        isBrewed = false;
        currentBrewTimeOfTea = brewTimeOfTea;
    }

    void Update()
    {
        CheckBrew();
    }

    /// <summary>
    /// Pour tea from the kettle
    /// </summary>
    public void PourTea()
    {
        if (isOnCoolDown)
        {
            Debug.Log("Kettle is on cooldown. Please wait.");
            return;
        }
        
        // Check if we have brewed tea to pour
        if (currentKettleMagazine > 0 && isBrewed)
        {
            isPourAnimation = true;
            animator.SetBool("isPour", isPourAnimation);
            currentKettleMagazine -= 1;
            
            // If we run out of tea, update all related states
            if (currentKettleMagazine <= 0)
            {
                currentKettleMagazine = 0; // Ensure it doesn't go negative
                isHaveHotWater = false;
                isHaveTea = false;
                isBrewed = false;
                Debug.Log("Kettle is now empty");
            }
            
            StartCoroutine(WaitForPourAnimation());
            StartCoroutine(StartCoolDown());
        }
        else if (currentKettleMagazine <= 0)
        {
            Debug.Log("Kettle is empty");
        }
        else if (!isBrewed)
        {
            Debug.Log("Tea is not brewed yet");
        }
    }
    
    /// <summary>
    /// Add tea to the kettle
    /// </summary>
    public bool AddTea()
    {
        // Only add tea if we don't already have tea
        if (!isHaveTea)
        {
            isHaveTea = true;
            
            // Reset brew status when new tea is added
            isBrewed = false;
            
            // Reset brew time when new ingredients are added
            ResetBrewTime();
            
            Debug.Log("Tea added to kettle");
            return true;
        }
        else
        {
            Debug.Log("Kettle already has tea");
            return false;
        }
    }
    
    /// <summary>
    /// Add hot water to the kettle
    /// </summary>
    public bool AddHotWater()
    {
        // Only add hot water if we don't already have hot water
        if (!isHaveHotWater)
        {
            isHaveHotWater = true;
            
            // Reset brew status when new hot water is added
            isBrewed = false;
            
            // Reset brew time when new ingredients are added
            ResetBrewTime();
            
            Debug.Log("Hot water added to kettle");
            return true;
        }
        else
        {
            Debug.Log("Kettle already has hot water");
            return false;
        }
    }

    private IEnumerator WaitForPourAnimation()
    {
        float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration);
        isPourAnimation = false;
        animator.SetBool("isPour", isPourAnimation);
    }

    IEnumerator StartCoolDown()
    {
        isOnCoolDown = true;
        yield return new WaitForSeconds(coolDownTime);
        isOnCoolDown = false;
    }
    
    /// <summary>
    /// Reset the brew time to its original value
    /// </summary>
    public void ResetBrewTime()
    {
        currentBrewTimeOfTea = brewTimeOfTea;
        Debug.Log("Brew time reset");
    }

    void CheckBrew()
    {
        Debug.DrawRay(transform.position, -transform.up * 1f, Color.red);
        
        // Only check brewing if we have both tea and hot water but not already brewed
        if (isHaveTea && isHaveHotWater && !isBrewed)
        {
            if (CheckIsOnKettleBase())
            {
                UpdateBrewTime(true);
            }
            else
            {
                UpdateBrewTime(false);
            }
        }
    }

    void UpdateBrewTime(bool shouldContinue)
    {
        if (shouldContinue)
        {
            if (currentBrewTimeOfTea > 0)
            {
                currentBrewTimeOfTea -= Time.deltaTime;
            }
            else
            {
                // Only log the message when the tea first becomes brewed
                if (!isBrewed)
                {
                    Debug.Log("TEA IS BREWED!");
                    isBrewed = true;
                    currentKettleMagazine = maxKettleMagazine;
                }
            }
        }
    }

    public bool CheckIsOnKettleBase()
    {
        // Use the transform.up direction (which should be pointing upwards in world space)
        // and check in the opposite direction to find what's below the kettle
        if (Physics.Raycast(transform.position, -transform.up, out bottomOfKettle, 1f) && 
            bottomOfKettle.collider.CompareTag("Kettle_Base"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Completely empty the kettle
    /// </summary>
    public void EmptyKettle()
    {
        currentKettleMagazine = 0;
        isHaveHotWater = false;
        isHaveTea = false;
        isBrewed = false;
        Debug.Log("Kettle emptied");
    }
    
    /// <summary>
    /// Get the current state of the kettle as a string for UI display
    /// </summary>
    public string GetKettleState()
    {
        if (currentKettleMagazine > 0 && isBrewed)
        {
            return $"Brewed Tea: {currentKettleMagazine}/{maxKettleMagazine}";
        }
        else if (isHaveTea && isHaveHotWater && !isBrewed)
        {
            if (CheckIsOnKettleBase())
            {
                return $"Brewing: {Mathf.CeilToInt(currentBrewTimeOfTea)}s left";
            }
            else
            {
                return "Tea & Hot Water (Put on base to brew)";
            }
        }
        else if (isHaveTea && !isHaveHotWater)
        {
            return "Tea (Needs hot water)";
        }
        else if (!isHaveTea && isHaveHotWater)
        {
            return "Hot Water (Needs tea)";
        }
        else
        {
            return "Empty";
        }
    }
}
