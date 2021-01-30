using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[ExecuteInEditMode]
public class LevelGenerator : MonoBehaviour
{
    public GridGenerator gridGenerator;
    public AreaGenerator areaGenerator;

    private void Start()
    {
        // Generate();
    }

    public void Generate()
    {
        gridGenerator.GenerateGrid();

        areaGenerator.FindPrefabs();
        areaGenerator.StartAreaGenerator(gridGenerator.grid.linearLevel);
    }

    public void Stop()
    {
        StopAllCoroutines();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelGenerator))]
public class GenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LevelGenerator t = (LevelGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            t.Generate();
        }
        if (GUILayout.Button("Stop All"))
        {
            t.Stop();
        }
        DrawDefaultInspector();
    }
}
#endif
