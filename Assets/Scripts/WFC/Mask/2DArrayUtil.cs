using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MyWFC
{
    [System.Serializable]
    public class IntArrayHelper
    {
        public IntArrayHelper(int size)
        {
            v = new int[size];
        }
        public int[] v;
    }

    [System.Serializable]
    public class BoolArrayHelper
    {
        public BoolArrayHelper(int size)
        {
            v = new bool[size];
        }

        public bool[] v;
    }
}
