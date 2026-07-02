using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ChildRenamerWindow : EditorWindow
{
    private GameObject selectedObject;
    private string baseName = "";
    private string indexFormat = "00";
    private int startingIndex = 0;
    private bool useCustomMappings = false;
    private List<IndexMapping> mappings = new List<IndexMapping>();
    private Vector2 scrollPosition;
    private string newMappingIndex = "";
    private string newMappingValue = "";

    [MenuItem("Tools/Child Renamer")]
    public static void ShowWindow()
    {
        ChildRenamerWindow window = GetWindow<ChildRenamerWindow>("Child Renamer");
        window.minSize = new Vector2(400, 550);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Child Renamer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Selection handling
        EditorGUILayout.BeginHorizontal();
        selectedObject = (GameObject)EditorGUILayout.ObjectField("Target Object", selectedObject, typeof(GameObject), true);
        if (GUILayout.Button("Refresh", GUILayout.Width(80)))
        {
            if (Selection.activeGameObject != null)
            {
                selectedObject = Selection.activeGameObject;
                baseName = selectedObject.name;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (selectedObject == null)
        {
            EditorGUILayout.HelpBox("Please select a GameObject or use the refresh button.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        
        // Starting Index Field
        startingIndex = EditorGUILayout.IntField("Starting Index", startingIndex);
        if (startingIndex < 0) startingIndex = 0;
        
        indexFormat = EditorGUILayout.TextField("Index Format", indexFormat);

        EditorGUILayout.Space();
        useCustomMappings = EditorGUILayout.Toggle("Use Custom Index Mappings", useCustomMappings);

        if (useCustomMappings)
        {
            DrawMappingUI();
        }

        EditorGUILayout.Space();

        // Preview
        EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
        DrawPreview();

        EditorGUILayout.Space();

        // Rename button
        GUI.enabled = selectedObject != null && !string.IsNullOrEmpty(baseName);
        if (GUILayout.Button("Rename Children", GUILayout.Height(30)))
        {
            RenameChildren();
        }
        GUI.enabled = true;

        // Quick example mappings
        if (useCustomMappings && GUILayout.Button("Load Example Mappings (11=J, 12=K, 13=L)"))
        {
            LoadExampleMappings();
        }
        
        // Quick example for starting index
        if (GUILayout.Button("Set Starting Index to 1"))
        {
            startingIndex = 1;
        }
    }

    private void DrawMappingUI()
    {
        EditorGUILayout.LabelField("Index Mappings", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Add mappings for specific indices. Example: index 11 will be named 'BaseName_J'", MessageType.Info);

        // Add new mapping
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Label("Index:", GUILayout.Width(40));
        newMappingIndex = EditorGUILayout.TextField(newMappingIndex, GUILayout.Width(60));
        
        GUILayout.Label("Value:", GUILayout.Width(40));
        newMappingValue = EditorGUILayout.TextField(newMappingValue, GUILayout.Width(100));
        
        if (GUILayout.Button("Add", GUILayout.Width(60)))
        {
            AddMapping();
        }
        EditorGUILayout.EndHorizontal();

        // Display existing mappings
        if (mappings.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Mappings:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(120));
            
            List<IndexMapping> toRemove = new List<IndexMapping>();
            foreach (var mapping in mappings)
            {
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label($"Index {mapping.Index}:", GUILayout.Width(70));
                
                string newValue = EditorGUILayout.TextField(mapping.Value, GUILayout.Width(120));
                if (newValue != mapping.Value)
                {
                    mapping.Value = newValue;
                }
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    toRemove.Add(mapping);
                }
                EditorGUILayout.EndHorizontal();
            }

            foreach (var mapping in toRemove)
            {
                mappings.Remove(mapping);
            }
            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("No mappings added yet. Add mappings above.", MessageType.Info);
        }
    }

    private void AddMapping()
    {
        if (string.IsNullOrEmpty(newMappingIndex) || string.IsNullOrEmpty(newMappingValue))
        {
            EditorUtility.DisplayDialog("Invalid Input", "Please enter both an index and a value.", "OK");
            return;
        }

        if (int.TryParse(newMappingIndex, out int index))
        {
            // Check for duplicates
            var existing = mappings.Find(m => m.Index == index);
            if (existing != null)
            {
                existing.Value = newMappingValue;
                EditorUtility.DisplayDialog("Updated", $"Updated mapping for index {index} to '{newMappingValue}'", "OK");
            }
            else
            {
                mappings.Add(new IndexMapping { Index = index, Value = newMappingValue });
                mappings.Sort((a, b) => a.Index.CompareTo(b.Index));
                EditorUtility.DisplayDialog("Added", $"Added mapping for index {index} to '{newMappingValue}'", "OK");
            }
            
            // Clear fields after adding
            newMappingIndex = "";
            newMappingValue = "";
            GUI.FocusControl(null);
        }
        else
        {
            EditorUtility.DisplayDialog("Invalid Index", "Please enter a valid number for the index.", "OK");
        }
    }

    private void DrawPreview()
    {
        if (selectedObject == null) return;

        Transform[] children = selectedObject.GetComponentsInChildren<Transform>();
        int childCount = 0;

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == selectedObject.transform) continue;
            childCount++;
        }

        if (childCount == 0)
        {
            EditorGUILayout.LabelField("No children found.");
            return;
        }

        int displayCount = Mathf.Min(childCount, 10);
        int currentIndex = 0;

        EditorGUILayout.LabelField($"Showing first {displayCount} of {childCount} children (Starting from index {startingIndex}):", EditorStyles.miniLabel);
        
        for (int i = 0; i < children.Length && currentIndex < displayCount; i++)
        {
            if (children[i] == selectedObject.transform) continue;

            // Calculate the actual index (starting from the starting index)
            int actualIndex = startingIndex + currentIndex;
            string newName = GetNewName(actualIndex);
            string currentName = children[i].name;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Child {actualIndex}:", GUILayout.Width(70));
            EditorGUILayout.LabelField($"{currentName} → {newName}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            currentIndex++;
        }

        if (childCount > 10)
        {
            EditorGUILayout.LabelField($"... and {childCount - 10} more children");
        }
    }

    private string GetNewName(int index)
    {
        string indexString;

        if (useCustomMappings)
        {
            var mapping = mappings.Find(m => m.Index == index);
            if (mapping != null && !string.IsNullOrEmpty(mapping.Value))
            {
                indexString = mapping.Value;
            }
            else
            {
                indexString = index.ToString(indexFormat);
            }
        }
        else
        {
            indexString = index.ToString(indexFormat);
        }

        return $"{baseName}_{indexString}";
    }

    private void RenameChildren()
    {
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "No target object selected.", "OK");
            return;
        }

        Transform[] children = selectedObject.GetComponentsInChildren<Transform>();
        int renamedCount = 0;
        int currentIndex = 0;

        // Undo support
        Undo.RecordObject(selectedObject, "Rename Children");

        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child == selectedObject.transform) continue;

            // Calculate the actual index (starting from the starting index)
            int actualIndex = startingIndex + currentIndex;
            string newName = GetNewName(actualIndex);

            // Check for duplicate names and add numbers if needed
            string finalName = newName;
            int duplicateCounter = 1;
            bool nameExists;

            do
            {
                nameExists = false;
                for (int j = 0; j < i; j++)
                {
                    if (children[j] != selectedObject.transform && children[j].name == finalName)
                    {
                        nameExists = true;
                        break;
                    }
                }

                if (nameExists)
                {
                    finalName = $"{newName}_{duplicateCounter}";
                    duplicateCounter++;
                }
            } while (nameExists);

            Undo.RecordObject(child, "Rename Child");
            child.name = finalName;
            renamedCount++;
            currentIndex++;
        }

        EditorUtility.DisplayDialog("Success", $"Renamed {renamedCount} children starting from index {startingIndex}.", "OK");
    }

    private void LoadExampleMappings()
    {
        mappings.Clear();
        mappings.Add(new IndexMapping { Index = 11, Value = "J" });
        mappings.Add(new IndexMapping { Index = 12, Value = "K" });
        mappings.Add(new IndexMapping { Index = 13, Value = "L" });
        useCustomMappings = true;
    }

    [System.Serializable]
    private class IndexMapping
    {
        public int Index;
        public string Value;
    }

    private void OnSelectionChange()
    {
        if (Selection.activeGameObject != null)
        {
            selectedObject = Selection.activeGameObject;
            baseName = selectedObject.name;
            Repaint();
        }
    }
}