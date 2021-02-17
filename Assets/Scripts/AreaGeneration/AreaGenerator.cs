using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration.AreaGeneration
{
    public class AreaGenerator : MonoBehaviour
    {
        [SerializeField] GameObject generatorPrefab;
        public MyWFC.TileSet startAreaTileset;
        public MyWFC.TileSet tileSet;
        public MyWFC.ConnectionGroups connections;
        public MyWFC.PathConstraint.PathConstraintType pathConstraintType;
        public DeBroglie.Wfc.ModelConstraintAlgorithm modelConstraintAlgorithm;
        public bool backTrack = true;
        public int backTrackDepth = 5;

        public List<Area> areaList = new List<Area>();
        public List<MyWFC.AdjacentWFC> generators = new List<MyWFC.AdjacentWFC>();

        private void Start()
        {
            FindPrefabs();
        }

        /// <summary>
        /// Gets the WFC Generator prefab form the Resources folder.
        /// </summary>
        public void FindPrefabs()
        {
            if (generatorPrefab == null)
            {
                generatorPrefab = Resources.Load<GameObject>("AdjacencyWFC");
            }
            // generatorPrefab = Resources.Load<GameObject>("AdjacencyWFC");
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
                    
            GameObject[] areaArr = GameObject.FindGameObjectsWithTag("Area");
            if (areaArr.Length > 0)
            {
                foreach (GameObject a in areaArr)
                    if (Application.isPlaying) Destroy(a); else DestroyImmediate(a);
            }

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
            Area a = new Area(new GameObject("Area " + tile.index), tile);
            areaList.Add(a);

            MyWFC.TileSet t;
            if (tile.index == 0)
                t = startAreaTileset;
            else
                t = tileSet;

            GameObject newGeneratorObj = Instantiate(generatorPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));

            MyWFC.AdjacentWFC generator = newGeneratorObj.GetComponent<MyWFC.AdjacentWFC>();
            newGeneratorObj.transform.parent = a.parent.transform;
            newGeneratorObj.transform.localPosition = new Vector3(-tile.width / 2 * t.GridSize + t.GridSize, 0, -tile.height / 2 * t.GridSize + t.GridSize);
            // newGeneratorObj.transform.position = tile.position;
            // newGeneratorObj.transform.parent = a.parent.transform;

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
            var borderC = new MyWFC.BorderConstraint();
            var fixedC = new MyWFC.FixedTileCostraint();
            var pathC = new MyWFC.PathConstraint();

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

            if (t.borderTiles.Count > 0)
            {
                borderC = generator.gameObject.AddComponent<MyWFC.BorderConstraint>();
                for (int i = 0; i < t.borderTiles.Count; i++)
                {
                    if (!WFCUtil.FindTileUse(t, t.borderTiles[i]) || !t.borderUse[i])
                        continue;
                    else
                        borderC.borderTiles.Add(t.borderTiles[i].GetComponent<MyWFC.MyTile>());
                }
            }

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
            for (int i = 0; i < generators.Count; i++)
            {
                yield return generators[i].Generate(true);
                areaList[i].generated = true;
            }
        }
    }
}