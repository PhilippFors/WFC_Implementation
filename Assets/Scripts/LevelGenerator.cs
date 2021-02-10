using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace LevelGeneration
{
    [ExecuteInEditMode]
    public class LevelGenerator : MonoBehaviour
    {
        public GridGeneration.GridGenerator gridGenerator;
        public AreaGeneration.AreaGenerator areaGenerator;

        private void Start()
        {
            // Generate();
        }

        public void Generate()
        {
            gridGenerator.dic["gridSize"] = areaGenerator.tileSet.GridSize;
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
}