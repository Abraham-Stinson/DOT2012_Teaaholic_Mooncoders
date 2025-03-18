using UnityEngine;

public class CupboardDoor : MonoBehaviour, IInteractable
{
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    public float rotationSpeed = 2f;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, 90, 0);
    }

    void Update()
    {
        OpenCupBoard();
    }

    // IInteractable interface'ini implemente et
    public void interact() // Küçük harfle başlayan metod adı
    {
        isOpen = !isOpen;
        Debug.Log(isOpen ? "Dolap kapağı açıldı!" : "Dolap kapağı kapandı!");
        
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