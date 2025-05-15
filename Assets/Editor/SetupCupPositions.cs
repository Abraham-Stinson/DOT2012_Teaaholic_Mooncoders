using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tool to automatically set up cup positions for all chairs in the scene
/// </summary>
public class SetupCupPositions : EditorWindow
{
    [MenuItem("Tools/Setup/Create Cup Positions")]
    public static void ShowWindow()
    {
        GetWindow<SetupCupPositions>("Cup Position Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Create Cup Positions for All Chairs", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Setup All Chairs in Scene"))
        {
            SetupAllChairsInScene();
        }
        
        if (GUILayout.Button("Setup Selected Tables"))
        {
            SetupSelectedTables();
        }
    }
    
    private void SetupAllChairsInScene()
    {
        Chair[] allChairs = GameObject.FindObjectsOfType<Chair>();
        int count = 0;
        
        foreach (Chair chair in allChairs)
        {
            if (CreateCupPositionForChair(chair))
            {
                count++;
            }
        }
        
        Debug.Log($"Created cup positions for {count} chairs");
    }
    
    private void SetupSelectedTables()
    {
        int chairCount = 0;
        
        foreach (GameObject obj in Selection.gameObjects)
        {
            TableController table = obj.GetComponent<TableController>();
            
            if (table != null)
            {
                List<Chair> chairs = table.GetChairs();
                
                foreach (Chair chair in chairs)
                {
                    if (CreateCupPositionForChair(chair))
                    {
                        chairCount++;
                    }
                }
            }
            else
            {
                // Check if it's a chair directly
                Chair chair = obj.GetComponent<Chair>();
                if (chair != null && CreateCupPositionForChair(chair))
                {
                    chairCount++;
                }
            }
        }
        
        Debug.Log($"Created cup positions for {chairCount} chairs in selected objects");
    }
    
    private bool CreateCupPositionForChair(Chair chair)
    {
        if (chair == null) return false;
        
        // Check if the chair already has a cup position using reflection (since it's private)
        System.Reflection.FieldInfo fieldInfo = typeof(Chair).GetField("cupPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Transform existingCupPosition = fieldInfo?.GetValue(chair) as Transform;
        
        if (existingCupPosition != null)
        {
            // Already has a cup position
            return false;
        }
        
        // Get the chair's forward direction and position
        Transform chairTransform = chair.transform;
        
        // Create a new cup position object
        GameObject cupPosObj = new GameObject("CupPosition");
        Undo.RegisterCreatedObjectUndo(cupPosObj, "Create Cup Position");
        
        // Position the cup slightly in front of the chair and to the right
        cupPosObj.transform.SetParent(chairTransform);
        cupPosObj.transform.localPosition = chairTransform.forward * 0.6f + chairTransform.right * 0.2f + Vector3.up * 0.05f;
        cupPosObj.transform.localRotation = Quaternion.identity;
        
        // Set the cup position field using reflection
        if (fieldInfo != null)
        {
            Undo.RecordObject(chair, "Set Cup Position");
            fieldInfo.SetValue(chair, cupPosObj.transform);
        }
        
        return true;
    }
} 