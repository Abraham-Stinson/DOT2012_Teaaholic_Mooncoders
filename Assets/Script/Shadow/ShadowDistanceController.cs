using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // URP için

public class ShadowDistanceController : MonoBehaviour
{
    public float shadowDistance = 150f;
    
    void Start()
    {
        // URP içindeki Shadow Distance ayarını değiştir
        var pipeline = GraphicsSettings.currentRenderPipeline;
        if (pipeline is UniversalRenderPipelineAsset urpAsset)
        {
            // Reflection kullanarak private değeri değiştir
            var field = typeof(UniversalRenderPipelineAsset).GetField("m_ShadowDistance", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(urpAsset, shadowDistance);
        }
    }
}