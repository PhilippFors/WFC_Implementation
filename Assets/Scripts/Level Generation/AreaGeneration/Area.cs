using UnityEngine;

namespace LevelGeneration
{
    [System.Serializable]
    public class Area
    {
        public bool generated;
        public GameObject parent;
        public GridGeneration.GridTile tileInfo;
        public DoorWay entrance;
        public DoorWay exit;
        public Area(GameObject g, GridGeneration.GridTile tile)
        {
            parent = g;
            tileInfo = tile;
            parent.transform.position = tile.position;
            parent.tag = "Area";
        }
    }
}