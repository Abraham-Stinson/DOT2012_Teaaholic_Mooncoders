using UnityEngine;

public class CupboardDoor : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    public float rotationSpeed = 3f;

    [SerializeField] private bool isRightDoor=true; 

    void Start()
    {
        closedRotation = transform.rotation;
        if(isRightDoor)
        openRotation = closedRotation * Quaternion.Euler(0, 0, 90);
        else
        openRotation = closedRotation * Quaternion.Euler(0, 0, -90);
    }

    void Update()
    {
        OpenCupBoard();
    }

    public void interact() // Küçük harfle başlayan metod adı
    {
        isOpen = !isOpen;
    }
    void OpenCupBoard(){
        if (isOpen)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * rotationSpeed);
        } 
        else
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, Time.deltaTime * rotationSpeed);
        }
    }
}