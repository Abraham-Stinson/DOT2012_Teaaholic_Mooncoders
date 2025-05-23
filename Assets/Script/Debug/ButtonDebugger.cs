using UnityEngine;
using UnityEngine.UI;

public class ButtonDebugger : MonoBehaviour
{
    void Start()
    {
        // Tüm butonları bul ve durumlarını kontrol et
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        Debug.Log($"Total buttons found: {allButtons.Length}");
        
        foreach (Button button in allButtons)
        {
            Debug.Log($"Button: {button.name}");
            Debug.Log($"- Active: {button.gameObject.activeInHierarchy}");
            Debug.Log($"- Interactable: {button.interactable}");
            Debug.Log($"- Raycast Target: {button.GetComponent<Image>()?.raycastTarget}");
            Debug.Log($"- Parent: {button.transform.parent.name}");
            
            // Test listener ekle
            button.onClick.AddListener(() => {
                Debug.Log($"Button clicked: {button.name}");
            });
        }
    }
}