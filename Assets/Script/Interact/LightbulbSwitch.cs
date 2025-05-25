using UnityEngine;

public class LightbulbSwitch : MonoBehaviour, IInteractable
{

    [SerializeField] private Light[] bulbs;
    [SerializeField] private bool isOpen;
    [SerializeField] private Animator animator;
    void Start()
    {
        foreach (Light bulb in bulbs)
        {
            bulb.enabled = false;
        }
    }

    public void interact()
    {
        isOpen = !isOpen;
        animator.SetBool("is_Switch_Open", isOpen);
        foreach (Light bulb in bulbs)
        {
            bulb.enabled = isOpen;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
