using UnityEngine;
using UnityEngine.EventSystems;

public class HoverReveal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject[] objectsToShow;

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (GameObject obj in objectsToShow)
        {
            obj.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (GameObject obj in objectsToShow)
        {
            obj.SetActive(false);
        }
    }
}
