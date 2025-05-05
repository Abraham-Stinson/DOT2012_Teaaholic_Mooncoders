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

    [Header("CoolDown")]
    [SerializeField] private float coolDownTime = 1f;
    private bool isOnCoolDown = false;
    void Start()
    {
        currentKettleMagazine = minKettleMagazine;
    }


    void Update()
    {

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

    IEnumerator StartCoolDown(){
        isOnCoolDown=true;
        yield return new WaitForSeconds(coolDownTime);
        isOnCoolDown=false;
    }
}
