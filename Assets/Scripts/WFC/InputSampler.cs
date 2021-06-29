using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using DeBroglie;
using DeBroglie.Rot;

namespace MyWFC
{
    [ExecuteInEditMode]
    public class InputSampler : MonoBehaviour
    {
        public RuntimeTile[] RuntimeTiles => runtimeTiles;
        public Tile[,,] Sample => sample;

        [SerializeField] private List<GameObject> availableTiles;
        [SerializeField] private RuntimeTile[] runtimeTiles;
        [SerializeField] GameObject empty;
        [SerializeField] private int width = 10;
        [SerializeField] private int depth = 10;
        [SerializeField] private int height = 1;
        [SerializeField] private int gridSize = 1;
        [SerializeField] private bool drawGizmo;

        private Tile[,,] sample;
        private Dictionary<string, int> tileIndexer;
        private Dictionary<string, int> rotationTileIndexer;

        public virtual void Train()
        {
            tileIndexer = new Dictionary<string, int>();
            rotationTileIndexer = new Dictionary<string, int>();
            availableTiles = new List<GameObject>();
            runtimeTiles = new RuntimeTile[1000];
            empty = null;
            empty = Resources.Load("Environment/EmptyTile") as GameObject;
            sample = new Tile[width, height, depth];

            var count = transform.childCount;
            
            //Finding Available Tiles
            List<GameObject> empties = new List<GameObject>();
            for (int j = 0; j < count; j++)
            {
                GameObject tile = transform.GetChild(j).gameObject;

                var fab = tile;

                if (tile.GetHashCode() == empty.GetHashCode())
                {
                    empties.Add(tile);
                    continue;
                }

#if UNITY_EDITOR
                fab = PrefabUtility.GetCorrespondingObjectFromSource(tile);

                if (fab == null)
                    fab = (GameObject) Resources.Load(tile.name);

                if (!fab)
                {
                    fab = tile;
                }

                tile.name = fab.name;
#endif
                int id = tile.GetHashCode();
                if (!tileIndexer.ContainsValue(id))
                {
                    tileIndexer.Add(tile.name, id);

                    availableTiles.Add(tile);
                }
            }

            foreach (GameObject o in empties)
            {
                if (Application.isPlaying) Destroy(o);
                else DestroyImmediate(o);
            }

            count = transform.childCount;
            int index = 0;

            //Recording position and rotation of tiles
            for (int i = 0; i < count; i++)
            {
                GameObject tile = this.transform.GetChild(i).gameObject;
                MyTile MyTile = tile.GetComponent<MyTile>();
                Vector3 tilepos = tile.transform.localPosition;

                int x = tilepos.x == 0 ? 0 : Mathf.RoundToInt((tilepos.x) / gridSize);
                int y = tilepos.x == 0 ? 0 : Mathf.RoundToInt((tilepos.y) / gridSize);
                int z = tilepos.z == 0 ? 0 : Mathf.RoundToInt((tilepos.z) / gridSize);
                int rot = AngleCorrection(tile, tile.transform.eulerAngles.y);

                if (!rotationTileIndexer.ContainsKey(tile.name + rot.ToString()))
                {
                    rotationTileIndexer.Add(tile.name + rot.ToString(), index);
                    runtimeTiles[index] = new RuntimeTile(tileIndexer[tile.name], rot, MyTile.gameObject);
                    sample[x, y, z] = new Tile(rotationTileIndexer[tile.name + rot.ToString()]);
                    index++;
                }
                else
                {
                    sample[x, y, z] = new Tile(rotationTileIndexer[tile.name + rot.ToString()]);
                }

                MyTile.coords = new Vector2Int(x, z);
            }

            runtimeTiles = runtimeTiles.MySubArray<RuntimeTile>(0, rotationTileIndexer.Count + 1);
            RecordEmpty();
        }

        int AngleCorrection(GameObject tile, float R)
        {
            int rot = 0;

            if (R < 0f)
            {
                if (R >= -1f & R <= 1f || R <= -359f && R >= -361f)
                    rot = 0;
                if (R <= -89f & R >= -91f)
                    rot = 270;
                if (R <= -179f & R >= -181f)
                    rot = 180;
                if (R <= -269f & R >= -271f)
                    rot = 90;
            }
            else if (R >= 0f)
            {
                if (R > -1f & R < 1f || R >= 359f && R <= 361f)
                    rot = 0;
                if (R > 89f & R < 91f)
                    rot = 90;
                if (R > 179f & R < 181f)
                    rot = 180;
                if (R > 269f & R < 271f)
                    rot = 270;
            }

            tile.transform.eulerAngles =
                new Vector3(tile.transform.localEulerAngles.x, rot, tile.transform.localEulerAngles.z);
            return (int) rot;
        }

        private void RecordEmpty()
        {
            var tile = empty.GetComponent<MyTile>();
            runtimeTiles[rotationTileIndexer.Count] = new RuntimeTile(tile.gameObject.GetHashCode(), 0, false, tile);
            for (int x = 0; x < sample.GetLength(0); x++)
            {
                for (int y = 0; y < sample.GetLength(1); y++)
                {
                    for (int z = 0; z < sample.GetLength(2); z++)
                    {
                        if (sample[x, y, z].Value == null)
                        {
                            // Debug.Log("is empty at: " + x + ", " + z);

                            GameObject o = Instantiate(empty) as GameObject;

                            if (!rotationTileIndexer.ContainsValue(tile.GetHashCode()))
                            {
                                rotationTileIndexer.Add(empty.name, tile.GetHashCode());

                                if (!tileIndexer.ContainsValue(tile.GetHashCode()))
                                {
                                    tileIndexer.Add(empty.name, tile.GetHashCode());
                                    availableTiles.Add(o);
                                }
                            }

                            sample[x, y, z] = WFCUtil.FindTile(runtimeTiles, tile.GetHashCode());
                            o.transform.parent = this.transform;
                            o.transform.localPosition = new Vector3(x * gridSize, y * gridSize, z * gridSize);
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.magenta;
            if (drawGizmo)
            {
                Gizmos.DrawWireCube(
                    new Vector3((width * gridSize / 2f) - gridSize * 0.5f, height * gridSize / 2f,
                        (depth * gridSize / 2f) - gridSize * 0.5f),
                    new Vector3(width * gridSize, height * gridSize, depth * gridSize));
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < this.transform.childCount; i++)
            {
                GameObject tile = this.transform.GetChild(i).gameObject;
                Vector3 tilepos = tile.transform.localPosition;
                if ((tilepos.x > -0.55f) && (tilepos.x <= width * gridSize - 0.55f) &&
                    (tilepos.z > -0.55f) && (tilepos.z <= depth * gridSize - 0.55f))
                {
                    Gizmos.DrawSphere(tilepos, gridSize * 0.1f);
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MyWFC.InputSampler), true)]
    public class InputEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MyWFC.InputSampler t = (MyWFC.InputSampler) target;

            if (GUILayout.Button("Train"))
            {
                t.Train();
            }

            DrawDefaultInspector();
        }
    }
#endif

    public static class MyExtension
    {
        public static T[] MySubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}