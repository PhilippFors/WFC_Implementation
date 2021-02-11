using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration.GridGeneration
{
    public class GridGenerator : MonoBehaviour
    {
        public int gridWidth;
        public int gridHeight;
        public int levelLength;
        public int seed;
        public bool randomSeed;
        public bool randomLength;
        public bool isLinear = true;
        public StringDictionary dic = new StringDictionary();
        public Directions[] directions;
        [SerializeField] GridTile[] arr;

        [Header("Gizmo settings")]
        public bool drawGizmo;
        int totalWeight
        {
            get
            {
                int w = 0;
                foreach (Directions d in directions)
                    w += d.weight;
                return w;
            }
        }

        public Grid grid;

        /// <summary>
        /// Starting the grid generation process.
        /// </summary>
        public void GenerateGrid()
        {
            GetSeed();

            if (randomLength)
                levelLength = Random.Range(4, 8);

            grid = new Grid(gridWidth, gridHeight, levelLength, seed, totalWeight, directions, dic, isLinear, this.gameObject);
            arr = grid.linearLevel;
        }

        /// <summary>
        /// Sets a seed chosen by the user or uses a random seed
        /// </summary>
        void GetSeed()
        {
            if (randomSeed)
            {
                string s = "";
                for (int i = 0; i < Random.Range(4, 8); i++)
                {
                    s += Random.Range(0, 10);
                }
                seed = int.Parse(s);
            }
            Random.InitState(seed);
        }

        private void OnDrawGizmos()
        {
            if (drawGizmo)
                if (grid != null)
                    for (int i = 0; i < grid.linearLevel.Length; i++)
                    {
                        if (grid.linearLevel[i] != null)
                        {
                            Gizmos.color = Color.black;
                            Gizmos.DrawCube(grid.linearLevel[i].position, new Vector3(grid.linearLevel[i].width, 1, grid.linearLevel[i].height));

                            Gizmos.color = Color.green;
                            Gizmos.DrawWireCube(grid.linearLevel[i].bounds.center, grid.linearLevel[i].bounds.size);

                            Gizmos.color = Color.yellow;
                            var bounds = QuantizedBounds(grid.linearLevel[i].bounds.center, grid.linearLevel[i].bounds.size);
                            Gizmos.DrawWireCube(bounds.center, bounds.size);
                        }
                    }
        }

        Bounds QuantizedBounds(Vector3 center, Vector3 size)
        {
            // Quantize the bounds to update only when theres a 10% change in size
            return new Bounds(Quantize(center, 0.1f * size), size);
        }
        Vector3 Quantize(Vector3 v, Vector3 quant)
        {
            float x = quant.x * Mathf.Floor(v.x / quant.x);
            float y = quant.y * Mathf.Floor(v.y / quant.y);
            float z = quant.z * Mathf.Floor(v.z / quant.z);
            return new Vector3(x, y, z);
        }

    }

    /// <summary>
    /// Holds the grid on which all of the gridtiles sit.
    /// </summary>
    public class Grid
    {
        public GridTile[] linearLevel;
        LinearGenerator linearGenerator;
        public Grid(int height, int width, int length, int seed, int _weight, Directions[] _directions, StringDictionary _dic, bool isLinear, GameObject gameOBJ)
        {
            linearLevel = new GridTile[length + 1];

            if (isLinear)
            {
                LinearGenerator l = new LinearGenerator(height, width, length, _weight, _directions, _dic, gameOBJ);

                linearLevel = l.TransferToArray(l.GenerateTiles());
            }
        }
    }

    /// <summary>
    /// Base class for all subsequent grid generators.
    /// </summary>
    public class MyGridGenerator
    {
        protected GridTile[,] tileGrid;
        protected int oldX;
        protected int oldY;
        protected int height;
        protected int width;
        protected int length;
        protected int totalWeight;
        protected Directions[] directions;
        protected StringDictionary dic;
        GameObject obj;
        public MyGridGenerator(int _height, int _width, int _length, int _weight, Directions[] _directions, StringDictionary _dic, GameObject gameOBJ)
        {
            totalWeight = _weight;
            height = _height;
            width = _width;
            directions = _directions;
            length = _length;
            dic = _dic;
            obj = gameOBJ;
        }

        /// <summary>
        /// Generates a new set of tiles.
        /// </summary>
        /// <returns></returns>
        public GridTile[,] GenerateTiles()
        {
            tileGrid = new GridTile[height, width];

            oldX = tileGrid.GetLength(0) / 2;
            oldY = tileGrid.GetLength(1) / 2;
            int r = Random.Range(0, 4);

            //Setting the first tile from which everything starts. Has no entrance.
            tileGrid[oldX, oldY] = new GridTile(obj.transform.position, 0, dic["baseTileSize"], dic["baseTileSize"]);

            for (int i = 1; i < length + 1; i++)
            {
                Directions newD = GetRandomDirection();

                switch (newD.direction)
                {
                    case Side.LEFT:
                        AddNewTile(oldX - 1, oldY, i, Side.LEFT);
                        break;
                    case Side.UP:
                        AddNewTile(oldX, oldY + 1, i, Side.UP);
                        break;
                    case Side.RIGHT:
                        AddNewTile(oldX + 1, oldY, i, Side.RIGHT);
                        break;
                    case Side.DOWN:
                        AddNewTile(oldX, oldY - 1, i, Side.DOWN);
                        break;
                }
            }
            return tileGrid;
        }

        /// <summary>
        /// Adds a new Tile to the grid at x, y position
        /// </summary>
        /// <param name="x">The x position on the grid</param>
        /// <param name="y">The y position on the grid</param>
        /// <param name="index">Tile index</param>
        /// <param name="d">The direction in which the new grid is placed</param>
        void AddNewTile(int x, int y, int index, Side d)
        {
            //if tile placement is illegal, recursive Method call happens until legal placement is found
            if (x < 0 || x >= tileGrid.GetLength(0) || y < 0 || y >= tileGrid.GetLength(1) || tileGrid[x, y] != null)
            {
                Directions dir = GetRandomDirection();
                switch (dir.direction)
                {
                    case Side.LEFT:
                        AddNewTile(oldX - 1, oldY, index, Side.LEFT);
                        break;
                    case Side.UP:
                        AddNewTile(oldX, oldY + 1, index, Side.UP);
                        break;
                    case Side.RIGHT:
                        AddNewTile(oldX + 1, oldY, index, Side.RIGHT);
                        break;
                    case Side.DOWN:
                        AddNewTile(oldX, oldY - 1, index, Side.DOWN);
                        break;
                }
                return;
            }


            int tileWidth = Random.Range(dic["minTileWidth"], dic["maxTileWidth"] + 1);

            int tileHeight = Random.Range(dic["minTileHeight"], dic["maxTileHeight"] + 1);

            //Creating new tile with real world position
            GridTile t = new GridTile(tileGrid[oldX, oldY].position + new Vector3(((tileWidth / 2 + dic["tileDistance"] * dic["gridSize"]) + (tileGrid[oldX, oldY].width / 2 * dic["gridSize"])) * (x - oldX), 0, ((tileHeight / 2 + dic["tileDistance"] * dic["gridSize"]) + (tileGrid[oldX, oldY].height / 2 * dic["gridSize"])) * (y - oldY)),
                            index, tileWidth, tileHeight);

            t.entrance = new DoorWay();
            tileGrid[oldX, oldY].exit = new DoorWay();

            t.entrance.side = (Side)((int)d * (-1));
            t.entrance.connected = tileGrid[oldX, oldY].exit;
            t.entrance.position = GetDoorPos(t.entrance.side, t.width, t.height);
            t.previous = tileGrid[oldX, oldY];
            tileGrid[x, y] = t;

            tileGrid[oldX, oldY].exit.side = d;
            tileGrid[oldX, oldY].exit.connected = t.entrance;
            tileGrid[oldX, oldY].exit.position = GetDoorPos(tileGrid[oldX, oldY].exit.side, tileGrid[oldX, oldY].width, tileGrid[oldX, oldY].height);
            tileGrid[oldX, oldY].next = t;

            oldX = x;
            oldY = y;
        }

        /// <summary>
        /// Finds a random Door position respective to the side it was placed on.
        /// x and y position corresponds to the tile position that is used later in the WFC generation.
        /// </summary>
        /// <param name="side">The side the door is on</param>
        /// <param name="w">Width of the tile</param>
        /// <param name="h">Height of the tile</param>
        /// <returns></returns>
        Vector3Int GetDoorPos(Side side, int w, int h)
        {
            if (side == Side.DOWN)
                return new Vector3Int(Random.Range(1, w - 2), 0, 0);
            if (side == Side.LEFT)
                return new Vector3Int(0, Random.Range(1, h - 2), 0);
            if (side == Side.RIGHT)
                return new Vector3Int(w - 1, Random.Range(1, h - 2), 0);
            if (side == Side.UP)
                return new Vector3Int(Random.Range(1, w - 2), h - 1, 0);

            return Vector3Int.zero;
        }

        /// <summary>
        /// Finds a random direction.
        /// Each direction has a certain weight.
        /// </summary>
        /// <returns></returns>
        Directions GetRandomDirection()
        {
            Directions newD = new Directions();
            int oldweight = 0;
            int rDirection = Random.Range(0, totalWeight);
            foreach (Directions t in directions)
            {
                if (rDirection < t.weight)
                {
                    if (oldweight == t.weight)
                    {
                        if (Random.Range(0f, 1f) <= 0.5)
                        {
                            newD = t;
                            oldweight = t.weight;
                            break;
                        }
                    }
                    else
                    {
                        newD = t;
                        oldweight = t.weight;
                        break;
                    }
                }
                rDirection -= t.weight;
            }
            return newD;
        }
    }

    public class NonLinearGenerator
    {
        public GridTile[,] tileGrid;
        int oldX;
        int oldY;
        int height;
        int width;
        int length;
        int totalWeight;
        Directions[] directions;
        StringDictionary dic;

        public NonLinearGenerator(int _height, int _width, int _length, int _weight, Directions[] _directions, StringDictionary _dic)
        {
            totalWeight = _weight;
            directions = _directions;
            length = _length;
            dic = _dic;
        }

        public GridTile[,] GenerateGrid()
        {
            tileGrid = new GridTile[height, width];

            int directionAmount = Random.Range(0, 3);

            for (int i = 0; i < directionAmount; i++)
            {
                int armLength = Random.Range(0, 5);
                for (int j = 0; j < armLength; j++)
                {

                }
            }

            return tileGrid;
        }

        void AddNewTile(int x, int y, int index, Side d)
        {
            if (x < 0 || x >= tileGrid.GetLength(0) || y < 0 || y >= tileGrid.GetLength(1) || tileGrid[x, y] != null)
            {
                Directions dir = GetRandomDirection();
                switch (dir.direction)
                {
                    case Side.LEFT:
                        AddNewTile(oldX - 1, oldY, index, Side.LEFT);
                        break;
                    case Side.UP:
                        AddNewTile(oldX, oldY + 1, index, Side.UP);
                        break;
                    case Side.RIGHT:
                        AddNewTile(oldX + 1, oldY, index, Side.RIGHT);
                        break;
                    case Side.DOWN:
                        AddNewTile(oldX, oldY - 1, index, Side.DOWN);
                        break;
                }
                return;
            }

            int tileWidth = Random.Range(dic["minTileWidth"], dic["maxTileWidth"] + 1);
            int tileHeight = Random.Range(dic["minTileHeight"], dic["maxTileHeight"] + 1);

            GridTile t = new GridTile(tileGrid[oldX, oldY].position + new Vector3(((tileWidth / 2 + dic["tileDistance"]) + (tileGrid[oldX, oldY].width / 2)) * (x - oldX), 0, ((tileHeight / 2 + dic["tileDistance"]) + (tileGrid[oldX, oldY].height / 2)) * (y - oldY)),
                            index, tileWidth, tileHeight);
            t.entrance.side = d;
            tileGrid[oldX, oldY].exit.side = (Side)((int)d * (-1));
            tileGrid[x, y] = t;
            oldX = x;
            oldY = y;
        }

        Directions GetRandomDirection()
        {
            Directions newD = new Directions();
            int oldweight = 0;
            int rDirection = Random.Range(0, totalWeight);
            foreach (Directions t in directions)
            {
                if (rDirection < t.weight)
                {
                    if (oldweight == t.weight)
                    {
                        if (Random.Range(0f, 1f) <= 0.5)
                        {
                            newD = t;
                            oldweight = t.weight;
                        }
                    }
                    else
                    {
                        newD = t;
                        oldweight = t.weight;
                    }
                }
                rDirection -= t.weight;
            }
            return newD;
        }
    }

    public class LinearGenerator : MyGridGenerator
    {

        public LinearGenerator(int _height, int _width, int _length, int _weight, Directions[] _directions, StringDictionary _dic, GameObject gameOBJ) : base(_height, _width, _length, _weight, _directions, _dic, gameOBJ)
        {

        }

        /// <summary>
        /// Transfers the Tiles placed on the grid into an Array for ease of use.
        /// Tiles get placed at the array position corresponding to their own index.
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public GridTile[] TransferToArray(GridTile[,] grid)
        {
            GridTile[] l = new GridTile[length + 1];
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j] != null)
                        l[grid[i, j].index] = grid[i, j];
                }
            }
            return l;
        }

    }

}