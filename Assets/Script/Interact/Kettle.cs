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
    [SerializeField] private float brewTimeOfTea=10f;
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
    }


    void Update()
    {
        CheckBrew();
    }

    public void PourTea()
    {

        if (isOnCoolDown)
        {
            Debug.Log("Kettle is on cooldown. Please wait.");
            return;
        }
        if (currentKettleMagazine > 0)
        {
            isPourAnimation = true;
            animator.SetBool("isPour", isPourAnimation);
            currentKettleMagazine -= 1;
            if(currentKettleMagazine==0){
                isHaveHotWater=false;
                isHaveTea=false;
            }
            StartCoroutine(WaitForPourAnimation());
            StartCoroutine(StartCoolDown());
        }
        else
        {
            Debug.Log("Kettle çayı bitti");
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
    public void ChangeBrewTime(){
        currentBrewTimeOfTea=brewTimeOfTea;
    }

    void CheckBrew()
    {
        Debug.DrawRay(transform.position, -transform.up * 1f, Color.red);
        if (CheckIsOnKettleBase())
        {
            if (isHaveHotWater&&isHaveTea)
            {
                UpdateBrewTime(true);
            }
            else
            {
                UpdateBrewTime(false);
            }
        }
        else{
            UpdateBrewTime(false);
        }
    }

    void UpdateBrewTime(bool Continue){
        if(Continue){
            if(currentBrewTimeOfTea>0){
                currentBrewTimeOfTea-=Time.deltaTime;
            }
            else{
                isBrewed=true;
                Debug.Log("ÇAY DEMLENDİİİ");
                currentKettleMagazine=maxKettleMagazine;
            }
        }
    }

    public bool CheckIsOnKettleBase(){
        if(Physics.Raycast(transform.position, -transform.up, out bottomOfKettle, 1f) && bottomOfKettle.collider.CompareTag("Kettle_Base")){
            return true;
        }
        else{
            return false;
        }
    }

    public void EmptyKettle(){
        currentKettleMagazine=0;
        isHaveHotWater=false;
        isHaveTea=false;
    }
}
