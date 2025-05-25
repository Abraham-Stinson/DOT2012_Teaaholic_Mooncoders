using UnityEngine;

public class TableBubbleSpawner : MonoBehaviour
{
    private TableController table;
    private GameObject bubble;
    private string currentRequest = "";

    private void Start()
    {
        table = GetComponent<TableController>();
    }

    private void Update()
    {
        if (table == null) return;

        bool hasGroup = table.HasGroup();
        bool seated = table.IsGroupSeated();
        string request = table.GetRequestedGameType();
        bool isPlaced = table.IsGamePlaced(); // yeni kontrol

        if (hasGroup && seated && !string.IsNullOrEmpty(request) && !isPlaced)
        {
            if (bubble == null || currentRequest != request)
            {
                SpawnBubble(request);
                currentRequest = request;
            }
        }
        else
        {
            RemoveBubble(); // oyun verildiyse bubble'ý kaldýr
        }
    }

    void SpawnBubble(string request)
    {
        RemoveBubble(); // varsa öncekini sil

        Vector3 pos = transform.position + Vector3.up * 6f;
        bubble = Instantiate(GameRequestUI.Instance.bubblePrefab, pos, Quaternion.identity, transform);
        SpriteRenderer sr = bubble.GetComponent<SpriteRenderer>();
        sr.sprite = GameRequestUI.Instance.GetBubbleSprite(request);
    }

    void RemoveBubble()
    {
        if (bubble != null)
        {
            Destroy(bubble);
            bubble = null;
            currentRequest = "";
        }
    }
}
