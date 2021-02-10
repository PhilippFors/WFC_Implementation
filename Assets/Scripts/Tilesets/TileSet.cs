﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MyWFC
{
    [CreateAssetMenu(fileName = "tile set", menuName = "Tiles/Tileset")]
    public class TileSet : ScriptableObject
    {   
        public int GridSize;
        [HideInInspector] public List<GameObject> tiles = new List<GameObject>();

        [HideInInspector] public GameObject entrance;

        [HideInInspector] public List<GameObject> borderTiles = new List<GameObject>();
        [HideInInspector] public List<GameObject> pathTiles = new List<GameObject>();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TileSet))]
    [CanEditMultipleObjects]
    public class TileSetEditor : Editor
    {
        TilesetCompare c = new TilesetCompare();
        public override void OnInspectorGUI()
        {
            TileSet t = (TileSet)target;
            
            GUILayout.Label("Tileset", EditorStyles.largeLabel);
            DrawDefaultInspector();
            if (t.tiles.Count != 0 && t.tiles != null)
                for (int i = 0; i < t.tiles.Count; i++)
                {

                    GUILayout.BeginHorizontal();
                    t.tiles[i] = (GameObject)EditorGUILayout.ObjectField(t.tiles[i], typeof(GameObject), false, GUILayout.MaxWidth(200f));
                    if (t.tiles[i] != null)
                    {
                        var MyTile = t.tiles[i].GetComponent<MyWFC.MyTile>();

                        var tempweight = MyTile.weight;
                        var tempID = MyTile.ID;
                        var tempUse = MyTile.use;

                        GUILayout.Label("Weight: ", GUILayout.MaxWidth(50f));
                        MyTile.weight = EditorGUILayout.IntField(MyTile.weight, GUILayout.MaxWidth(30f));

                        GUILayout.Space(5f);

                        GUILayout.Label("ID: ", GUILayout.MaxWidth(25f));
                        MyTile.ID = EditorGUILayout.IntField(MyTile.ID, GUILayout.MaxWidth(30f));

                        GUILayout.Space(5f);

                        GUILayout.Label("Use: ", GUILayout.MaxWidth(25f));
                        MyTile.use = EditorGUILayout.Toggle(MyTile.use, GUILayout.MaxWidth(20f));

                        if (tempweight != MyTile.weight || tempID != MyTile.ID || tempUse != MyTile.use)
                            PrefabUtility.SavePrefabAsset(t.tiles[i]);
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        t.tiles.Remove(t.tiles[i]);
                    }
                    GUILayout.EndHorizontal();
                }

            GUILayout.Label("Entrance Tile");
            t.entrance = (GameObject)EditorGUILayout.ObjectField(t.entrance, typeof(GameObject), false);

            if (GUILayout.Button("Add Tile"))
            {
                t.tiles.Add(null);
            }

            if (GUILayout.Button("Sort by ID"))
                sortarray(t);


            if (GUI.changed)
            {
                EditorUtility.SetDirty(t);
            }
        }

        void sortarray(TileSet t)
        {
            t.tiles.Sort(c);
        }
    }

    public class TilesetCompare : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            return x.GetComponent<MyWFC.MyTile>().ID.CompareTo(y.GetComponent<MyWFC.MyTile>().ID);
        }
    }
#endif
}