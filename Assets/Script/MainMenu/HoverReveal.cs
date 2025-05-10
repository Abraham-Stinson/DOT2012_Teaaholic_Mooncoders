using UnityEngine;
using UnityEngine.EventSystems;

public class HoverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject objectToShow_1;
    public GameObject objectToShow_2;


    public void OnPointerEnter(PointerEventData eventData)
    {
        objectToShow_1.SetActive(true);
        objectToShow_2.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        objectToShow_1.SetActive(false);
        objectToShow_2.SetActive(false);
    }
}
