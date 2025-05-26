using UnityEngine;

public class GameRequestUI : MonoBehaviour
{
    public static GameRequestUI Instance;

    public GameObject bubblePrefab;
    public Sprite tavlaBubble;
    public Sprite okeyBubble;
    public Sprite iskambilBubble;
    public Sprite defaultBubble;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public Sprite GetBubbleSprite(string gameRequest)
    {
        switch (gameRequest.ToLower())
        {
            case "Tavla": return tavlaBubble;
            case "Okey": return okeyBubble;
            case "Iskambil": return iskambilBubble;
            default: return defaultBubble;
        }
    }
}
