using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration
{
    [System.Serializable]
    public class DoorWay
    {
        public Side side;
        public Vector3Int position;
        public DoorWay connected;

        public DoorWay()
        {

        }

        public DoorWay(DoorWay connected, Side side)
        {

        }
    }
}