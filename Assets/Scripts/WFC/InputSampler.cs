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
        Dictionary<string, int> TileIndexer;
        Dictionary<string, int> RotationTileIndexer;
        public List<GameObject> availableTiles;
        public RuntimeTile[] runtimeTiles;
        [SerializeField] GameObject empty;
        public Tile[,] sample;
        public int width = 10;
        public int depth = 10;
        public int height = 1;
        public int gridSize = 1;
        public bool drawGizmo;

        public virtual void Train()
        {
            TileIndexer = new Dictionary<string, int>();
            RotationTileIndexer = new Dictionary<string, int>();
            availableTiles = new List<GameObject>();
            runtimeTiles = new RuntimeTile[1000];
            empty = null;
            empty = Resources.Load("Environment/EmptyTile") as GameObject;
            sample = new Tile[width, depth];

            int count = transform.childCount;
            //Finding Available Tiles
            List<GameObject> empties = new List<GameObject>();
            for (int j = 0; j < count; j++)
            {
                GameObject tile = this.transform.GetChild(j).gameObject;
                Vector3 tilepos = tile.transform.localPosition;
                UnityEngine.Object fab = tile;

                if (tile.GetHashCode() == empty.GetHashCode())
                {
                    empties.Add(tile);
                    continue;
                }

#if UNITY_EDITOR
                fab = PrefabUtility.GetCorrespondingObjectFromSource(tile);

                if (fab == null)
                    fab = (GameObject)Resources.Load(tile.name);

                if (!fab)
                {
                    fab = tile;
                }
                tile.name = fab.name;
#endif
                int id = tile.GetHashCode();
                if (!TileIndexer.ContainsValue(id))
                {
                    TileIndexer.Add(tile.name, id);

                    availableTiles.Add(tile);
                }
            }

            foreach (GameObject o in empties)
            {
                if (Application.isPlaying) Destroy(o); else DestroyImmediate(o);
            }

            count = transform.childCount;
            int index = 0;

            //Recording position and rotation of tiles
            for (int i = 0; i < count; i++)
            {
                GameObject tile = this.transform.GetChild(i).gameObject;
                MyTile MyTile = tile.GetComponent<MyTile>();
                Vector3 tilepos = tile.transform.localPosition;

                int X = tilepos.x == 0 ? 0 : Mathf.RoundToInt((tilepos.x) / gridSize);
                int Z = tilepos.z == 0 ? 0 : Mathf.RoundToInt((tilepos.z) / gridSize);
                int R = AngleCorrection(tile, tile.transform.eulerAngles.y);

                if (!RotationTileIndexer.ContainsKey(tile.name + R.ToString()))
                {
                    RotationTileIndexer.Add(tile.name + R.ToString(), index);
                    runtimeTiles[index] = new RuntimeTile(TileIndexer[tile.name], R, MyTile.gameObject);
                    sample[X, Z] = new Tile(RotationTileIndexer[tile.name + R.ToString()]);
                    index++;
                }
                else
                {
                    sample[X, Z] = new Tile(RotationTileIndexer[tile.name + R.ToString()]);
                }

                MyTile.coords = new Vector2Int(X, Z);
            }

            runtimeTiles = runtimeTiles.MySubArray<RuntimeTile>(0, RotationTileIndexer.Count + 1);
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
            tile.transform.eulerAngles = new Vector3(tile.transform.localEulerAngles.x, rot, tile.transform.localEulerAngles.z);
            return (int)rot;
        }

        void RecordEmpty()
        {
            var tile = empty.GetComponent<MyTile>();
            runtimeTiles[RotationTileIndexer.Count] = new RuntimeTile(tile.GetHashCode(), 0, false, empty.gameObject);
            for (int x = 0; x < sample.GetLength(0); x++)
            {
                for (int z = 0; z < sample.GetLength(1); z++)
                {
                    if (sample[x, z].Value == null)
                    {
                        // Debug.Log("is empty at: " + x + ", " + z);

                        GameObject o = Instantiate(empty) as GameObject;

                        if (!RotationTileIndexer.ContainsValue(tile.GetHashCode()))
                        {
                            RotationTileIndexer.Add(empty.name, tile.GetHashCode());

                            if (!TileIndexer.ContainsValue(tile.GetHashCode()))
                            {
                                TileIndexer.Add(empty.name, tile.GetHashCode());
                                availableTiles.Add(o);
                            }
                        }

                        sample[x, z] = WFCUtil.FindTile(runtimeTiles, tile.GetHashCode());
                        o.transform.parent = this.transform;
                        o.transform.localPosition = new Vector3(x * gridSize, 0, z * gridSize);
                    }
                }
            }
        }

        int FindTile(int ID, int rot = 0)
        {
            int index = -1;
            for (int j = 0; j < runtimeTiles.Length; j++)
            {
                if (runtimeTiles[j].ID == ID)
                    index = j;
            }
            return index;
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.magenta;
            if (drawGizmo)
            {
                Gizmos.DrawWireCube(new Vector3((width * gridSize / 2f) - gridSize * 0.5f, height * gridSize / 2f, (depth * gridSize / 2f) - gridSize * 0.5f),
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
            MyWFC.InputSampler t = (MyWFC.InputSampler)target;

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
