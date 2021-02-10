using System.Collections;
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
        [HideInInspector] public List<bool> pathUse = new List<bool>();
        [HideInInspector] public List<bool> borderUse = new List<bool>();
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


            DrawDefaultInspector();
            //General tile editor
            if (t.pathUse.Count <= 0)
            {
                if (t.pathTiles.Count > 0)
                {
                    t.pathUse = new List<bool>();
                    for (int i = 0; i < t.pathTiles.Count; i++)
                        t.pathUse.Add(true);
                }
                else
                    t.pathUse = new List<bool>();

            }
            if (t.borderUse.Count <= 0)
            {
                if (t.borderTiles.Count > 0)
                {
                    t.borderUse = new List<bool>();
                    for (int i = 0; i < t.borderTiles.Count; i++)
                        t.borderUse.Add(true);
                }
                else
                    t.borderUse = new List<bool>();

            }
            TileEditor(t);
            GUILayout.Space(20f);
            BorderTileEditor(t);
            GUILayout.Space(20f);
            PathTilesEditor(t);

            if (GUILayout.Button("Sort by ID"))
                sortarray(t);

            //Entrance Tile
            GUILayout.Label("Entrance Tile");
            t.entrance = (GameObject)EditorGUILayout.ObjectField(t.entrance, typeof(GameObject), false);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(t);
            }
        }

        void sortarray(TileSet t)
        {
            if (t.tiles.Count > 0)
                t.tiles.Sort(c);
            if (t.borderTiles.Count > 0)
                t.borderTiles.Sort(c);
            if (t.pathTiles.Count > 0)
                t.pathTiles.Sort(c);
        }

        void TileEditor(TileSet t)
        {
            GUILayout.Label("Tileset", EditorStyles.largeLabel);
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

            if (GUILayout.Button("Add Tile"))
            {
                t.tiles.Add(null);
            }
        }
        void BorderTileEditor(TileSet t)
        {

            GUILayout.Label("Border tiles", EditorStyles.largeLabel);
            if (t.borderTiles.Count > 0 && t.borderTiles != null)
                for (int i = 0; i < t.borderTiles.Count; i++)
                {

                    GUILayout.BeginHorizontal();
                    t.borderTiles[i] = (GameObject)EditorGUILayout.ObjectField(t.borderTiles[i], typeof(GameObject), false, GUILayout.MaxWidth(200f));
                    if (t.borderTiles[i] != null)
                    {
                        var MyTile = t.borderTiles[i].GetComponent<MyWFC.MyTile>();

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
                        t.borderUse[i] = EditorGUILayout.Toggle(t.borderUse[i], GUILayout.MaxWidth(20f));

                        if (tempweight != MyTile.weight || tempID != MyTile.ID || tempUse != MyTile.use)
                            PrefabUtility.SavePrefabAsset(t.borderTiles[i]);
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        t.borderTiles.Remove(t.borderTiles[i]);
                        t.borderUse.RemoveAt(i);
                    }
                    GUILayout.EndHorizontal();
                }

            if (GUILayout.Button("Add Tile"))
            {
                t.borderTiles.Add(null);
                t.borderUse.Add(true);
            }
        }
        void PathTilesEditor(TileSet t)
        {
            GUILayout.Label("Path tiles", EditorStyles.largeLabel);
            if (t.pathTiles.Count != 0 && t.pathTiles != null)
                for (int i = 0; i < t.pathTiles.Count; i++)
                {

                    GUILayout.BeginHorizontal();
                    t.pathTiles[i] = (GameObject)EditorGUILayout.ObjectField(t.pathTiles[i], typeof(GameObject), false, GUILayout.MaxWidth(200f));
                    if (t.pathTiles[i] != null)
                    {
                        var MyTile = t.pathTiles[i].GetComponent<MyWFC.MyTile>();

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
                        t.pathUse[i] = EditorGUILayout.Toggle(t.pathUse[i], GUILayout.MaxWidth(20f));

                        if (tempweight != MyTile.weight || tempID != MyTile.ID || tempUse != MyTile.use)
                            PrefabUtility.SavePrefabAsset(t.pathTiles[i]);
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        t.pathTiles.Remove(t.pathTiles[i]);
                        t.pathUse.RemoveAt(i);
                    }
                    GUILayout.EndHorizontal();
                }

            if (GUILayout.Button("Add Tile"))
            {
                t.pathTiles.Add(null);
                t.pathUse.Add(true);
            }
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