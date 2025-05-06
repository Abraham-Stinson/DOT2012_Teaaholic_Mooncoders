using UnityEngine;

public class LightbulbSwitch : MonoBehaviour, IInteractable
{
    
    [SerializeField] private Light bulb;
    [SerializeField] private bool isOpen;
    [SerializeField] private Animator animator;
    void Start()
    {
        bulb.enabled=false;
    }

    public void interact(){
        isOpen=!isOpen;
        animator.SetBool("is_Switch_Open",isOpen);
        bulb.enabled=isOpen;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
