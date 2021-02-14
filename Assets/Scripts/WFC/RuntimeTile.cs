using UnityEngine;
using System.Collections.Generic;
namespace MyWFC
{
    /// <summary>
    /// Used for creating unique tiles from a tileset with limited rotations during the WFC generation.
    /// </summary>
    [System.Serializable]
    public class RuntimeTile
    {
        public RuntimeTile(int id, int r, double w = 1, bool sym = false, GameObject o = null, List<TileSide> sides = null)
        {
            ID = id;
            rotation = r;
            obj = o;
            weight = w;
            this.sides = sides;
        }

        public RuntimeTile(int id, int bID, int r, double w = 1, bool sym = false, GameObject o = null, List<TileSide> sides = null)
        {
            ID = id;
            this.bID = bID;
            rotation = r;
            obj = o;
            weight = w;
            this.sides = sides;
        }

        public GameObject obj;
        public List<TileSide> sides;
        public int ID;
        public int bID = -1;
        public int rotation;
        public double weight;
        public bool symmetry;
    }
}