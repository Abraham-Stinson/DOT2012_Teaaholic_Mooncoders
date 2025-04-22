using System.Collections.Generic;
using UnityEngine;

public class HighLight : MonoBehaviour
{
    [SerializeField] private List<Renderer> renderers;
    [SerializeField] private Color color = new Color (1f, 1f, 1f, 0.8f); 
    [SerializeField] private List<Material> materials;

    void Awake()
    {
        materials=new List<Material>();
        foreach(var renderer in renderers){
            materials.AddRange(new List<Material>(renderer.materials));
        }
    }

    public void ToggleHighLight(bool val){
        if(val){
            foreach(var material in materials){
                material.EnableKeyword("_EMISSION");

                material.SetColor("_EmissionColor", color);
            }
        }
        else{
            foreach(var material in materials){
            material.DisableKeyword("_EMISSION");
            }
        }
    }
}