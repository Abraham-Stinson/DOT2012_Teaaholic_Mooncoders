using System.Collections;
using UnityEngine;

public class Kettle : MonoBehaviour
{
    [Header("About Kettle")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool isPourAnimation;
    [SerializeField] private float maxKettleMagazine=10f;
    [SerializeField] private float minKettleMagazine=0f;
    [SerializeField] private float currentKettleMagazine;
    void Start()
    {
        currentKettleMagazine=minKettleMagazine;
    }

    
    void Update()
    {
        
    }

    public void PourTea(){
        isPourAnimation=true;
        animator.SetBool("isPour",isPourAnimation);
        currentKettleMagazine-=1;
        Debug.Log("Çay verildi");
        StartCoroutine(WaitForPourAnimation());
    }

    private IEnumerator WaitForPourAnimation(){
        float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationDuration);
        isPourAnimation = false;
        animator.SetBool("isPour", isPourAnimation);

    }
}
