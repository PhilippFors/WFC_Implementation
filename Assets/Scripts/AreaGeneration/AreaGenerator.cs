using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration.AreaGeneration
{
    public class AreaGenerator : MonoBehaviour
    {
        [SerializeField] GameObject generatorPrefab;
        public MyWFC.TileSet tileSet;
        public MyWFC.ConnectionGroups connections;
        public List<Area> areaList = new List<Area>();
        public List<MyWFC.AdjacentWFC> generators = new List<MyWFC.AdjacentWFC>();
        public bool loaded = false;
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
            if (areaList.Count != 0)
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
            Area a = new Area(new GameObject("Area " + tile.index), tile);
            areaList.Add(a);

            GameObject newGeneratorObj = Instantiate(generatorPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0));

            MyWFC.AdjacentWFC generator = newGeneratorObj.GetComponent<MyWFC.AdjacentWFC>();
            newGeneratorObj.transform.parent = a.parent.transform;
            newGeneratorObj.transform.localPosition = new Vector3(-tile.width / 2 * tileSet.GridSize + tileSet.GridSize, 0, -tile.height / 2 * tileSet.GridSize + tileSet.GridSize);
            // newGeneratorObj.transform.position = tile.position;
            // newGeneratorObj.transform.parent = a.parent.transform;

            generator.tileSet = tileSet;
            generator.ConnectionGroups = connections;
            generator.gridSize = tileSet.GridSize;
            generator.size = new Vector3Int(tile.width, 1, tile.height);

            AddConstraints(a, generator);

            generators.Add(generator);
        }

        /// <summary>
        /// Checks the tileset if there are any possible constraints to use and adds the corresponding component to the generator gameObject.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="generator"></param>
        void AddConstraints(Area area, MyWFC.AdjacentWFC generator)
        {
            var borderC = new MyWFC.BorderConstraint();
            var fixedC = new MyWFC.FixedTileCostraint();
            var pathC = new MyWFC.PathConstraint();

            if (tileSet.entrance != null)
            {
                fixedC = generator.gameObject.AddComponent<MyWFC.FixedTileCostraint>();

                var entrance = area.tileInfo.entrance;
                var exit = area.tileInfo.exit;

                if (entrance != null)
                    fixedC.pointList.Add(new MyWFC.MyTilePoint() { point = entrance.position, tile = tileSet.entrance.GetComponent<MyWFC.MyTile>() });
                if (exit != null)
                    fixedC.pointList.Add(new MyWFC.MyTilePoint() { point = exit.position, tile = tileSet.entrance.GetComponent<MyWFC.MyTile>() });
            }

            if (tileSet.borderTiles.Count > 0)
            {
                borderC = generator.gameObject.AddComponent<MyWFC.BorderConstraint>();
                for (int i = 0; i < tileSet.borderTiles.Count; i++)
                {
                    if (!tileSet.borderUse[i] || !tileSet.borderTiles[i].GetComponent<MyWFC.MyTile>().use)
                        continue;
                    else
                        borderC.borderTiles.Add(tileSet.borderTiles[i].GetComponent<MyWFC.MyTile>());
                }
            }

            if (tileSet.pathTiles.Count > 0)
            {
                pathC = generator.gameObject.AddComponent<MyWFC.PathConstraint>();
                if (fixedC != null && fixedC.useConstraint)
                    foreach (MyWFC.MyTilePoint p in fixedC.pointList)
                        pathC.endPoints.Add(p);

                for (int i = 0; i < tileSet.pathTiles.Count; i++)
                {
                    if (!tileSet.pathUse[i] || !tileSet.pathTiles[i].GetComponent<MyWFC.MyTile>().use)
                        continue;
                    else
                        pathC.pathTiles.Add(tileSet.pathTiles[i].GetComponent<MyWFC.MyTile>());
                }
                pathC.pathTiles.Add(tileSet.entrance.GetComponent<MyWFC.MyTile>());
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