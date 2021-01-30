using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    UP = 1, DOWN = -1, LEFT = 2, RIGHT = -2
}

[System.Serializable]
public class Directions
{
    public Direction direction;

    public int weight;
}
