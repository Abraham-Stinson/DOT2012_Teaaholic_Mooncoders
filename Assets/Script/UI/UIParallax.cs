using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input System desteði

public class UIParallax : MonoBehaviour
{
    public float moveAmount = 10f;
    public float smoothSpeed = 5f;

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // Yeni Input System'den mouse pozisyonunu oku
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float mouseX = (mousePosition.x / Screen.width) * 2 - 1;
        float mouseY = (mousePosition.y / Screen.height) * 2 - 1;

        Vector2 targetPos = initialPosition + new Vector2(mouseX, mouseY) * moveAmount;
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * smoothSpeed);
    }
}
