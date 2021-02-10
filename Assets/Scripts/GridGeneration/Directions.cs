using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration
{
    public enum Side
    {
        UP = 1, DOWN = -1, LEFT = 2, RIGHT = -2
    }

    [System.Serializable]
    public class Directions
    {
        public Side direction;

        public int weight;
    }
}
