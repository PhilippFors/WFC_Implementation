using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using DeBroglie;
using Utils;

namespace MyWFC
{
    [ExecuteInEditMode]
    public class InputSampler : MonoBehaviour
    {
        public RuntimeTile[] RuntimeTiles => runtimeTiles;
        public Tile[,,] Sample => sample;

        [SerializeField] private List<GameObject> availableTiles;
        [SerializeField] private RuntimeTile[] runtimeTiles;
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
            runtimeTiles = new RuntimeTile[1000];
            sample = new Tile[width, height, depth];

            var count = transform.childCount;
            
            // Indexing the availiable tiles
            for (int j = 0; j < count; j++)
            {
                var inputTile = transform.GetChild(j).gameObject;

#if UNITY_EDITOR
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(inputTile);

                if (prefab == null)
                    prefab = (GameObject) Resources.Load(inputTile.name);

                if (!prefab)
                {
                    prefab = inputTile;
                }

                inputTile.name = prefab.name;
#endif

                var myTile = inputTile.GetComponent<MyTile>();
                int id = myTile.GetHashCode();

                var find = availableTiles.Find(x => x.GetComponent<MyTile>().GetHashCode() == myTile.GetHashCode());
                if (find)
                {
                    inputTile.name = find.name;
                }
                
                if (!tileIndexer.ContainsValue(id))
                {
                    tileIndexer.Add(inputTile.name, id);
                }
            }
            
            int index = 0;

            // Recording position and rotation of tiles
            for (int i = 0; i < count; i++)
            {
                GameObject tile = transform.GetChild(i).gameObject;
                MyTile myTile = tile.GetComponent<MyTile>();
                Vector3 tilepos = tile.transform.localPosition;

                int x = tilepos.x == 0 ? 0 : Mathf.RoundToInt((tilepos.x) / gridSize);
                int y = tilepos.x == 0 ? 0 : Mathf.RoundToInt((tilepos.y) / gridSize);
                int z = tilepos.z == 0 ? 0 : Mathf.RoundToInt((tilepos.z) / gridSize);
                int rot = Mathf.RoundToInt(tile.transform.eulerAngles.y);

                if (!rotationTileIndexer.ContainsKey(tile.name + rot))
                {
                    rotationTileIndexer.Add(tile.name + rot, index);
                    runtimeTiles[index] = new RuntimeTile(tileIndexer[tile.name], rot, myTile);
                    sample[x, y, z] = new Tile(rotationTileIndexer[tile.name + rot]);
                    index++;
                }
                else
                {
                    sample[x, y, z] = new Tile(rotationTileIndexer[tile.name + rot]);
                }
            }

            runtimeTiles = runtimeTiles.TrimArray<RuntimeTile>(0, rotationTileIndexer.Count);
            RecordEmpty();
        }

        // Any Empty spots in the sample get a value of -1
        private void RecordEmpty()
        {
            for (int x = 0; x < sample.GetLength(0); x++)
            {
                for (int y = 0; y < sample.GetLength(1); y++)
                {
                    for (int z = 0; z < sample.GetLength(2); z++)
                    {
                        if (sample[x, y, z].Value == null)
                        {
                            sample[x, y, z] = new Tile(-1);
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
}