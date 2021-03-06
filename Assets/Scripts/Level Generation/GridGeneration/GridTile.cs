using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration.GridGeneration
{
    [System.Serializable]
    public class GridTile
    {
        public Vector3 position;
        public int index;
        public int width;
        public int height;
        public Bounds bounds;

        public GridTile previous;
        public GridTile next;

        public DoorWay entrance;
        public DoorWay exit;
        public GridTile(Vector3 pos, int _index, int _width, int _height)
        {
            width = _width;
            height = _height;
            position = pos;
            index = _index;
            bounds = new Bounds(pos, new Vector3(_width, 10, _height));
        }
    }
}