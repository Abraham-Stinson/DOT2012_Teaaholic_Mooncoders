using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public Light lightToControl; // Hangi ışığı kontrol edeceğiz
    public KeyCode interactKey = KeyCode.E; // Hangi tuşa basılacak
    private bool isPlayerNear = false;

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            lightToControl.enabled = !lightToControl.enabled; // Işığı aç/kapat
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
