using UnityEngine;

public class CupboardDoor : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    [SerializeField] private Animator animator;

    public void interact()
    {
        isOpen = !isOpen;
        animator.SetBool("isOpen",isOpen);
    }
    
}