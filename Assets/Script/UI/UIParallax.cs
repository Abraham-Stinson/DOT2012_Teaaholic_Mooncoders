using UnityEngine;

public class UIParallax : MonoBehaviour
{
    public float moveAmount = 10f;      // Ne kadar hareket etsin (piksel cinsinden)
    public float smoothSpeed = 5f;      // Ne kadar yumuþak geçiþ olsun

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {

        float mouseX = (Input.mousePosition.x / Screen.width) * 2 - 1;
        float mouseY = (Input.mousePosition.y / Screen.height) * 2 - 1;

        Vector2 targetPos = initialPosition + new Vector2(mouseX, mouseY) * moveAmount;
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * smoothSpeed);
    }

}
