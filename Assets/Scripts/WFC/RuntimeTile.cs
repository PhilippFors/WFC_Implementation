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
        public RuntimeTile(int id, int r, bool sym = false, MyTile o = null, List<TileSide> sides = null)
        {
            ID = id;
            rotation = r;
            obj = o;
            this.sides = sides;
        }

        public MyTile obj;
        public List<TileSide> sides;
        public int ID;
        public int rotation;
        public bool symmetry;
    }
}