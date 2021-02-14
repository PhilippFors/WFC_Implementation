using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace MyWFC
{
    public class MyTile : MonoBehaviour
    {
        /// <summary>
        /// Unique ID for the tile
        /// </summary>
        public int ID = -1;

        //used for big tiles
        [HideInInspector] public int bID;
        [HideInInspector] public List<int> runtimeIDs = new List<int>();

        public bool use = true;

        /// <summary>
        /// The frequenzy of the tile placement
        /// </summary>
        public int weight;

        public int rotationDeg;
        public bool hasRotation;

        public Vector2Int coords;
        public Vector3Int size = new Vector3Int(1, 1, 1);
        public Vector3 center = Vector3.zero;
        public List<Cell> cells = new List<Cell>() { new Cell() };

        [SerializeField] bool showGizmo;
        [SerializeField] float gizmoSize = 0.5f;
        private void OnDrawGizmos()
        {

            if (ID == 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(size.x - 1f, size.y - 1f, size.z - 1f));
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            foreach (Cell c in cells)
                if (c.sides.Count > 0 && showGizmo)
                    foreach (TileSide s in c.sides)
                    {
                        switch (s.side)
                        {
                            case Sides.Left:
                                for (int i = 0; i < s.all.Count; i++)
                                {
                                    ConnectionColor(s.all[i]);
                                    Gizmos.DrawCube(new Vector3((float)(-size.x) / 2, 0, (1 - i) * (float)size.x / 3) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                }
                                // ConnectionColor(s.connection);
                                // Gizmos.DrawCube(new Vector3(-size.x / 2, 0, 0), Vector3.one / 4f);

                                break;
                            case Sides.Right:
                                for (int i = 0; i < s.all.Count; i++)
                                {
                                    ConnectionColor(s.all[i]);
                                    Gizmos.DrawCube(new Vector3((float)size.x / 2, 0, (-1 + i) * (float)size.x / 3) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                }
                                // ConnectionColor(s.connection);
                                // Gizmos.DrawCube(new Vector3(size.x / 2, 0, 0), Vector3.one / 4f);
                                break;
                            case Sides.Back:
                                for (int i = 0; i < s.all.Count; i++)
                                {
                                    ConnectionColor(s.all[i]);
                                    Gizmos.DrawCube(new Vector3((-1 + i) * (float)size.z / 3, 0, (float)(-size.z) / 2) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                }
                                // ConnectionColor(s.connection);
                                // Gizmos.DrawCube(new Vector3(0, 0, -size.z / 2), Vector3.one / 4f);
                                break;
                            case Sides.Front:
                                for (int i = 0; i < s.all.Count; i++)
                                {
                                    ConnectionColor(s.all[i]);
                                    Gizmos.DrawCube(new Vector3((1 - i) * (float)size.z / 3, 0, (float)size.z / 2) + c.center, new Vector3(gizmoSize, gizmoSize, gizmoSize));
                                }
                                // ConnectionColor(s.connection);
                                // Gizmos.DrawCube(new Vector3(0, 0, size.z / 2), Vector3.one / 4f);
                                break;
                        }
                    }
        }

        void ConnectionColor(Connections c)
        {
            switch (c)
            {
                case Connections.BIDYES:
                    Gizmos.color = new Color(0.5f, 0.5f, 0.5f);
                    break;
                case Connections.BID:
                    Gizmos.color = new Color(0.2f, 0.2f, 0.2f);
                    break;
                case Connections.A:
                    Gizmos.color = Color.red;
                    break;
                case Connections.B:
                    Gizmos.color = new Color(0.5f, 1f, 0.5f, 1);
                    break;
                case Connections.C:
                    Gizmos.color = Color.blue;
                    break;
                case Connections.D:
                    Gizmos.color = Color.black;
                    break;
                case Connections.E:
                    Gizmos.color = Color.cyan;
                    break;
                case Connections.F:
                    Gizmos.color = Color.green;
                    break;
                case Connections.G:
                    Gizmos.color = Color.gray;
                    break;
                case Connections.H:
                    Gizmos.color = Color.magenta;
                    break;
                case Connections.I:
                    Gizmos.color = Color.yellow;
                    break;
                case Connections.J:
                    Gizmos.color = Color.white;
                    break;
            }
        }
#endif
    }

    [System.Serializable]
    public class Cell
    {
        public Vector3 center = Vector3.zero;
        public TileSide side1 = new TileSide() { side = Sides.Right };
        public TileSide side2 = new TileSide() { side = Sides.Left };
        public TileSide side3 = new TileSide() { side = Sides.Front };
        public TileSide side4 = new TileSide() { side = Sides.Back };

        [HideInInspector]
        public List<TileSide> sides
        {
            get
            {
                List<TileSide> arr = new List<TileSide>();
                arr.Add(side1);
                arr.Add(side2);
                arr.Add(side3);
                arr.Add(side4);
                return arr;
            }
        }
    }
    [System.Serializable]
    public class TileSide
    {
        public Sides side;
        public Connections connection;

        public Connections left;
        public Connections middle;
        public Connections right;


        public List<Connections> all
        {
            get
            {
                List<Connections> l = new List<Connections>();
                l.Add(left);
                l.Add(middle);
                l.Add(right);
                return l;
            }
            set
            {
                left = value[0];
                middle = value[1];
                right = value[2];
            }
        }
        // public List<SubSide> subSides = new List<SubSide>(){new SubSide(){side = UnderSides.Lower, connection = Connections.A},
        //                                                     new SubSide(){side = UnderSides.Middle, connection = Connections.A},
        //                                                     new SubSide(){side = UnderSides.Upper, connection = Connections.A}};

        public override bool Equals(object obj)
        {
            TileSide otherS = (TileSide)obj;
            return otherS.left == left && otherS.right == right && otherS.middle == middle;
        }

        public bool Equals(Connections c)
        {
            return left == c && right == c && middle == c;
        }
    }
    // [System.Serializable]
    // public class SubSide
    // {
    //     public UnderSides side;
    //     public Connections connection;
    // }


    public enum Sides
    {
        Left = 180, Right = 0, Front = 270, Back = 90
    }
    public enum UnderSides
    {
        Upper, Lower, Middle
    }
    public enum Connections
    {
        BIDYES = -2,
        BID = -1,
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
        H = 7,
        I = 8,
        J = 9
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MyTile))]
    [CanEditMultipleObjects]
    public class TileEditor : Editor
    {
        bool init = false;
        public override void OnInspectorGUI()
        {
            MyTile t = (MyTile)target;
            DrawDefaultInspector();
            if (GUILayout.Button("Fill sides"))
            {
                foreach (Cell c in t.cells)
                {
                    c.side1 = new TileSide() { side = Sides.Left, connection = Connections.A };
                    c.side2 = new TileSide() { side = Sides.Right, connection = Connections.A };
                    c.side3 = new TileSide() { side = Sides.Front, connection = Connections.A };
                    c.side4 = new TileSide() { side = Sides.Back, connection = Connections.A };
                }
                init = true;
            }

            t.cells[0].center = t.center;

            if (GUI.changed)
                EditorUtility.SetDirty(t);
        }
    }
#endif
}
