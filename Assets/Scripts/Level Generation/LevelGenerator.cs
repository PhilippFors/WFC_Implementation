using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace LevelGeneration
{
    [ExecuteInEditMode]
    public class LevelGenerator : MonoBehaviour
    {
        public bool generateOnStart = false;

        #region Area generation
        
        [Header("Area Generation")]
        [SerializeField] GameObject generatorPrefab;
        public MyWFC.TileSet startAreaTileset;
        public MyWFC.TileSet tileSet;
        public MyWFC.ConnectionGroups connections;
        public MyWFC.PathConstraint.PathConstraintType pathConstraintType;
        public DeBroglie.Wfc.ModelConstraintAlgorithm modelConstraintAlgorithm;
        public bool backTrack = true;
        public int backTrackDepth = 5;
        [HideInInspector] public List<Area> areaList = new List<Area>();
        [HideInInspector] public List<MyWFC.AdjacentWFC> generators = new List<MyWFC.AdjacentWFC>();

        #endregion

        #region Grid Generator

        [Header("Grid Generation")]
        public int gridWidth;
        public int gridHeight;
        public int levelLength;
        public int seed;
        public bool randomSeed;
        public bool randomLength;
        public bool isLinear = true;
        public StringDictionary dic = new StringDictionary();
        public Directions[] directions;

        #endregion

        private void Start()
        {
            if (generateOnStart)
                Generate();
        }

        public void Generate()
        {
            //Creating a new gridlayout
            var gridGen = new GridGeneration.GridGenerator(gridWidth, gridHeight, levelLength, seed, randomSeed, randomLength, dic, directions, this.transform.position);
            gridGen.dic["gridSize"] = tileSet.GridSize;
            gridGen.GenerateGrid();

            //Locating the generator prefab in the resources folder
            FindPrefabs();

            StartAreaGenerator(gridGen.grid.linearLevel);
        }

        #region Area Generation

        /// <summary>
        /// Loads the WFC Generator prefab form the Resources folder.
        /// </summary>
        public void FindPrefabs()
        {
            if (generatorPrefab == null)
            {
                generatorPrefab = Resources.Load<GameObject>("AdjacencyWFC");
            }
        }

        /// <summary>
        /// Initates the generation process for each Area.
        /// </summary>
        /// <param name="tile"></param>
        public void StartAreaGenerator(GridGeneration.GridTile[] tile)
        {
            generators.Clear();

            if (areaList.Count > 0)
                foreach (Area a in areaList)
                    if (Application.isPlaying) Destroy(a.parent); else DestroyImmediate(a.parent);

            areaList.Clear();

            foreach (GridGeneration.GridTile t in tile)
            {
                GenerateArea(t);
            }
            StartCoroutine(StartGenerator());
            StartCoroutine(CheckForFinsihed());
        }

        IEnumerator CheckForFinsihed()
        {
            while (!CheckList())
            {
                yield return null;
            }

            Debug.Log("All Areas Generated!");
        }

        bool CheckList()
        {
            int i = 0;
            foreach (Area a in areaList)
            {
                if (a.generated)
                    i++;
            }
            return i == areaList.Count;
        }

        /// <summary>
        /// Setting up the Area and the WFC generator.
        /// </summary>
        /// <param name="tile"></param>
        void GenerateArea(GridGeneration.GridTile tile)
        {
            //Creating a new Area with a new gameobject as its parent
            Area a = new Area(new GameObject("Area " + tile.index), tile);
            areaList.Add(a);

            //Choosing a tileset depending on whether it is the start area or not
            MyWFC.TileSet t;
            if (tile.index == 0)
                t = startAreaTileset;
            else
                t = tileSet;

            //Instantiate a new generator
            GameObject newGeneratorObj = Instantiate(generatorPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));

            //Configuring the generator
            MyWFC.AdjacentWFC generator = newGeneratorObj.GetComponent<MyWFC.AdjacentWFC>();
            newGeneratorObj.transform.parent = a.parent.transform;
            newGeneratorObj.transform.localPosition = new Vector3(-tile.width / 2 * t.GridSize + t.GridSize, 0, -tile.height / 2 * t.GridSize + t.GridSize);

            generator.tileSet = t;
            generator.ConnectionGroups = connections;
            generator.gridSize = t.GridSize;
            generator.size = new Vector3Int(tile.width, 1, tile.height);
            generator.modelConstraintAlgorithm = modelConstraintAlgorithm;
            generator.backTrackDepth = backTrackDepth;
            generator.backTrack = backTrack;
            AddConstraints(a, generator, t);

            generators.Add(generator);
        }

        /// <summary>
        /// Checks the tileset if there are any possible constraints to use and adds the corresponding component to the generator gameObject.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="generator"></param>
        void AddConstraints(Area area, MyWFC.AdjacentWFC generator, MyWFC.TileSet t)
        {
            MyWFC.BorderConstraint borderC = null;
            MyWFC.FixedTileCostraint fixedC = null;
            MyWFC.PathConstraint pathC = null;

            //Configuring fixed tile constraint
            if (t.entrance != null)
            {
                fixedC = generator.gameObject.AddComponent<MyWFC.FixedTileCostraint>();

                var entrance = area.tileInfo.entrance;
                var exit = area.tileInfo.exit;

                if (entrance != null)
                    fixedC.pointList.Add(new MyWFC.MyTilePoint() { point = entrance.position, tile = t.entrance.GetComponent<MyWFC.MyTile>() });
                if (exit != null)
                    fixedC.pointList.Add(new MyWFC.MyTilePoint() { point = exit.position, tile = t.entrance.GetComponent<MyWFC.MyTile>() });
            }

            //Configuring border constraint
            if (t.borderTiles.Count > 0)
            {
                borderC = generator.gameObject.AddComponent<MyWFC.BorderConstraint>();
                borderC.respectFixedConstraint = fixedC != null;
                borderC.sides = MyWFC.Borders.XMax | MyWFC.Borders.XMin | MyWFC.Borders.ZMax | MyWFC.Borders.ZMin;
                for (int i = 0; i < t.borderTiles.Count; i++)
                {
                    if (!WFCUtil.FindTileUse(t, t.borderTiles[i]) || !t.borderUse[i])
                        continue;
                    else
                        borderC.borderTiles.Add(t.borderTiles[i].GetComponent<MyWFC.MyTile>());
                }
            }

            //Configuring path constraint
            if (t.pathTiles.Count > 0)
            {
                pathC = generator.gameObject.AddComponent<MyWFC.PathConstraint>();
                pathC.pathConstraintType = pathConstraintType;
                if (fixedC != null && fixedC.useConstraint)
                    foreach (MyWFC.MyTilePoint p in fixedC.pointList)
                        pathC.endPoints.Add(p);

                for (int i = 0; i < t.pathTiles.Count; i++)
                {
                    if (!WFCUtil.FindTileUse(t, t.pathTiles[i]) || !t.pathUse[i])
                        continue;
                    else
                        pathC.pathTiles.Add(t.pathTiles[i].GetComponent<MyWFC.MyTile>());
                }
                pathC.pathTiles.Add(t.entrance.GetComponent<MyWFC.MyTile>());
            }
        }

        /// <summary>
        /// Starting the WFC Generators.
        /// </summary>
        /// <returns></returns>
        IEnumerator StartGenerator()
        {
            //Starting the generation of each generator in the order they were created
            for (int i = 0; i < generators.Count; i++)
            {
                yield return generators[i].Generate(true);
                areaList[i].generated = true;
            }
        }

        #endregion

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
            // if (GUILayout.Button("Stop All"))
            // {
            //     t.Stop();
            // }
            DrawDefaultInspector();
        }
    }
#endif
}