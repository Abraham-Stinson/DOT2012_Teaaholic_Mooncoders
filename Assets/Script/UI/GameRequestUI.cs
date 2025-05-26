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
            case "tavla": return tavlaBubble;
            case "okey": return okeyBubble;
            case "iskambil": return iskambilBubble;
            default: return defaultBubble;
        }
    }
}
