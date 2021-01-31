using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MyWFC
{
    [CreateAssetMenu(fileName = "tile set", menuName = "Tiles/Tileset")]
    public class TileSet : ScriptableObject
    {
        public List<GameObject> tiles = new List<GameObject>();

        public GameObject entrance;
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

            if (t.tiles.Count != 0 && t.tiles != null)
                for (int i = 0; i < t.tiles.Count; i++)
                {

                    GUILayout.BeginHorizontal();
                    t.tiles[i] = (GameObject)EditorGUILayout.ObjectField(t.tiles[i], typeof(GameObject), false);
                    if (t.tiles[i] != null)
                    {
                        var MyTile = t.tiles[i].GetComponent<MyWFC.MyTile>();
                        MyTile.weight = EditorGUILayout.IntField(MyTile.weight);
                        GUILayout.Label("ID: ");
                        MyTile.ID = EditorGUILayout.IntField(MyTile.ID);
                    }
                    if (GUILayout.Button("Remove"))
                    {
                        t.tiles.Remove(t.tiles[i]);
                    }
                    GUILayout.EndHorizontal();
                }


            if (GUILayout.Button("Add Tile"))
            {
                t.tiles.Add(null);
            }

            if (GUILayout.Button("Sort by ID"))
                sortarray(t);

            // if (GUILayout.Button("Add all in folder"))
            // {
            //     t.tiles = new List<GameObject>();
            //     string path = AssetDatabase.GetAssetPath(t);
            //     string[] guids = AssetDatabase
            //     string[] paths = new string[guids.Length];
            //     for (int i = 0; i < guids.Length; i++)
            //     {
            //         paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
            //     }

            //     Object[] arr = new Object[paths.Length];
            //     for (int i = 0; i < paths.Length; i++)
            //     {
            //         t.tiles.Add((GameObject)AssetDatabase.LoadAssetAtPath(paths[i], typeof(MyTile)));
            //     }

            // }
            t.entrance = (GameObject)EditorGUILayout.ObjectField(t.entrance, typeof(GameObject), false);
            if (GUI.changed)
            {
                AssetDatabase.SaveAssets();
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