using UnityEngine;

public class Valve : MonoBehaviour, IInteractable
{
    [SerializeField] private ParticleSystem waterEffect;
    [SerializeField] private Collider waterEffectCollider;
    [SerializeField] private Animator animator;
    private bool isOpen;
    void Start()
    {
        waterEffect.Stop();
        waterEffectCollider.enabled = false;


    }

    public void interact()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen", isOpen);
        FlowingWater();
    }

    void Update()
    {

    }

    void FlowingWater()
    {
        if (isOpen)
        {
            waterEffectCollider.enabled = true;
            waterEffect.Play();

        }
        else
        {
            waterEffectCollider.enabled = false;
            waterEffect.Stop();
        }

    }
}
