using UnityEngine;

public class Valve : MonoBehaviour, IInteractable
{
    [SerializeField] private ParticleSystem waterEffect;
    [SerializeField] private BoxCollider waterEffectCollider;
    [SerializeField] private Quaternion closedRotation;
    [SerializeField] private Quaternion openRotation;
    [SerializeField] private float rotationSpeed=5f;
    private bool isOpen;
    void Start()
    {
        waterEffect.Stop();
        waterEffectCollider.enabled=false;
        closedRotation = transform.rotation;
        openRotation= closedRotation * Quaternion.Euler(0, -35, 0);

    }

    public void interact(){
        isOpen=!isOpen;
        FlowingWater();
    }

    void Update()
    {
        if (isOpen)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * rotationSpeed);
        } 
        else{
            transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void FlowingWater(){
        if(isOpen){
            waterEffectCollider.enabled=true;
            waterEffect.Play();

        }
        else{
            waterEffectCollider.enabled=false;
            waterEffect.Stop();
        }
         
    }
}
