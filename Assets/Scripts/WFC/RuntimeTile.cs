using UnityEngine;
using System.Collections.Generic;
namespace MyWFC
{
    [System.Serializable]
    public class RuntimeTile
    {
        public RuntimeTile(int id, int r, int w = 1, bool sym = false, GameObject o = null, List<TileSide> sides = null)
        {
            ID = id;
            rotation = r;
            obj = o;
            weight = w;
            this.sides = sides;
        }
        public GameObject obj;
        public List<TileSide> sides;
        public int ID;
        public int rotation;
        public int weight;
        public bool symmetry;
    }
}