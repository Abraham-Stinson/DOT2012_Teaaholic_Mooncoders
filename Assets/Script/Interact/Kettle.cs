using System.Collections;
using UnityEngine;

public class Kettle : MonoBehaviour
{
    [Header("About Kettle")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool isPourAnimation;
    [SerializeField] public float maxKettleMagazine = 10f;
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
    [SerializeField] private bool isOnCoolDown = false;

    [Header("Effects")]
    [SerializeField] private ParticleSystem steamParticle;

    void Start()
    {
        currentKettleMagazine = minKettleMagazine;
        isHaveTea = false;
        isHaveHotWater = false;
        isBrewed = false;
        currentBrewTimeOfTea = brewTimeOfTea;
        if (steamParticle != null)
        {
            steamParticle.Stop(true);
        }
        else
        {
            Debug.LogError("Steam Particle System is not assigned!");
        }
    }

    void Update()
    {
        CheckBrew();
    }

    /// <summary>
    /// Pour tea from the kettle
    /// </summary>
    public void PourTea(Tea_Cup teaCup)
    {
        if (isOnCoolDown)
        {
            Debug.Log("Kettle bekleme süresinde. Lütfen bekleyin.");
            return;
        }
        
        // Check if we have brewed tea to pour
        if (currentKettleMagazine > 0 && isBrewed)
        {
            SoundManager.Instance.PlayCayDoldurma();

            teaCup.AddTea();
            Debug.Log("Çay dökülüyor");
            isPourAnimation = true;
            animator.SetBool("isPour", isPourAnimation);
            currentKettleMagazine -= 1;

            SoundManager.Instance.PlayCayDoldurma();

            if (currentKettleMagazine <= 0)
            {
                currentKettleMagazine = 0; // Ensure it doesn't go negative
                isHaveHotWater = false;
                isHaveTea = false;
                isBrewed = false;
                Debug.Log("Kettle artık boş");
            }
            
            StartCoroutine(WaitForPourAnimation());
            StartCoroutine(StartCoolDown());
        }
        else if (currentKettleMagazine <= 0)
        {
            Debug.Log("Kettle boş");
        }
        else if (!isBrewed)
        {
            Debug.Log("Çay henüz demlenmemiş");
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
            
            Debug.Log("Kettle'a çay eklendi");
            return true;
        }
        else
        {
            Debug.Log("Kettle'da zaten çay var");
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
            
            // Reset brew status when new water is added
            isBrewed = false;
            
            // Reset brew time when new ingredients are added
            ResetBrewTime();
            
            Debug.Log("Kettle'a sıcak su eklendi");
            return true;
        }
        else
        {
            Debug.Log("Kettle'da zaten sıcak su var");
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
            if (steamParticle != null && !steamParticle.isPlaying)
            {
                steamParticle.Play(true);
                Debug.Log("[ParticleSystem] Started playing");
            }

            if (currentBrewTimeOfTea > 0)
            {
                currentBrewTimeOfTea -= Time.deltaTime;
            }
            else
            {
                if (!isBrewed)
                {
                    if (steamParticle != null)
                    {
                        steamParticle.Stop(true);
                        Debug.Log("[ParticleSystem] Stopped playing");
                    }
                    
                    Debug.Log("ÇAY DEMLENDİ!");
                    isBrewed = true;
                    currentKettleMagazine = maxKettleMagazine;
                }
            }
        }
        else
        {
            if (steamParticle != null && steamParticle.isPlaying)
            {
                steamParticle.Stop(true);
                Debug.Log("[ParticleSystem] Stopped - not on kettle base");
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
        Debug.Log("Kettle boşaltıldı");
        if (steamParticle != null && steamParticle.isPlaying)
        {
            steamParticle.Stop(true);
        }
    }
    
    /// <summary>
    /// Get the current state of the kettle as a string for UI display
    /// </summary>
    public string GetKettleState()
    {
        if (currentKettleMagazine > 0 && isBrewed)
        {
            return $"Demlenmiş Çay: {currentKettleMagazine}/{maxKettleMagazine}";
        }
        else if (isHaveTea && isHaveHotWater && !isBrewed)
        {
            if (CheckIsOnKettleBase())
            {
                return $"Demleniyor: {Mathf.CeilToInt(currentBrewTimeOfTea)} saniye kaldı";
            }
            else
            {
                return "Çay ve Sıcak Su (Demlemek için altlığa koy)";
            }
        }
        else if (isHaveTea && !isHaveHotWater)
        {
            return "Çay (Sıcak su gerekiyor)";
        }
        else if (!isHaveTea && isHaveHotWater)
        {
            return "Sıcak Su (Çay gerekiyor)";
        }
        else
        {
            return "Boş";
        }
    }
}
