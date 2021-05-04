using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MyWFC
{
    [CreateAssetMenu(fileName = "new tile set", menuName = "Tiles/Tileset")]
    public class TileSet : ScriptableObject
    {
        [SerializeField] private int gridSize;
        public int GridSize => gridSize;

        /// <summary>
        /// The main tilset. Every used tile should be in here.
        /// </summary>
        /// <typeparam name="GameObject"></typeparam>
        [HideInInspector] public List<GameObject> tiles = new List<GameObject>();

        [HideInInspector] public GameObject entrance;
        [HideInInspector] public GameObject empty;

        [HideInInspector] public List<double> frequenzies = new List<double>();
        [HideInInspector] public List<bool> tileUse = new List<bool>();
        [HideInInspector] public List<bool> pathUse = new List<bool>();
        [HideInInspector] public List<bool> borderUse = new List<bool>();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TileSet))]
    [CanEditMultipleObjects]
    public class TileSetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TileSet t = (TileSet)target;

            DrawDefaultInspector();
            TileEditor(t);
            GUILayout.Space(20f);
            
            GUILayout.Label("Entrance Tile");
            t.entrance = (GameObject)EditorGUILayout.ObjectField(t.entrance, typeof(GameObject), false);
            GUILayout.Label("Empty Tile");
            t.empty = (GameObject)EditorGUILayout.ObjectField(t.empty, typeof(GameObject), false);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(t);
            }
            if (GUILayout.Button("Save all prefabs"))
            {
                foreach (GameObject obj in t.tiles)
                    PrefabUtility.SavePrefabAsset(obj);
            }
        }

        void TileEditor(TileSet t)
        {
            GUILayout.Label("Tileset", EditorStyles.largeLabel);
            GUILayout.Space(30f);
            if (t.tiles.Count != 0 && t.tiles != null)
                for (int i = 0; i < t.tiles.Count; i++)
                {

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40f);
                    if (t.tiles[i] != null)
                    {
                        var preview = AssetPreview.GetAssetPreview(t.tiles[i]);
                        if (!AssetPreview.IsLoadingAssetPreview(t.tiles[i].GetInstanceID()) && preview != null)
                            EditorGUI.DrawPreviewTexture(new Rect(0, 61f * (i + 0) + 75f, 60f, 60f), preview);
                    }
                    t.tiles[i] = (GameObject)EditorGUILayout.ObjectField(t.tiles[i], typeof(GameObject), false, GUILayout.MaxWidth(200f));

                    if (t.tiles[i] != null)
                    {
                        GUILayout.Label("Weight: ", GUILayout.MaxWidth(50f));
                        t.frequenzies[i] = EditorGUILayout.DoubleField(t.frequenzies[i], GUILayout.MaxWidth(30f));

                        GUILayout.Space(5f);

                        GUILayout.Label("Use: ", GUILayout.MaxWidth(25f));
                        t.tileUse[i] = EditorGUILayout.Toggle(t.tileUse[i], GUILayout.MaxWidth(25f));

                        GUILayout.Label("B: ", GUILayout.MaxWidth(20f));
                        t.borderUse[i] = EditorGUILayout.Toggle(t.borderUse[i], GUILayout.MaxWidth(20f));

                        GUILayout.Label("P: ", GUILayout.MaxWidth(20f));
                        t.pathUse[i] = EditorGUILayout.Toggle(t.pathUse[i], GUILayout.MaxWidth(20f));
                    }

                    if (GUILayout.Button("Remove"))
                    {
                        t.tiles.Remove(t.tiles[i]);
                        t.tileUse.RemoveAt(i);
                        t.borderUse.RemoveAt(i);
                        t.pathUse.RemoveAt(i);
                        t.frequenzies.RemoveAt(i);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(40f);
                }

            if (GUILayout.Button("Add Tile"))
            {
                t.tiles.Add(null);
                t.tileUse.Add(true);
                t.borderUse.Add(false);
                t.pathUse.Add(false);
                t.frequenzies.Add(1);
            }
        }
    }
#endif
}